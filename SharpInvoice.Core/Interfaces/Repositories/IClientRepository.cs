using SharpInvoice.Core.Entities;

namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing Client entities.
/// </summary>
public interface IClientRepository : IRepository<Client>
{
    /// <summary>
    /// Gets a client by its ID.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>The client if found; otherwise, null.</returns>
    Task<Client?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets clients by business ID.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <returns>A collection of clients.</returns>
    Task<IEnumerable<Client>> GetByBusinessIdAsync(string businessId);
    
    /// <summary>
    /// Checks if a client exists.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>True if the client exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if a client has invoices.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <returns>True if the client has invoices; otherwise, false.</returns>
    Task<bool> HasInvoicesAsync(Guid id);
    
    /// <summary>
    /// Gets a client by email and business ID.
    /// </summary>
    /// <param name="email">The client's email.</param>
    /// <param name="businessId">The business ID.</param>
    /// <returns>The client if found; otherwise, null.</returns>
    Task<Client?> GetByEmailAndBusinessIdAsync(string email, string businessId);
} 