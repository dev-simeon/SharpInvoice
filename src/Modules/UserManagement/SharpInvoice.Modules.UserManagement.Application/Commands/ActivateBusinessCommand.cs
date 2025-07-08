namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;

public record ActivateBusinessCommand(Guid BusinessId) : IRequest;