using SharpInvoice.Core.Domain.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(string id);
    Task<IEnumerable<Transaction>> GetByInvoiceIdAsync(string invoiceId);
    Task<bool> ExistsAsync(string id);
    Task<bool> ExternalTransactionIdExistsAsync(string externalTransactionId);
    Task AddAsync(Transaction transaction);
    Task SaveChangesAsync();
} 