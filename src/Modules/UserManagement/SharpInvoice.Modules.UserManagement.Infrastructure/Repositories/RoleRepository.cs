namespace SharpInvoice.Modules.UserManagement.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

public class RoleRepository(AppDbContext context) : IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<Role?> GetByIdAsync(Guid roleId)
    {
        return await context.Roles.FindAsync(roleId);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid roleId)
    {
        return await context.Roles
            .Include(r => r.Permissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task AddAsync(Role role)
    {
        await context.Roles.AddAsync(role);
    }
}