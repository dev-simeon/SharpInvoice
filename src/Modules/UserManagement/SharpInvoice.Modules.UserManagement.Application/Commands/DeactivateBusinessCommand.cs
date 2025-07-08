namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using System;

public class DeactivateBusinessCommand(Guid businessId) : IRequest
{
    public Guid BusinessId { get; } = businessId;
}