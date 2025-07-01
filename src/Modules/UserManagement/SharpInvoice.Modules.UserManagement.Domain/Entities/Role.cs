namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

using SharpInvoice.Shared.Kernel.Domain;
using System.ComponentModel.DataAnnotations;

public sealed class Role : AuditableEntity<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; private init; }

    [MaxLength(255)]
    public string? Description { get; private set; }

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public ICollection<TeamMember> TeamMembers { get; private set; } = [];

    private Role(Guid id, string name, string? description) : base(id)
    {
        Name = name; Description = description;
    }
    public static Role Create(string name, string? description) => new(Guid.NewGuid(), name, description);
    public void UpdateDescription(string? description) => Description = description;
    public void AddPermission(Permission permission)
    {
        if (!_rolePermissions.Any(rp => rp.PermissionId == permission.Id))
        {
            _rolePermissions.Add(new RolePermission(Id, permission.Id));
        }
    }
    public void RemovePermission(Permission permission)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
    }
    private Role() { Name = string.Empty; } // EF Core
}