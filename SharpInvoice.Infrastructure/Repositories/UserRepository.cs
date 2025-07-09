using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext db) : IUserRepository
    {
        public async Task AddAsync(User user)
        {
            await db.Users.AddAsync(user);
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await db.Users.FindAsync(userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task UpdateAsync(User user)
        {
            db.Users.Update(user);
            return Task.CompletedTask;
        }
    }
}