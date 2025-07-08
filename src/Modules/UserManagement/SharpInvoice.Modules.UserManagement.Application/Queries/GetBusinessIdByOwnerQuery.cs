namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using System;

public record GetBusinessIdByOwnerQuery(Guid OwnerId) : IRequest<Guid>; 