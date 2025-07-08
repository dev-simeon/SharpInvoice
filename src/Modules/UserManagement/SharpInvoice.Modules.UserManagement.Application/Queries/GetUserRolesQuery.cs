namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using System;
using System.Collections.Generic;

public record GetUserRolesQuery(Guid UserId, Guid BusinessId) : IRequest<IEnumerable<string>>; 