namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using System;

public record UnlinkAccountCommand(Guid UserId, string Provider) : IRequest;