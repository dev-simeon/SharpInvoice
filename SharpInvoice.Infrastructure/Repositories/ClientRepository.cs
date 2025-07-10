using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Client entities.
/// </summary>
public class ClientRepository(AppDbContext db) : BaseRepository<Client>(db), IClientRepository
{
    /// <summary>
    /// Gets a client by its unique identifier.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>The client entity if found, null otherwise.</returns>
    public async Task<Client?> GetByIdAsync(string id)
    {
        return await DbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets all clients for a specific business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <returns>A collection of clients belonging to the specified business.</returns>
    public async Task<IEnumerable<Client>> GetByBusinessIdAsync(string businessId)
    {
        return await DbSet
            .Where(c => c.BusinessId == businessId)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a client with the specified ID exists.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>True if the client exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(c => c.Id == id);
    }

    /// <summary>
    /// Checks if a client has any invoices.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>True if the client has invoices, false otherwise.</returns>
    public async Task<bool> HasInvoicesAsync(string id)
    {
        return await Context.Invoices.AnyAsync(i => i.ClientId == id);
    }

    /// <summary>
    /// Gets a client by email and business ID.
    /// </summary>
    /// <param name="email">The client email.</param>
    /// <param name="businessId">The business ID.</param>
    /// <returns>The client entity if found, null otherwise.</returns>
    public async Task<Client?> GetByEmailAndBusinessIdAsync(string email, string businessId)
    {
        if (string.IsNullOrEmpty(email))
            return null;
            
        return await DbSet
            .FirstOrDefaultAsync(c => c.Email != null && 
                                c.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase) && 
                                c.BusinessId == businessId);
    }
} 