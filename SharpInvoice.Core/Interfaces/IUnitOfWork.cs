using SharpInvoice.Core.Interfaces.Repositories;

namespace SharpInvoice.Core.Interfaces;

/// <summary>
/// Defines the Unit of Work pattern interface to manage transactions and repository access.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IUserRepository Users { get; }
    
    /// <summary>
    /// Gets the business repository.
    /// </summary>
    IBusinessRepository Businesses { get; }
    
    /// <summary>
    /// Gets the client repository.
    /// </summary>
    IClientRepository Clients { get; }
    
    /// <summary>
    /// Gets the invitation repository.
    /// </summary>
    IInvitationRepository Invitations { get; }
    
    /// <summary>
    /// Gets the invoice repository.
    /// </summary>
    IInvoiceRepository Invoices { get; }
    
    /// <summary>
    /// Gets the password reset token repository.
    /// </summary>
    IPasswordResetTokenRepository PasswordResetTokens { get; }
    
    /// <summary>
    /// Gets the role repository.
    /// </summary>
    IRoleRepository Roles { get; }
    
    /// <summary>
    /// Gets the team member repository.
    /// </summary>
    ITeamMemberRepository TeamMembers { get; }
    
    /// <summary>
    /// Gets the transaction repository.
    /// </summary>
    ITransactionRepository Transactions { get; }
    
    /// <summary>
    /// Gets a generic repository for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A generic repository for the specified entity type.</returns>
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <returns>True if a new transaction was started, false if there's already an active transaction.</returns>
    Task<bool> BeginTransactionAsync();
    
    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync();
    
    /// <summary>
    /// Executes a function within a transaction.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> func);

    /// <summary>
    /// Executes an action within a transaction.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    Task ExecuteTransactionAsync(Func<Task> action);
} 