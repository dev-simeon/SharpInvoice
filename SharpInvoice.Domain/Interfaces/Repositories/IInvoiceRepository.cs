using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(string id);
    Task<IEnumerable<Invoice>> GetByBusinessIdAsync(string businessId);
    Task<IEnumerable<Invoice>> GetByClientIdAsync(string clientId);
    Task<IEnumerable<Invoice>> GetByStatusAsync(string businessId, InvoiceStatus status);
    Task<bool> ExistsAsync(string id);
    Task<bool> InvoiceNumberExistsAsync(string businessId, string invoiceNumber);
    Task<string> GenerateNextInvoiceNumberAsync(string businessId);
    Task AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
    Task SaveChangesAsync();
} 