using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _db;

    public InvoiceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Invoice?> GetByIdAsync(string id)
    {
        return await _db.Invoices
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Invoice>> GetByBusinessIdAsync(string businessId)
    {
        return await _db.Invoices
            .Where(i => i.BusinessId == businessId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByClientIdAsync(string clientId)
    {
        return await _db.Invoices
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(string businessId, InvoiceStatus status)
    {
        return await _db.Invoices
            .Where(i => i.BusinessId == businessId && i.Status == status)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _db.Invoices.AnyAsync(i => i.Id == id);
    }

    public async Task<bool> InvoiceNumberExistsAsync(string businessId, string invoiceNumber)
    {
        return await _db.Invoices.AnyAsync(i => i.BusinessId == businessId && i.InvoiceNumber == invoiceNumber);
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(string businessId)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        
        // Get the highest invoice number for this business in the current year and month
        var latestInvoice = await _db.Invoices
            .Where(i => i.BusinessId == businessId)
            .Where(i => i.IssueDate.Year == year && i.IssueDate.Month == month)
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (latestInvoice == null)
        {
            // No invoices for this month yet, start with 001
            return $"{year}{month:D2}001";
        }

        // Parse the numeric part of the latest invoice number
        if (int.TryParse(latestInvoice.InvoiceNumber.Substring(6), out int lastNumber))
        {
            // Increment and format with leading zeros
            return $"{year}{month:D2}{(lastNumber + 1):D3}";
        }

        // Fallback if parsing fails
        return $"{year}{month:D2}001";
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _db.Invoices.AddAsync(invoice);
    }

    public Task UpdateAsync(Invoice invoice)
    {
        _db.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
    {
        return await _db.Invoices
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < DateTime.UtcNow)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 