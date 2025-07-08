namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;

public record HandleExternalLoginCommand : IRequest<AuthResponseDto>;