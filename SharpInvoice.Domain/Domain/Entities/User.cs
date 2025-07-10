namespace SharpInvoice.Core.Domain.Entities;

using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;
using System.Security.Cryptography;

public class User : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public string Email { get; private set; }
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

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];
    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = [];

    // Factory & Constructor
    public static User Register(string email, string firstName, string lastName, string passwordHash, string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(email, firstName, lastName, passwordHash, ApplicationUserRole.User, phoneNumber);
    }

    public static User CreateApplicationUser(string email, string firstName, string lastName, string passwordHash, ApplicationUserRole role, string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        if (role == ApplicationUserRole.User)
            throw new ArgumentException("Cannot create an application user with the default 'USER' role. Use Register() instead.", nameof(role));

        return new User(email, firstName, lastName, passwordHash, role, phoneNumber);
    }

    private User(string email, string firstName, string lastName, string passwordHash, ApplicationUserRole role, string? phoneNumber = null)
    {
        Id = KeyGenerator.Generate("user", $"{firstName}-{lastName}");
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber;
        ApplicationUserRole = role;
    }

    // Methods
    public void UpdateProfile(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
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
}