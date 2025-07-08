namespace SharpInvoice.Shared.Infrastructure.Interfaces;

/// <summary>
/// Represents the Unit of Work pattern for managing transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the save operation.</param>
    /// <returns>
    /// A task that represents the asynchronous save operation. 
    /// The task result contains the number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}