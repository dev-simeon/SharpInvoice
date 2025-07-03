namespace SharpInvoice.Modules.Auth.Application.Dtos;

using System.Security.Claims;

/// <summary>
/// Contains information about an external login provider.
/// </summary>
public class ExternalLoginInfo(ClaimsPrincipal principal, string loginProvider, string providerKey, string providerDisplayName)
{
    /// <summary>
    /// Gets the claims principal from the external login provider.
    /// </summary>
    public ClaimsPrincipal Principal { get; } = principal;
    
    /// <summary>
    /// Gets the name of the external login provider (e.g., "Google", "Facebook").
    /// </summary>
    public string LoginProvider { get; } = loginProvider;
    
    /// <summary>
    /// Gets the unique identifier for the user in the external provider's system.
    /// </summary>
    public string ProviderKey { get; } = providerKey;
    
    /// <summary>
    /// Gets the friendly name of the external provider.
    /// </summary>
    public string ProviderDisplayName { get; } = providerDisplayName;
} 