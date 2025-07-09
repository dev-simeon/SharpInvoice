namespace SharpInvoice.Core.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

public sealed class User : AuditableEntity<Guid>
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; private init; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; private set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public string? AvatarUrl { get; private set; }
    public string? PhoneNumber { get; private set; }

    [Required]
    public string PasswordHash { get; private set; }

    public bool EmailConfirmed { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiry { get; private set; }

    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorCode { get; private set; }
    public DateTime? TwoFactorCodeExpiry { get; private set; }

    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = new List<ExternalLogin>();

    private User(Guid id, string email, string firstName, string lastName) : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = string.Empty;
    }

    public static User Create(string email, string firstName, string lastName, string passwordHash)
    {
        var user = new User(Guid.NewGuid(), email, firstName, lastName);
        user.SetPasswordHash(passwordHash);
        user.GenerateEmailConfirmationToken();

        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));

        return user;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
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

    public void GeneratePasswordResetToken()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        AddDomainEvent(new PasswordResetRequestedDomainEvent(Id, token));
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
        PasswordHash = newPasswordHash;
    }

    public void SetPasswordHash(string newPasswordHash) => PasswordHash = newPasswordHash;

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
        AddDomainEvent(new TwoFactorAuthenticationRequiredDomainEvent(Id));
    }

    public void ClearTwoFactorCode()
    {
        TwoFactorCode = null;
        TwoFactorCodeExpiry = null;
    }

    private User() { /* EF Core constructor */ }
}