using SharpInvoice.Modules.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpInvoice.Modules.Auth.Application.Interfaces
{
    /// <summary>
    /// Repository for external login operations
    /// </summary>
    public interface IExternalLoginRepository
    {
        /// <summary>
        /// Gets an external login by provider and key
        /// </summary>
        Task<ExternalLogin?> GetByProviderAndKeyAsync(string loginProvider, string providerKey);

        /// <summary>
        /// Gets all external logins for a specific user
        /// </summary>
        Task<IEnumerable<ExternalLogin>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Gets an external login by id
        /// </summary>
        Task<ExternalLogin?> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new external login
        /// </summary>
        Task AddAsync(ExternalLogin externalLogin);

        /// <summary>
        /// Deletes an external login by id
        /// </summary>
        Task DeleteAsync(Guid id);
    }
}