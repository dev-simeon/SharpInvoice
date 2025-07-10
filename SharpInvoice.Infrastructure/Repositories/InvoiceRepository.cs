using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Invoice entities.
/// </summary>
public class InvoiceRepository(AppDbContext db) : BaseRepository<Invoice>(db), IInvoiceRepository
{
    /// <summary>
    /// Gets an invoice by its unique identifier.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <returns>The invoice entity if found, null otherwise.</returns>
    public async Task<Invoice?> GetByIdAsync(string id)
    {
        return await DbSet
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    /// <summary>
    /// Gets all invoices for a specific business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <returns>A collection of invoices belonging to the specified business.</returns>
    public async Task<IEnumerable<Invoice>> GetByBusinessIdAsync(string businessId)
    {
        return await DbSet
            .Where(i => i.BusinessId == businessId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all invoices for a specific client.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <returns>A collection of invoices for the specified client.</returns>
    public async Task<IEnumerable<Invoice>> GetByClientIdAsync(string clientId)
    {
        return await DbSet
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all invoices with a specific status for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="status">The invoice status.</param>
    /// <returns>A collection of invoices with the specified status.</returns>
    public async Task<IEnumerable<Invoice>> GetByStatusAsync(string businessId, InvoiceStatus status)
    {
        return await DbSet
            .Where(i => i.BusinessId == businessId && i.Status == status)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if an invoice with the specified ID exists.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <returns>True if the invoice exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(i => i.Id == id);
    }

    /// <summary>
    /// Checks if an invoice number already exists for a business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceNumber">The invoice number to check.</param>
    /// <returns>True if the invoice number exists, false otherwise.</returns>
    public async Task<bool> InvoiceNumberExistsAsync(string businessId, string invoiceNumber)
    {
        return await DbSet.AnyAsync(i => i.BusinessId == businessId && i.InvoiceNumber == invoiceNumber);
    }

    /// <summary>
    /// Gets all overdue invoices.
    /// </summary>
    /// <returns>A collection of overdue invoices.</returns>
    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
    {
        return await DbSet
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.UtcNow)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }
} 