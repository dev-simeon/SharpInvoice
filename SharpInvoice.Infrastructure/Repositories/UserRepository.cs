using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Domain.Entities;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Repository for managing User entities.
/// </summary>
public class UserRepository(AppDbContext db) : BaseRepository<User>(db), IUserRepository
{
    /// <summary>
    /// Gets a user by its unique identifier.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user entity if found, null otherwise.</returns>
    public async Task<User?> GetByIdAsync(string id)
    {
        return await DbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <returns>The user entity if found, null otherwise.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>A collection of all users.</returns>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    /// <summary>
    /// Checks if a user with the specified ID exists.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(u => u.Id == id);
    }

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if a user with the email exists, false otherwise.</returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet.AnyAsync(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));
    }
}