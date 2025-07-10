namespace SharpInvoice.Core.Domain.Entities;

public class ExternalLogin
{
    // Properties
    public string LoginProvider { get; private init; }
    public string ProviderKey { get; private init; }
    public string UserId { get; private init; }
    public User User { get; private set; } = null!;
    
    // Constructor
    public ExternalLogin(string userId, string loginProvider, string providerKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loginProvider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        UserId = userId;
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
    }
} 