using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpInvoice.Modules.Auth.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the external login repository using Entity Framework Core
    /// </summary>
    public class ExternalLoginRepository(AppDbContext context) : IExternalLoginRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<ExternalLogin?> GetByProviderAndKeyAsync(string loginProvider, string providerKey)
        {
            return await _context.ExternalLogins
                .SingleOrDefaultAsync(e => e.LoginProvider == loginProvider && e.ProviderKey == providerKey);
        }

        public async Task<IEnumerable<ExternalLogin>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ExternalLogins
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<ExternalLogin?> GetByIdAsync(Guid id)
        {
            return await _context.ExternalLogins
                .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(ExternalLogin externalLogin)
        {
            await _context.ExternalLogins.AddAsync(externalLogin);
        }

        public async Task DeleteAsync(Guid id)
        {
            var externalLogin = await GetByIdAsync(id);
            if (externalLogin != null)
            {
                _context.ExternalLogins.Remove(externalLogin);
            }
        }
    }
}