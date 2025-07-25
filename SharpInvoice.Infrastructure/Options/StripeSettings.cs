namespace SharpInvoice.Infrastructure.Options;

/// <summary>
/// Represents Stripe payment gateway configuration settings.
/// </summary>
public class StripeSettings
{
    /// <summary>
    /// Gets or sets the Stripe secret key.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Stripe publishable key.
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Stripe webhook secret.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
} 