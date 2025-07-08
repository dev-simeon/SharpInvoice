namespace SharpInvoice.Modules.Auth.Application.Queries;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using System;
using System.Collections.Generic;

public record GetLinkedAccountsQuery(Guid UserId) : IRequest<IEnumerable<ExternalAccountDto>>;