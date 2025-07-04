namespace SharpInvoice.Shared.Infrastructure.Configuration;

/// <summary>
/// Defines the settings for the SendGrid email service.
/// </summary>
public record SendGridSettings
{
    public const string SectionName = "SendGrid";
    public string ApiKey { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    
    // SMTP Settings
    public string SmtpHost { get; init; } = "smtp.sendgrid.net";
    public int SmtpPort { get; init; } = 587;
    public string SmtpUsername { get; init; } = "apikey"; // For SendGrid, this is always "apikey"
} 