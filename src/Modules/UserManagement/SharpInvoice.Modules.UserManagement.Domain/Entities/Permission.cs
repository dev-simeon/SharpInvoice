namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using System.ComponentModel.DataAnnotations;

public sealed class Permission : Entity<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; private init; }

    [MaxLength(255)]
    public string Description { get; private init; }

    private Permission(Guid id, string name, string description) : base(id)
    {
        Name = name; Description = description;
    }
    public static Permission Create(string name, string description) => new(Guid.NewGuid(), name, description);
    private Permission() { Name = string.Empty; Description = string.Empty; } // EF Core
}