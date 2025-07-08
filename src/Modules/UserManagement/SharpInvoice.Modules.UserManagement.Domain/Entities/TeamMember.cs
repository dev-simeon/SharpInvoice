namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Kernel.Domain;
using SharpInvoice.Shared.Kernel.Exceptions;
using SharpInvoice.Modules.UserManagement.Domain.Events;

public sealed class TeamMember : AuditableEntity<Guid>
{
    public Guid UserId { get; private init; }
    public User User { get; private init; } = null!;
    public Guid BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public Guid RoleId { get; private set; }
    public Role Role { get; } = null!;

    private TeamMember(Guid id, Guid userId, Guid businessId, Guid roleId) : base(id)
    {
        UserId = userId;
        BusinessId = businessId;
        RoleId = roleId;
    }

    public static TeamMember Create(Guid userId, Guid businessId, Guid roleId)
    {
        // Further validation could be added here if needed,
        // such as checking if the user is already a member of another business.
        var teamMember = new TeamMember(Guid.NewGuid(), userId, businessId, roleId);
        teamMember.AddDomainEvent(new TeamMemberAddedDomainEvent(teamMember.Id));
        return teamMember;
    }

    public void UpdateRole(Guid newRoleId)
    {
        RoleId = newRoleId;
    }

    private TeamMember() { Role = null!; } // EF Core
}