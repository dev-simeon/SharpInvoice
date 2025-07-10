namespace SharpInvoice.Core.Domain.Entities;

using System;

public sealed class TeamMember
{
    // Properties
    public string UserId { get; private init; }
    public User User { get; private init; } = null!;
    public string BusinessId { get; private init; }
    public Business Business { get; private init; } = null!;
    public string RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public DateTime CreatedAt { get; private init; }

    // Constructor
    public TeamMember(string userId, string businessId, string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        UserId = userId;
        BusinessId = businessId;
        RoleId = roleId;
        CreatedAt = DateTime.UtcNow;
    }

    // Methods
    public void UpdateRole(string newRoleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newRoleId);
        RoleId = newRoleId;
    }
}