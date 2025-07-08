namespace SharpInvoice.Modules.UserManagement.Domain.Events;

using SharpInvoice.Shared.Kernel.Domain;
using System;

public class TeamMemberAddedDomainEvent(Guid teamMemberId) : DomainEvent
{
    public Guid TeamMemberId { get; } = teamMemberId;
}