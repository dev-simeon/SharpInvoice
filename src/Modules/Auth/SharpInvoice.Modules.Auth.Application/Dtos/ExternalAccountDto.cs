namespace SharpInvoice.Modules.Auth.Application.Dtos;

/// <summary>
/// Data transfer object for external account information
/// </summary>
/// <remarks>
/// Creates a new instance of ExternalAccountDto
/// </remarks>
public class ExternalAccountDto(string provider, string displayName, string email, DateTime linkedOn)
{
    /// <summary>
    /// Gets the external authentication provider (e.g. "Google", "Facebook")
    /// </summary>
    public string Provider { get; } = provider;

    /// <summary>
    /// Gets the display name for the external account
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// Gets the account email from the external provider
    /// </summary>
    public string Email { get; } = email;

    /// <summary>
    /// Gets the date when the account was linked
    /// </summary>
    public DateTime LinkedOn { get; } = linkedOn;
}