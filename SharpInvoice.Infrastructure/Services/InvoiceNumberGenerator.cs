using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Services;

public class InvoiceNumberGenerator(AppDbContext db) : IInvoiceNumberGenerator
{
    public async Task<string> GenerateInvoiceNumberAsync(string businessId)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        
        // Get the highest invoice number for this business in the current year and month
        var latestInvoice = await db.Invoices
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
        if (int.TryParse(latestInvoice.InvoiceNumber.AsSpan(6), out int lastNumber))
        {
            // Increment and format with leading zeros
            return $"{year}{month:D2}{(lastNumber + 1):D3}";
        }

        // Fallback if parsing fails
        return $"{year}{month:D2}001";
    }
} 