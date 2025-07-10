using SharpInvoice.Core.Domain.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(string id);
    Task<IEnumerable<Client>> GetByBusinessIdAsync(string businessId);
    Task<bool> ExistsAsync(string id);
    Task<bool> HasInvoicesAsync(string id);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task<Client?> GetByEmailAndBusinessIdAsync(string email, string businessId);
    Task SaveChangesAsync();
} 