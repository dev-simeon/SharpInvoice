using SharpInvoice.Modules.Auth.Domain.Entities;
using System.Threading.Tasks;

namespace SharpInvoice.Modules.Auth.Application.Interfaces
{
    /// <summary>
    /// Repository for password reset token operations
    /// </summary>
    public interface IPasswordResetTokenRepository
    {
        /// <summary>
        /// Gets a password reset token by the token string
        /// </summary>
        Task<PasswordResetToken?> GetByTokenAsync(string token);

        /// <summary>
        /// Gets a valid password reset token
        /// </summary>
        Task<PasswordResetToken?> GetValidTokenAsync(string token, string email);

        /// <summary>
        /// Adds a new password reset token
        /// </summary>
        Task AddAsync(PasswordResetToken token);
    }
}