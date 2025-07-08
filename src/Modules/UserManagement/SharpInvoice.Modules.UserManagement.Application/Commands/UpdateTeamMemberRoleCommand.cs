namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using System;

public record UpdateTeamMemberRoleCommand(Guid TeamMemberId, Guid NewRoleId) : IRequest;