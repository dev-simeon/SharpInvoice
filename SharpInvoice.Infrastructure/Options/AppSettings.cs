namespace SharpInvoice.Infrastructure.Options;

/// <summary>
/// Represents the application settings loaded from configuration.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the JWT configuration.
    /// </summary>
    public JwtSettings Jwt { get; set; } = new JwtSettings();

    /// <summary>
    /// Gets or sets the SMTP configuration.
    /// </summary>
    public SmtpSettings Smtp { get; set; } = new SmtpSettings();

    /// <summary>
    /// Gets or sets the authentication configuration.
    /// </summary>
    public AuthenticationSettings Authentication { get; set; } = new AuthenticationSettings();

    /// <summary>
    /// Gets or sets the Azure Storage configuration.
    /// </summary>
    public AzureStorageSettings AzureStorage { get; set; } = new AzureStorageSettings();

    /// <summary>
    /// Gets or sets the application URL.
    /// </summary>
    public string AppUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret key.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SendGrid API key.
    /// </summary>
    public string SendGridKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the security settings.
    /// </summary>
    public SecuritySettings Security { get; set; } = new SecuritySettings();

    /// <summary>
    /// Gets or sets the allowed CORS origins.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}

/// <summary>
/// Represents JWT configuration settings.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Gets or sets the JWT secret.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT expiry in minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the JWT refresh token expiry in days.
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

/// <summary>
/// Represents SMTP configuration settings.
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// Gets or sets the SMTP host.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP port.
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP from address.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP from name.
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL.
    /// </summary>
    public bool UseSsl { get; set; } = true;
}

/// <summary>
/// Represents authentication configuration settings.
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Gets or sets the Google authentication settings.
    /// </summary>
    public ExternalProviderSettings Google { get; set; } = new ExternalProviderSettings();

    /// <summary>
    /// Gets or sets the Facebook authentication settings.
    /// </summary>
    public ExternalProviderSettings Facebook { get; set; } = new ExternalProviderSettings();
}

/// <summary>
/// Represents external provider configuration settings.
/// </summary>
public class ExternalProviderSettings
{
    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Represents Azure Storage configuration settings.
/// </summary>
public class AzureStorageSettings
{
    /// <summary>
    /// Gets or sets the Azure Storage connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure Storage container name.
    /// </summary>
    public string ContainerName { get; set; } = string.Empty;
}

/// <summary>
/// Represents security configuration settings.
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Gets or sets the BCrypt work factor.
    /// </summary>
    public int BcryptWorkFactor { get; set; } = 12;

    /// <summary>
    /// Gets or sets the password reset token expiry in minutes.
    /// </summary>
    public int PasswordResetTokenExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the email confirmation token expiry in hours.
    /// </summary>
    public int EmailConfirmationTokenExpiryHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the team invitation token expiry in hours.
    /// </summary>
    public int TeamInvitationExpiryHours { get; set; } = 72;
} 