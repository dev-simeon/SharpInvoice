namespace SharpInvoice.Core.Domain.Entities;

using System;
using SharpInvoice.Core.Domain.Shared;

public sealed class TeamMember : AuditableEntity<Guid>
{
    private TeamMember(Guid id, Guid userId, Guid businessId, Guid roleId) : base(id)
    {
        UserId = userId;
        BusinessId = businessId;
        RoleId = roleId;
    }

    public static TeamMember Create(Guid userId, Guid businessId, Guid roleId)
    {
        return new TeamMember(Guid.NewGuid(), userId, businessId, roleId);
    }

    public void UpdateRole(Guid newRoleId)
    {
        RoleId = newRoleId;
    }

    public Guid UserId { get; private init; }
    public User User { get; private init; } = null!;
    public Guid BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public Guid RoleId { get; private set; }
    public Role Role { get; } = null!;

    private TeamMember() { Role = null!; }
}