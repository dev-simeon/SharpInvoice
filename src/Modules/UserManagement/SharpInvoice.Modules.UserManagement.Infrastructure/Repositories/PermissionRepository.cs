namespace SharpInvoice.Modules.UserManagement.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PermissionRepository(AppDbContext context) : IPermissionRepository
{
    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await context.Permissions.ToListAsync();
    }
}