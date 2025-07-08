namespace SharpInvoice.Modules.UserManagement.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BusinessRepository : IBusinessRepository
{
    private readonly AppDbContext _context;

    public BusinessRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Business business)
    {
        await _context.Businesses.AddAsync(business);
    }

    public async Task<Business?> GetByIdAsync(Guid id)
    {
        return await _context.Businesses.FindAsync(id);
    }

    public async Task<IEnumerable<Business>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Businesses.Where(b => b.OwnerId == ownerId).ToListAsync();
    }

    public async Task<Business?> GetByNameAndCountryAsync(string name, string country)
    {
        return await _context.Businesses
            .FirstOrDefaultAsync(b => b.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) && b.Country.ToLower() == country.ToLower());
    }
    
    public async Task<IEnumerable<Business>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Businesses
            .Where(b => b.OwnerId == userId || b.TeamMembers.Any(tm => tm.UserId == userId))
            .ToListAsync();
    }

    public Task UpdateAsync(Business business)
    {
        _context.Businesses.Update(business);
        return Task.CompletedTask;
    }
} 