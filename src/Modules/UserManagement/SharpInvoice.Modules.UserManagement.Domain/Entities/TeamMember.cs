namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Kernel.Domain;

public sealed class TeamMember : AuditableEntity<Guid>
{
    public Guid UserId { get; private init; }
    public User User { get; private init; } = null!;
    public Guid BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public Guid RoleId { get; private set; }
    public Role Role { get; } = null!;

    internal TeamMember(Guid userId, Guid businessId, Guid roleId)
    {
        UserId = userId; BusinessId = businessId; RoleId = roleId;
    }
    public static TeamMember Create(Guid userId, Guid businessId, Guid roleId)
        => new(userId, businessId, roleId);
    public void UpdateRole(Guid newRoleId) => RoleId = newRoleId;
    private TeamMember() { Role = null!; } // EF Core
}