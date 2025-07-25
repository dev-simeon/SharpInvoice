using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing PasswordResetToken entities.
/// </summary>
public class PasswordResetTokenRepository(AppDbContext db) : GenericRepository<PasswordResetToken>(db), IPasswordResetTokenRepository
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

    /// <summary>
    /// Creates a new password reset token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created password reset token.</returns>
    public async Task<PasswordResetToken> CreateForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // Get user to obtain email
        var user = await Context.Set<User>().FindAsync(new object[] { userId }, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        // Generate a secure token
        var token = Guid.NewGuid().ToString("N");

        // Create password reset token (valid for 1 hour)
        var resetToken = new PasswordResetToken(token, userId, user.Email, 60);

        await DbSet.AddAsync(resetToken, cancellationToken);
        return resetToken;
    }

    /// <summary>
    /// Gets a valid (non-expired, non-used) password reset token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The token value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The valid password reset token if found; otherwise, null.</returns>
    public async Task<PasswordResetToken?> GetValidTokenAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return await DbSet
            .Where(t => t.UserId == userId && 
                       t.Token == token && 
                       !t.IsUsed && 
                       t.ExpiryDate > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }
} 