using Microsoft.EntityFrameworkCore;
using SharpInvoice.Infrastructure.Persistence;

namespace SharpInvoice.Infrastructure.Repositories;

/// <summary>
/// Base repository that provides common functionality for all repositories.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
public abstract class BaseRepository<TEntity>(AppDbContext context) where TEntity : class
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public virtual async Task AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public virtual async Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        await Task.CompletedTask; // Make the method consistently async
    }

    /// <summary>
    /// Removes an entity from the database.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    public virtual async Task RemoveAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        await Task.CompletedTask; // Make the method consistently async
    }

    /// <summary>
    /// Saves all changes made in this repository to the database.
    /// </summary>
    public virtual async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }
} 