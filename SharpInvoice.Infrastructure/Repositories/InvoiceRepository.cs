using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Enums;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Invoice entities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoiceRepository"/> class.
/// </remarks>
/// <param name="db">The database context.</param>
public class InvoiceRepository(AppDbContext db) : GenericRepository<Invoice>(db), IInvoiceRepository
{

    /// <summary>
    /// Gets an invoice by its unique identifier, including related items and transactions.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <returns>The invoice entity if found, null otherwise.</returns>
    public override async Task<Invoice?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id.ToString() == id.ToString(), cancellationToken);
    }

    /// <summary>
    /// Gets all invoices for a specific business with pagination support.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of invoices belonging to the specified business.</returns>
    public async Task<IEnumerable<Invoice>> GetByBusinessIdAsync(string businessId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.BusinessId.ToString() == businessId)
            .OrderByDescending(i => i.IssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all invoices for a specific client with pagination support.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of invoices for the specified client.</returns>
    public async Task<IEnumerable<Invoice>> GetByClientIdAsync(string clientId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.ClientId.ToString() == clientId)
            .OrderByDescending(i => i.IssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all invoices with a specific status for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="status">The invoice status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of invoices with the specified status.</returns>
    public async Task<IEnumerable<Invoice>> GetByStatusAsync(string businessId, InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.BusinessId.ToString() == businessId && i.Status == status)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if an invoice exists.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the invoice exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(i => i.Id.ToString() == id, cancellationToken);
    }

    /// <summary>
    /// Checks if an invoice number exists for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceNumber">The invoice number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the invoice number exists, false otherwise.</returns>
    public async Task<bool> InvoiceNumberExistsAsync(string businessId, string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(i => 
            i.BusinessId.ToString() == businessId && 
            i.InvoiceNumber == invoiceNumber, 
            cancellationToken);
    }

    /// <summary>
    /// Gets all overdue invoices.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of overdue invoices.</returns>
    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        
        return await DbSet
            .Where(i => i.DueDate < today && i.Status == InvoiceStatus.Sent)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the total revenue for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total revenue.</returns>
    public async Task<decimal> GetTotalRevenueAsync(string businessId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.BusinessId == businessId && i.Status == InvoiceStatus.Paid)
            .SumAsync(i => i.AmountPaid, cancellationToken);
    }

    /// <summary>
    /// Gets the total outstanding amount for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total outstanding amount.</returns>
    public async Task<decimal> GetTotalOutstandingAsync(string businessId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.BusinessId == businessId && 
                        (i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue))
            .SumAsync(i => i.Total - i.AmountPaid, cancellationToken);
    }
} 