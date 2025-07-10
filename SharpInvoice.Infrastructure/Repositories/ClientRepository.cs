using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _db;

    public ClientRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Client?> GetByIdAsync(string id)
    {
        return await _db.Clients.FindAsync(id);
    }

    public async Task<IEnumerable<Client>> GetByBusinessIdAsync(string businessId)
    {
        return await _db.Clients
            .Where(c => c.BusinessId == businessId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _db.Clients.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> HasInvoicesAsync(string id)
    {
        return await _db.Invoices.AnyAsync(i => i.ClientId == id);
    }

    public async Task AddAsync(Client client)
    {
        await _db.Clients.AddAsync(client);
    }

    public Task UpdateAsync(Client client)
    {
        _db.Clients.Update(client);
        return Task.CompletedTask;
    }

    public async Task<Client?> GetByEmailAndBusinessIdAsync(string email, string businessId)
    {
        if (string.IsNullOrEmpty(email))
            return null;
            
        return await _db.Clients
            .FirstOrDefaultAsync(c => c.Email != null && 
                                c.Email.ToLower() == email.ToLower() && 
                                c.BusinessId == businessId);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 