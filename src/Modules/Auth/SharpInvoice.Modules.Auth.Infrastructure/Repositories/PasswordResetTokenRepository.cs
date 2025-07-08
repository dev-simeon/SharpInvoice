using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Threading.Tasks;

namespace SharpInvoice.Modules.Auth.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the password reset token repository using Entity Framework Core
    /// </summary>
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token && t.ExpiryDate > DateTime.UtcNow && !t.IsUsed);
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string token, string email)
        {
            return await _context.PasswordResetTokens.SingleOrDefaultAsync(
                t => t.Token == token &&
                     t.UserEmail == email &&
                     !t.IsUsed &&
                     t.ExpiryDate > DateTime.UtcNow);
        }

        public async Task AddAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
        }
    }
}