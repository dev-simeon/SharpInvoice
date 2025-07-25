namespace SharpInvoice.Core.Shared;

using System;

/// <summary>
/// Base class for all domain entities with common functionality.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; internal set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }
    
    /// <summary>
    /// Gets or sets the deletion timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Marks the entity as deleted (soft delete).
    /// </summary>
    public void Delete()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Sets the CreatedAt property to the current UTC time.
    /// </summary>
    public void SetCreatedNow() => CreatedAt = DateTime.UtcNow;
    
    /// <summary>
    /// Sets the UpdatedAt property to the current UTC time.
    /// </summary>
    public void SetUpdatedNow() => UpdatedAt = DateTime.UtcNow;
}