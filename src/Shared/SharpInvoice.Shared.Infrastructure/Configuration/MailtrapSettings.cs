namespace SharpInvoice.Shared.Infrastructure.Configuration;

/// <summary>
/// Defines the settings for the Mailtrap email service.
/// </summary>
public record MailtrapSettings
{
    public const string SectionName = "Mailtrap";
    public string ApiKey { get; init; } = string.Empty;
    public string InboxId { get; init; } = string.Empty;
}