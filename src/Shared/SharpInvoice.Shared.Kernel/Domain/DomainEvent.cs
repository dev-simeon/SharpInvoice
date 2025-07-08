namespace SharpInvoice.Shared.Kernel.Domain;

using MediatR;

public abstract class DomainEvent : INotification
{
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}