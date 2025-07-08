using SharpInvoice.Modules.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpInvoice.Modules.Auth.Application.Interfaces
{
    /// <summary>
    /// Repository for user-related data operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Gets a user by their refresh token
        /// </summary>
        Task<User?> GetByRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Checks if a user with the specified email exists
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Adds a new user
        /// </summary>
        Task AddAsync(User user);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        void Update(User user);

        /// <summary>
        /// Gets users with expired refresh tokens
        /// </summary>
        Task<IEnumerable<User>> GetUsersWithExpiredRefreshTokensAsync();
    }
}