namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using System;

public record ConfirmEmailCommand(Guid UserId, string Token) : IRequest<bool>;