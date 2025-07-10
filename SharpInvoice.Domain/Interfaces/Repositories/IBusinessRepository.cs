using SharpInvoice.Core.Domain.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface IBusinessRepository
{
    Task<Business?> GetByIdAsync(string id);
    Task<IEnumerable<Business>> GetByOwnerIdAsync(string ownerId);
    Task<IEnumerable<Business>> GetActiveBusinessesAsync(string ownerId);
    Task<bool> ExistsAsync(string id);
    Task AddAsync(Business business);
    Task UpdateAsync(Business business);
    Task<Business?> GetByIdIncludingDeletedAsync(string id);
    Task<bool> CanRestoreAsync(string id);
    Task SaveChangesAsync();
} 