namespace SharpInvoice.Core.Domain.Entities;

using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Domain.Shared;

public sealed class Role : BaseEntity
{
    // Properties
    public string Id { get; private init; }
    public BusinessRole Name { get; private set; }
    public string? Description { get; private set; }

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];

    // Constructor
    public Role(BusinessRole name, string? description = null)
    {
        Id = KeyGenerator.Generate("role", name.ToString());
        Name = name;
        Description = description;
    }

    // Methods
    public void UpdateDescription(string? description) => Description = description;
}