namespace SharpInvoice.Shared.Kernel.Domain;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IDomainEvent
{
    DateTime DateOccurred { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}