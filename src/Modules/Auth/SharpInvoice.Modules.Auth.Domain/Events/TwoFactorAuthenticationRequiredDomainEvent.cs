namespace SharpInvoice.Modules.Auth.Domain.Events;

using SharpInvoice.Shared.Kernel.Domain;
using System;

public class TwoFactorAuthenticationRequiredDomainEvent(Guid userId) : DomainEvent
{
    public Guid UserId { get; } = userId;
}