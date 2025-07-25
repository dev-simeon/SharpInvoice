using SharpInvoice.Core.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing PasswordResetToken entities.
/// </summary>
public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    /// <summary>
    /// Gets a password reset token by its value.
    /// </summary>
    /// <param name="token">The token value.</param>
    /// <returns>The password reset token if found; otherwise, null.</returns>
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    
    /// <summary>
    /// Gets all password reset tokens for a specific user email.
    /// </summary>
    /// <param name="userEmail">The user's email.</param>
    /// <returns>A collection of password reset tokens.</returns>
    Task<IEnumerable<PasswordResetToken>> GetByUserEmailAsync(string userEmail);
    
    /// <summary>
    /// Checks if a password reset token exists.
    /// </summary>
    /// <param name="id">The token ID.</param>
    /// <returns>True if the token exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Creates a new password reset token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created password reset token.</returns>
    Task<PasswordResetToken> CreateForUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a valid (non-expired, non-used) password reset token for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="token">The token value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The valid password reset token if found; otherwise, null.</returns>
    Task<PasswordResetToken?> GetValidTokenAsync(string userId, string token, CancellationToken cancellationToken = default);
} 