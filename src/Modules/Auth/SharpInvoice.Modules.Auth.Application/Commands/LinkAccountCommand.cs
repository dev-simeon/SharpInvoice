namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using System;

public record LinkAccountCommand(Guid UserId, string Provider, string ProviderKey) : IRequest<bool>;