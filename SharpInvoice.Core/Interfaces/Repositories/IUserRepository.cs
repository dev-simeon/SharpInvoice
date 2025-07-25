using SharpInvoice.Core.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing User entities.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a user by its email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>A collection of all users.</returns>
    Task<IEnumerable<User>> GetAllAsync();
    
    /// <summary>
    /// Checks if a user exists.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>True if the user exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string id);
    
    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if a user with the email exists; otherwise, false.</returns>
    Task<bool> EmailExistsAsync(string email);
}
