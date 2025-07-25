using SharpInvoice.Core.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing Transaction entities.
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Gets a transaction by its ID.
    /// </summary>
    /// <param name="id">The transaction ID.</param>
    /// <returns>The transaction if found; otherwise, null.</returns>
    Task<Transaction?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all transactions for a specific invoice.
    /// </summary>
    /// <param name="invoiceId">The invoice ID.</param>
    /// <returns>A collection of transactions.</returns>
    Task<IEnumerable<Transaction>> GetByInvoiceIdAsync(Guid invoiceId);
    
    /// <summary>
    /// Checks if a transaction exists.
    /// </summary>
    /// <param name="id">The transaction ID.</param>
    /// <returns>True if the transaction exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if a transaction with the specified external ID exists.
    /// </summary>
    /// <param name="externalTransactionId">The external transaction ID.</param>
    /// <returns>True if a transaction with the external ID exists; otherwise, false.</returns>
    Task<bool> ExternalTransactionIdExistsAsync(string externalTransactionId);
} 