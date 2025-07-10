using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Role entities.
/// </summary>
public class RoleRepository(AppDbContext db) : BaseRepository<Role>(db), IRoleRepository
{
    /// <summary>
    /// Gets a role by its unique identifier.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <returns>The role entity if found, null otherwise.</returns>
    public async Task<Role?> GetByIdAsync(string id)
    {
        return await DbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>The role entity if found, null otherwise.</returns>
    public async Task<Role?> GetByNameAsync(BusinessRole name)
    {
        return await DbSet.FirstOrDefaultAsync(r => r.Name == name);
    }

    /// <summary>
    /// Gets all roles.
    /// </summary>
    /// <returns>A collection of all roles.</returns>
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await DbSet.OrderBy(r => (int)r.Name).ToListAsync();
    }

    /// <summary>
    /// Checks if a role with the specified ID exists.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <returns>True if the role exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(r => r.Id == id);
    }
} 