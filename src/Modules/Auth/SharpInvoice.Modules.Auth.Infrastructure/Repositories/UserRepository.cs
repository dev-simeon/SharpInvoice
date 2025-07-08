using Microsoft.EntityFrameworkCore;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpInvoice.Modules.Auth.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of the user repository using Entity Framework Core
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public async Task<IEnumerable<User>> GetUsersWithExpiredRefreshTokensAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Users
                .Where(u => u.RefreshTokenExpiryTime < now && u.RefreshToken != null)
                .ToListAsync();
        }
    }
}