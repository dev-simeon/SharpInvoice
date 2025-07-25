namespace SharpInvoice.Core.Entities;

using SharpInvoice.Core.Domain.Shared;
using SharpInvoice.Core.Enums;
using SharpInvoice.Core.Shared;

public sealed class Role(BusinessRole name, string? description = null) : BaseEntity
{
    // Properties
    public string Id { get; private init; } = KeyGenerator.Generate("role", name.ToString());
    public BusinessRole Name { get; private set; } = name;
    public string? Description { get; private set; } = description;

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];

    // Methods
    public void UpdateDescription(string? description) => Description = description;
}