namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using System;
using System.Collections.Generic;

public record GetUserPermissionsQuery(Guid UserId, Guid BusinessId) : IRequest<IEnumerable<string>>; 