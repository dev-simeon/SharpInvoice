using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Domain.Enums;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Role?> GetByIdAsync(string id)
    {
        return await _db.Roles.FindAsync(id);
    }

    public async Task<Role?> GetByNameAsync(BusinessRole name)
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _db.Roles.OrderBy(r => (int)r.Name).ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _db.Roles.AnyAsync(r => r.Id == id);
    }

    public async Task AddAsync(Role role)
    {
        await _db.Roles.AddAsync(role);
    }

    public Task UpdateAsync(Role role)
    {
        _db.Roles.Update(role);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 