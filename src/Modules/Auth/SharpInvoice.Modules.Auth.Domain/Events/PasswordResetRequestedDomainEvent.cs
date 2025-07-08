namespace SharpInvoice.Modules.Auth.Domain.Events;

using SharpInvoice.Shared.Kernel.Domain;
using System;

public class PasswordResetRequestedDomainEvent(Guid userId, string token) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string Token { get; } = token;
}