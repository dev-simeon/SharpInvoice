namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System;
using System.Collections.Generic;

public record GetTeamMembersQuery(Guid BusinessId) : IRequest<IEnumerable<TeamMemberDto>>;