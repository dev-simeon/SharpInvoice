namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Security.Cryptography;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;

public class User : BaseEntity
{
    public User(string email, string firstName, string lastName, string phoneNumber, string passwordHash, string emailToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));

        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        EmailConfirmationToken = emailToken;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdateContactInfo(string? avatarUrl, string? phoneNumber)
    {
        AvatarUrl = avatarUrl;
        PhoneNumber = phoneNumber;
    }

    public string GenerateRefreshToken()
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        return refreshToken;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }

    public static void GeneratePasswordResetToken()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiryTime = expiry;
    }

    public void GenerateEmailConfirmationToken()
    {
        EmailConfirmationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiry = null;
    }

    public void ResetPassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }

    public void SetPasswordHash(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }

    public void EnableTwoFactor() => TwoFactorEnabled = true;

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        ClearTwoFactorCode();
    }

    public void GenerateTwoFactorCode()
    {
        if (!TwoFactorEnabled) return;
        TwoFactorCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString("D6");
        TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(10);
    }

    public void ClearTwoFactorCode()
    {
        TwoFactorCode = null;
        TwoFactorCodeExpiry = null;
    }

    public int Id { get; private init; }
    public string Email { get; private init; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public ApplicationUserRole ApplicationUserRole { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    public string? AvatarUrl { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiry { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorCode { get; private set; }
    public DateTime? TwoFactorCodeExpiry { get; private set; }
    //public ICollection<ExternalLogin> ExternalLogins { get; private set; } = new List<ExternalLogin>();

    private User() { }
}