namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using System;

public record RemoveTeamMemberCommand(Guid TeamMemberId) : IRequest;