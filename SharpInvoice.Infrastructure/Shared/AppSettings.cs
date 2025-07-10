namespace SharpInvoice.Infrastructure.Shared;

/// <summary>
/// Root class for application settings, mapped from appsettings.json.
/// </summary>
public class AppSettings
{
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
    /// <summary>Gets or sets the JWT (JSON Web Token) settings.</summary>
    public JwtSettings Jwt { get; set; } = new();
    
    /// <summary>Gets or sets the settings for the SMTP email service.</summary>
    public SmtpSettings Smtp { get; set; } = new();

    /// <summary>Gets or sets the settings for Azure Blob Storage.</summary>
    public AzureStorageSettings AzureStorage { get; set; } = new();

    /// <summary>Gets or sets the settings for external authentication providers.</summary>
    public AuthenticationSettings Authentication { get; set; } = new();
    
    /// <summary>Gets or sets the base URL of the application.</summary>
    public string AppUrl { get; set; } = string.Empty;
}

/// <summary>
/// Contains the database connection strings.
/// </summary>
public class ConnectionStringsSettings
{
    /// <summary>Gets or sets the connection string for the local SQL database.</summary>
    public string SqlDbLocal { get; set; } = string.Empty;
    /// <summary>Gets or sets the connection string for the remote SQL database.</summary>
    public string SqlDbRemote { get; set; } = string.Empty;
}

/// <summary>
/// Contains settings for JWT (JSON Web Token) generation and validation.
/// </summary>
public class JwtSettings
{
    /// <summary>Gets or sets the secret key used for signing the token.</summary>
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the issuer of the token.</summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the audience of the token.</summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the token's expiration time in minutes.</summary>
    public int ExpiryMinutes { get; set; }
}

/// <summary>
/// Contains settings for the SMTP email service.
/// </summary>
public class SmtpSettings
{
    /// <summary>Gets or sets the SMTP server host.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Gets or sets the SMTP server port.</summary>
    public int Port { get; set; }

    /// <summary>Gets or sets the username for SMTP authentication.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the password for SMTP authentication.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender's email address.</summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender's display name.</summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether to use SSL.</summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>Gets or sets the SendGrid template IDs.</summary>
    public TemplateSettings Templates { get; set; } = new();
}

/// <summary>
/// Contains SendGrid template IDs for various email types.
/// </summary>
public class TemplateSettings
{
    public string EmailConfirmation { get; set; } = string.Empty;
    public string PasswordReset { get; set; } = string.Empty;
    public string Invitation { get; set; } = string.Empty;
    public string Invoice { get; set; } = string.Empty;
}

/// <summary>
/// Contains settings for Azure Blob Storage.
/// </summary>
public class AzureStorageSettings
{
    /// <summary>Gets or sets the connection string for the Azure Storage account.</summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the name of the blob container.</summary>
    public string ContainerName { get; set; } = "invoices";
}

/// <summary>
/// Contains settings for external authentication providers like Google and Facebook.
/// </summary>
public class AuthenticationSettings
{
    /// <summary>Gets or sets the Google authentication settings.</summary>
    public GoogleAuthSettings Google { get; set; } = new();
    /// <summary>Gets or sets the Facebook authentication settings.</summary>
    public FacebookAuthSettings Facebook { get; set; } = new();
}

/// <summary>
/// Defines the settings for Google's OAuth 2.0 authentication.
/// </summary>
public class GoogleAuthSettings
{
    /// <summary>Gets or sets the Google API client ID.</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>Gets or sets the Google API client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Defines the settings for Facebook's OAuth 2.0 authentication.
/// </summary>
public class FacebookAuthSettings
{
    /// <summary>Gets or sets the Facebook application ID.</summary>
    public string AppId { get; set; } = string.Empty;
    /// <summary>Gets or sets the Facebook application secret.</summary>
    public string AppSecret { get; set; } = string.Empty;
} 