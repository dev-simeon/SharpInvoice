using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly AppDbContext _db;

    public BusinessRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Business?> GetByIdAsync(string id)
    {
        return await _db.Businesses.FindAsync(id);
    }

    public async Task<IEnumerable<Business>> GetByOwnerIdAsync(string ownerId)
    {
        return await _db.Businesses
            .Where(b => b.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Business>> GetActiveBusinessesAsync(string ownerId)
    {
        return await _db.Businesses
            .Where(b => b.OwnerId == ownerId && b.IsActive)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _db.Businesses.AnyAsync(b => b.Id == id);
    }

    public async Task AddAsync(Business business)
    {
        await _db.Businesses.AddAsync(business);
    }

    public Task UpdateAsync(Business business)
    {
        _db.Businesses.Update(business);
        return Task.CompletedTask;
    }

    public async Task<Business?> GetByIdIncludingDeletedAsync(string id)
    {
        // Temporarily disable the global query filter
        return await _db.Businesses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> CanRestoreAsync(string id)
    {
        // Check if there's another active business with the same name and country
        var deletedBusiness = await GetByIdIncludingDeletedAsync(id);
        if (deletedBusiness == null)
            return false;

        return !await _db.Businesses
            .AnyAsync(b => !b.IsDeleted && 
                     b.Name == deletedBusiness.Name && 
                     b.Country == deletedBusiness.Country);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 