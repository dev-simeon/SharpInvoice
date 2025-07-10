using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _db;

    public PasswordResetTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        // The query filter in the configuration already filters out used or expired tokens
        return await _db.PasswordResets
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<IEnumerable<PasswordResetToken>> GetByUserEmailAsync(string userEmail)
    {
        // The query filter in the configuration already filters out used or expired tokens
        return await _db.PasswordResets
            .Where(t => t.UserEmail.ToLower() == userEmail.ToLower())
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _db.PasswordResets.AnyAsync(t => t.Id == id);
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _db.PasswordResets.AddAsync(token);
    }

    public Task UpdateAsync(PasswordResetToken token)
    {
        _db.PasswordResets.Update(token);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
} 