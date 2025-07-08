namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using System;

public record InviteTeamMemberCommand(Guid BusinessId, string Email, Guid RoleId) : IRequest;