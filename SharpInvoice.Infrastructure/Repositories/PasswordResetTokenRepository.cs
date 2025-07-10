using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing PasswordResetToken entities.
/// </summary>
public class PasswordResetTokenRepository(AppDbContext db) : BaseRepository<PasswordResetToken>(db), IPasswordResetTokenRepository
{
    /// <summary>
    /// Gets a password reset token by its token string.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <returns>The password reset token entity if found, null otherwise.</returns>
    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        // The query filter in the configuration already filters out used or expired tokens
        return await DbSet
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    /// <summary>
    /// Gets all password reset tokens for a specific user email.
    /// </summary>
    /// <param name="userEmail">The user email.</param>
    /// <returns>A collection of password reset tokens for the specified user email.</returns>
    public async Task<IEnumerable<PasswordResetToken>> GetByUserEmailAsync(string userEmail)
    {
        // The query filter in the configuration already filters out used or expired tokens
        return await DbSet
            .Where(t => t.UserEmail.Equals(userEmail, StringComparison.CurrentCultureIgnoreCase))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a password reset token with the specified ID exists.
    /// </summary>
    /// <param name="id">The token ID.</param>
    /// <returns>True if the token exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(t => t.Id == id);
    }
} 