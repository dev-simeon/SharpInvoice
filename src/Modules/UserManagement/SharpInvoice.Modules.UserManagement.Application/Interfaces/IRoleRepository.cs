namespace SharpInvoice.Modules.UserManagement.Application.Interfaces;

using SharpInvoice.Modules.UserManagement.Domain.Entities;
using System;
using System.Threading.Tasks;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
    Task<Role?> GetByIdAsync(Guid roleId);
    Task<Role?> GetByIdWithPermissionsAsync(Guid roleId);
    Task AddAsync(Role role);
}