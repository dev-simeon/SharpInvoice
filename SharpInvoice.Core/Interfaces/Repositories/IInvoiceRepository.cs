using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Enums;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing Invoice entities.
/// </summary>
public interface IInvoiceRepository : IRepository<Invoice>
{
    /// <summary>
    /// Gets all invoices for a specific business with pagination support.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of invoices belonging to the specified business.</returns>
    Task<IEnumerable<Invoice>> GetByBusinessIdAsync(string businessId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all invoices for a specific client with pagination support.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of invoices for the specified client.</returns>
    Task<IEnumerable<Invoice>> GetByClientIdAsync(string clientId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all invoices with a specific status for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="status">The invoice status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of invoices with the specified status.</returns>
    Task<IEnumerable<Invoice>> GetByStatusAsync(string businessId, InvoiceStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an invoice exists.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the invoice exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an invoice number exists for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceNumber">The invoice number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the invoice number exists; otherwise, false.</returns>
    Task<bool> InvoiceNumberExistsAsync(string businessId, string invoiceNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all overdue invoices.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of overdue invoices.</returns>
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total revenue for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total revenue.</returns>
    Task<decimal> GetTotalRevenueAsync(string businessId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the total outstanding amount for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total outstanding amount.</returns>
    Task<decimal> GetTotalOutstandingAsync(string businessId, CancellationToken cancellationToken = default);
} 