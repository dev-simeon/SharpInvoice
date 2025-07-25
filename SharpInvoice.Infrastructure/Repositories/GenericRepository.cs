using Microsoft.EntityFrameworkCore;
using SharpInvoice.Core.Interfaces.Repositories;
using SharpInvoice.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace SharpInvoice.Infrastructure.Repositories;

public class GenericRepository<TEntity>(AppDbContext context) : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext Context = context ?? throw new ArgumentNullException(nameof(context));
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        
        return await DbSet.FindAsync(new[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split(
            new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty.Trim());
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            // Apply a default ordering to ensure consistent pagination results
            // If entity has 'Id' property, use it as default ordering
            var entityType = typeof(TEntity);
            var idProperty = entityType.GetProperty("Id");
            if (idProperty != null)
            {
                var parameter = Expression.Parameter(entityType, "x");
                var property = Expression.Property(parameter, idProperty);
                var lambda = Expression.Lambda(property, parameter);
                
                var orderByMethod = typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityType, idProperty.PropertyType);
                
                query = (IOrderedQueryable<TEntity>)orderByMethod.Invoke(null, new object[] { query, lambda })!;
            }
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        try
        {
            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log exception or add telemetry here if needed
            throw new RepositoryException($"Error retrieving entities of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        try
        {
            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException($"Error counting entities of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        
        try
        {
            return await DbSet.AsNoTracking()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException($"Error finding entities of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        string includeProperties = "",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        
        IQueryable<TEntity> query = DbSet;

        foreach (var includeProperty in includeProperties.Split(
            new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty.Trim());
        }

        try
        {
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException($"Error retrieving entity of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        
        try
        {
            await DbSet.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException($"Error adding entity of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        
        try
        {
            await DbSet.AddRangeAsync(entities, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException($"Error adding entities of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        
        try
        {
            Context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Error updating entity of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        
        try
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Error removing entity of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        
        try
        {
            DbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Error removing entities of type {typeof(TEntity).Name}", ex);
        }
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        
        try
        {
            return await DbSet.AnyAsync(predicate, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException($"Error checking existence of entity of type {typeof(TEntity).Name}", ex);
        }
    }
    
    /// <summary>
    /// Executes a raw SQL query and returns entities of the specified type.
    /// Use this method only when necessary - prefer LINQ queries for better type safety.
    /// </summary>
    /// <param name="sql">The SQL query string</param>
    /// <param name="parameters">Parameters for the query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of entities</returns>
    public async Task<IEnumerable<TEntity>> FromSqlRawAsync(string sql, object[] parameters, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sql, nameof(sql));
        
        try
        {
            return await DbSet.FromSqlRaw(sql, parameters)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException($"Error executing raw SQL query for type {typeof(TEntity).Name}", ex);
        }
    }
}

/// <summary>
/// Exception thrown when a repository operation fails.
/// </summary>
public class RepositoryException : Exception
{
    public RepositoryException(string message) : base(message) { }
    public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
} 