using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Business entities.
/// </summary>
public class BusinessRepository(AppDbContext db) : BaseRepository<Business>(db), IBusinessRepository
{

    /// <summary>
    /// Gets a business by its unique identifier.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>The business entity if found, null otherwise.</returns>
    public async Task<Business?> GetByIdAsync(string id)
    {
        return await DbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets all businesses for a specific owner.
    /// </summary>
    /// <param name="ownerId">The owner's ID.</param>
    /// <returns>A collection of businesses owned by the specified user.</returns>
    public async Task<IEnumerable<Business>> GetByOwnerIdAsync(string ownerId)
    {
        return await DbSet
            .Where(b => b.OwnerId == ownerId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active businesses for a specific owner.
    /// </summary>
    /// <param name="ownerId">The owner's ID.</param>
    /// <returns>A collection of active businesses owned by the specified user.</returns>
    public async Task<IEnumerable<Business>> GetActiveBusinessesAsync(string ownerId)
    {
        return await DbSet
            .Where(b => b.OwnerId == ownerId && b.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a business with the specified ID exists.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>True if the business exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(b => b.Id == id);
    }

    /// <summary>
    /// Gets a business by its ID, including those marked as deleted.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>The business entity if found, null otherwise.</returns>
    public async Task<Business?> GetByIdIncludingDeletedAsync(string id)
    {
        // Temporarily disable the global query filter
        return await Context.Businesses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <summary>
    /// Checks if a deleted business can be restored.
    /// </summary>
    /// <param name="id">The business ID.</param>
    /// <returns>True if the business can be restored, false otherwise.</returns>
    public async Task<bool> CanRestoreAsync(string id)
    {
        // Check if there's another active business with the same name and country
        var deletedBusiness = await GetByIdIncludingDeletedAsync(id);
        if (deletedBusiness == null)
            return false;

        return !await DbSet
            .AnyAsync(b => !b.IsDeleted && 
                     b.Name == deletedBusiness.Name && 
                     b.Country == deletedBusiness.Country);
    }
} 