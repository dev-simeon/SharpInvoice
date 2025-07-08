namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;

public record AcceptInvitationCommand(string Token) : IRequest;