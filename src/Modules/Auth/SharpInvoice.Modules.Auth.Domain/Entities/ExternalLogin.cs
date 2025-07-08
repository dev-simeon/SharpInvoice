namespace SharpInvoice.Modules.Auth.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using System.ComponentModel.DataAnnotations;

public sealed class ExternalLogin : AuditableEntity<Guid>
{
    [Required] public Guid UserId { get; private init; }
    public User User { get; private init; } = null!;
    [Required][MaxLength(128)] public string LoginProvider { get; private init; }
    [Required][MaxLength(256)] public string ProviderKey { get; private init; }

    private ExternalLogin(Guid id, Guid userId, string loginProvider, string providerKey) : base(id)
    {
        UserId = userId; LoginProvider = loginProvider; ProviderKey = providerKey;
    }
    public static ExternalLogin Create(Guid userId, string loginProvider, string providerKey) => new(Guid.NewGuid(), userId, loginProvider, providerKey);
    private ExternalLogin() { LoginProvider = string.Empty; ProviderKey = string.Empty; } // EF Core
}