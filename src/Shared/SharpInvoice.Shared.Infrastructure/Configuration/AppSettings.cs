namespace SharpInvoice.Shared.Infrastructure.Configuration;

/// <summary>
/// Represents the application's configuration settings, binding values from appsettings.json.
/// </summary>
public class AppSettings
{
    /// <summary>Gets or sets the database connection strings.</summary>
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
    /// <summary>Gets or sets the JWT (JSON Web Token) settings.</summary>
    public JwtSettings Jwt { get; set; } = new();
    /// <summary>Gets or sets the settings for external authentication providers.</summary>
    public AuthenticationSettings Authentication { get; set; } = new();
    /// <summary>Gets or sets the settings for the SendGrid email service.</summary>
    public SendGridSettings SendGrid { get; set; } = new();
    /// <summary>Gets or sets the base URL of the application.</summary>
    public string AppUrl { get; set; } = "https://sharpinvoice.com";
    /// <summary>Gets or sets the security settings for the application.</summary>
    public SecuritySettings Security { get; set; } = new();
}

/// <summary>
/// Contains the database connection strings for the application.
/// </summary>
public class ConnectionStringsSettings
{
    /// <summary>Gets or sets the connection string for the local SQL database.</summary>
    public string SqlDbLocal { get; set; } = string.Empty;
    /// <summary>Gets or sets the connection string for the remote SQL database.</summary>
    public string SqlDbRemote { get; set; } = string.Empty;
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

/// <summary>
/// Security settings for the application
/// </summary>
public class SecuritySettings
{
    public string PasswordPepper { get; set; } = string.Empty;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutTimeInMinutes { get; set; } = 15;
}