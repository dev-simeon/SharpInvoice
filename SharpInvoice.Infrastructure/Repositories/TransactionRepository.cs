using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Transaction entities.
/// </summary>
public class TransactionRepository(AppDbContext db) : BaseRepository<Transaction>(db), ITransactionRepository
{
    /// <summary>
    /// Gets a transaction by its unique identifier.
    /// </summary>
    /// <param name="id">The transaction ID.</param>
    /// <returns>The transaction entity if found, null otherwise.</returns>
    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await DbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets all transactions for a specific invoice.
    /// </summary>
    /// <param name="invoiceId">The invoice ID.</param>
    /// <returns>A collection of transactions for the specified invoice.</returns>
    public async Task<IEnumerable<Transaction>> GetByInvoiceIdAsync(string invoiceId)
    {
        return await DbSet
            .Where(t => t.InvoiceId == invoiceId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a transaction with the specified ID exists.
    /// </summary>
    /// <param name="id">The transaction ID.</param>
    /// <returns>True if the transaction exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(t => t.Id == id);
    }

    /// <summary>
    /// Checks if a transaction with the specified external ID exists.
    /// </summary>
    /// <param name="externalTransactionId">The external transaction ID.</param>
    /// <returns>True if a transaction with the external ID exists, false otherwise.</returns>
    public async Task<bool> ExternalTransactionIdExistsAsync(string externalTransactionId)
    {
        if (string.IsNullOrEmpty(externalTransactionId))
            return false;
            
        return await DbSet
            .AnyAsync(t => t.ExternalTransactionId == externalTransactionId);
    }
} 