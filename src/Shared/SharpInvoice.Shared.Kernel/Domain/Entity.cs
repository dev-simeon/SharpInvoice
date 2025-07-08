namespace SharpInvoice.Shared.Kernel.Domain;

using System.Collections.Generic;

/// <summary>
/// Base class for all entities in the domain.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = new();

    public TId Id { get; protected set; } = default!;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity(TId id)
    {
        Id = id;
    }

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // EF Core constructor
    protected Entity() { }
}