using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Enums;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing Role entities.
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Gets a role by its ID.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <returns>The role if found; otherwise, null.</returns>
    Task<Role?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>The role if found; otherwise, null.</returns>
    Task<Role?> GetByNameAsync(BusinessRole name);
    
    /// <summary>
    /// Gets all roles.
    /// </summary>
    /// <returns>A collection of all roles.</returns>
    Task<IEnumerable<Role>> GetAllAsync();
    
    /// <summary>
    /// Checks if a role exists.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <returns>True if the role exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string id);
} 