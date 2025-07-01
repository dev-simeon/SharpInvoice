namespace SharpInvoice.Modules.Auth.Application.Dtos;

using System.Security.Claims;

public class ExternalLoginInfo(ClaimsPrincipal principal, string loginProvider, string providerKey, string providerDisplayName)
{
    public ClaimsPrincipal Principal { get; } = principal;
    public string LoginProvider { get; } = loginProvider;
    public string ProviderKey { get; } = providerKey;
    public string ProviderDisplayName { get; } = providerDisplayName;
} 