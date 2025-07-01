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
} 