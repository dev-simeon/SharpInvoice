namespace SharpInvoice.Modules.UserManagement.Domain.Events;

using SharpInvoice.Shared.Kernel.Domain;
using System;

public class InvitationSentDomainEvent(Guid invitationId) : DomainEvent
{
    public Guid InvitationId { get; } = invitationId;
}