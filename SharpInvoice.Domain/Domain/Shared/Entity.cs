namespace SharpInvoice.Core.Domain.Shared;

/// <summary>
/// Base class for all entities in the domain.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    // EF Core constructor
    protected Entity() { }
}