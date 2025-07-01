namespace SharpInvoice.Modules.UserManagement.Domain.Entities;

public sealed class RolePermission
{
    public Guid RoleId { get; private init; }
    public Role Role { get; private init; } = null!;

    public Guid PermissionId { get; private init; }
    public Permission Permission { get; private init; } = null!;

    internal RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId; PermissionId = permissionId;
    }
    private RolePermission() { } // EF Core
}