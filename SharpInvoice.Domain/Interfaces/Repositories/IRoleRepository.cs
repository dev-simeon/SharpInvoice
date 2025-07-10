using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;

namespace SharpInvoice.Core.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(string id);
    Task<Role?> GetByNameAsync(BusinessRole name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<bool> ExistsAsync(string id);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task SaveChangesAsync();
} 