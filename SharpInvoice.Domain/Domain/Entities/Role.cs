namespace SharpInvoice.Core.Domain.Entities;

using System;
using System.Collections.Generic;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;

public sealed class Role : BaseEntity
{
    private Role(BusinessRole name, string? description) 
    {
        Name = name;
        Description = description;
    }
    public static Role Create(BusinessRole name, string? description)
    {
        return new Role(name, description);
    }

    public void UpdateDescription(string? description) => Description = description;

    public Guid Id { get; private set; }
    public BusinessRole Name { get; private set; }
    public string? Description { get; private set; }

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];

    private Role() { }
}