using SharpInvoice.Core.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing Business entities.
/// </summary>
public interface IBusinessRepository : IRepository<Business>
{
    /// <summary>
    /// Gets a business by its ID.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>The business if found; otherwise, null.</returns>
    Task<Business?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets businesses by owner ID.
    /// </summary>
    /// <param name="ownerId">The owner's ID.</param>
    /// <returns>A collection of businesses.</returns>
    Task<IEnumerable<Business>> GetByOwnerIdAsync(string ownerId);
    
    /// <summary>
    /// Gets active businesses by owner ID.
    /// </summary>
    /// <param name="ownerId">The owner's ID.</param>
    /// <returns>A collection of active businesses.</returns>
    Task<IEnumerable<Business>> GetActiveBusinessesAsync(string ownerId);
    
    /// <summary>
    /// Checks if a business exists.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>True if the business exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string id);
    
    /// <summary>
    /// Gets a business by ID including deleted.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>The business if found; otherwise, null.</returns>
    Task<Business?> GetByIdIncludingDeletedAsync(string id);
    
    /// <summary>
    /// Checks if a deleted business can be restored.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>True if the business can be restored; otherwise, false.</returns>
    Task<bool> CanRestoreAsync(string id);
} 