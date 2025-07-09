namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using SharpInvoice.Core.Domain.Shared;

public sealed class Role : AuditableEntity<Guid>
{
    private Role(Guid id, string name, string? description) : base(id)
    {
        Name = name;
        Description = description;
    }
    public static Role Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));
        return new Role(Guid.NewGuid(), name, description);
    }

    public void UpdateDescription(string? description) => Description = description;

    public string Name { get; private set; }
    public string? Description { get; private set; }

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];

    private Role() { Name = string.Empty; }
}