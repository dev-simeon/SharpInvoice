namespace SharpInvoice.Core.Interfaces.Repositories;

/// <summary>
/// Base interface for all repositories.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(
        System.Linq.Expressions.Expression<System.Func<TEntity, bool>>? filter = null,
        System.Func<System.Linq.IQueryable<TEntity>, System.Linq.IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        System.Linq.Expressions.Expression<System.Func<TEntity, bool>>? filter = null, 
        CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(
        System.Linq.Expressions.Expression<System.Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<System.Func<TEntity, bool>> predicate,
        string includeProperties = "",
        CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(
        System.Linq.Expressions.Expression<System.Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
} 