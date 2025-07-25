using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;
using SharpInvoice.Infrastructure.Repositories;
using System.Collections;

namespace SharpInvoice.Infrastructure;

/// <summary>
/// Implementation of the Unit of Work pattern for database operations.
/// Manages transaction scope and provides access to repositories.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UnitOfWork"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
/// <param name="logger">The logger.</param>
public class UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private readonly AppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly Hashtable _repositories = [];
    private readonly ILogger<UnitOfWork> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private bool _disposed;

    public IUserRepository Users => GetRepository<User, IUserRepository>();
    public IBusinessRepository Businesses => GetRepository<Business, IBusinessRepository>();
    public IClientRepository Clients => GetRepository<Client, IClientRepository>();
    public IInvitationRepository Invitations => GetRepository<Invitation, IInvitationRepository>();
    public IInvoiceRepository Invoices => GetRepository<Invoice, IInvoiceRepository>();
    public IPasswordResetTokenRepository PasswordResetTokens => GetRepository<PasswordResetToken, IPasswordResetTokenRepository>();
    public IRoleRepository Roles => GetRepository<Role, IRoleRepository>();
    public ITeamMemberRepository TeamMembers => GetRepository<TeamMember, ITeamMemberRepository>();
    public ITransactionRepository Transactions => GetRepository<Transaction, ITransactionRepository>();

    /// <summary>
    /// Gets a repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A repository for the specified entity type.</returns>
    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        return GetRepository<TEntity, IRepository<TEntity>>();
    }

    /// <summary>
    /// Gets a repository of the specified type for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TRepository">The repository type.</typeparam>
    /// <returns>A repository of the specified type.</returns>
    private TRepository GetRepository<TEntity, TRepository>() where TEntity : class
    {
        var entityType = typeof(TEntity).Name;
        
        if (!_repositories.ContainsKey(entityType))
        {
            var repositoryType = typeof(TRepository).Name;
            var concreteType = Type.GetType($"SharpInvoice.Infrastructure.Repositories.{entityType}Repository")
                ?? typeof(GenericRepository<TEntity>);

            var instance = Activator.CreateInstance(concreteType, _context);
            _repositories.Add(entityType, instance);
            
            _logger.LogDebug("Created repository {RepositoryType} for entity {EntityType}", repositoryType, entityType);
        }
        
        return (TRepository)_repositories[entityType];
    }
    
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred while saving changes");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes to the database");
            throw;
        }
    }
    
    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <returns>True if a new transaction was started, false if there's already an active transaction.</returns>
    public async Task<bool> BeginTransactionAsync()
    {
        if (_context.Database.CurrentTransaction != null)
        {
            return false;
        }
        
        await _context.Database.BeginTransactionAsync();
        _logger.LogDebug("Transaction started");
        return true;
    }
    
    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        if (_context.Database.CurrentTransaction == null)
        {
            _logger.LogWarning("No active transaction to commit");
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await _context.Database.CommitTransactionAsync();
            _logger.LogDebug("Transaction committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while committing the transaction");
            throw;
        }
    }
    
    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_context.Database.CurrentTransaction != null)
        {
            try
            {
                await _context.Database.RollbackTransactionAsync();
                _logger.LogDebug("Transaction rolled back");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rolling back the transaction");
                throw;
            }
        }
        else
        {
            _logger.LogWarning("No active transaction to roll back");
        }
    }
    
    /// <summary>
    /// Executes a function within a transaction.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public async Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> func)
    {
        bool transactionStarted = false;
        try
        {
            transactionStarted = await BeginTransactionAsync();
            var result = await func();
            
            // Only commit if we started the transaction (not if one was already active)
            if (transactionStarted)
            {
                await CommitTransactionAsync();
            }
            
            return result;
        }
        catch
        {
            // Only roll back if we started the transaction
            if (transactionStarted)
            {
                await RollbackTransactionAsync();
            }
            throw;
        }
    }

    /// <summary>
    /// Executes an action within a transaction.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        bool transactionStarted = false;
        try
        {
            transactionStarted = await BeginTransactionAsync();
            await action();
            
            // Only commit if we started the transaction
            if (transactionStarted)
            {
                await CommitTransactionAsync();
            }
        }
        catch
        {
            // Only roll back if we started the transaction
            if (transactionStarted)
            {
                await RollbackTransactionAsync();
            }
            throw;
        }
    }
    
    /// <summary>
    /// Disposes the resources used by the unit of work.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Disposes the resources used by the unit of work.
    /// </summary>
    /// <param name="disposing">Indicates whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _context.Dispose();
                _repositories?.Clear();
            }
            
            _disposed = true;
        }
    }
} 