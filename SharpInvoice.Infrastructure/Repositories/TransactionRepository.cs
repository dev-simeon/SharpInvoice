using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;

    public TransactionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await _db.Transactions.FindAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetByInvoiceIdAsync(string invoiceId)
    {
        return await _db.Transactions
            .Where(t => t.InvoiceId == invoiceId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _db.Transactions.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> ExternalTransactionIdExistsAsync(string externalTransactionId)
    {
        if (string.IsNullOrEmpty(externalTransactionId))
            return false;
            
        return await _db.Transactions
            .AnyAsync(t => t.ExternalTransactionId == externalTransactionId);
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _db.Transactions.AddAsync(transaction);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 