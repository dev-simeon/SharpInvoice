namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using System.ComponentModel.DataAnnotations;

public sealed class Role : AuditableEntity<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; private set; }

    [MaxLength(500)]
    public string? Description { get; private set; }

    private readonly List<RolePermission> _permissions = new();
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];

    private Role(Guid id, string name, string? description) : base(id)
    {
        Name = name; Description = description;
    }
    public static Role Create(string name, string? description) => new(Guid.NewGuid(), name, description);
    public void UpdateDescription(string? description) => Description = description;
    public void AddPermission(Permission permission)
    {
        if (!_permissions.Any(rp => rp.PermissionId == permission.Id))
        {
            _permissions.Add(new RolePermission(Id, permission.Id));
        }
    }
    public void RemovePermission(Permission permission)
    {
        var rolePermission = _permissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
        if (rolePermission != null)
        {
            _permissions.Remove(rolePermission);
        }
    }
    private Role() { Name = string.Empty; } // EF Core
}