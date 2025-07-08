namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;

public record RevokeTokenCommand(string RefreshToken) : IRequest<bool>;