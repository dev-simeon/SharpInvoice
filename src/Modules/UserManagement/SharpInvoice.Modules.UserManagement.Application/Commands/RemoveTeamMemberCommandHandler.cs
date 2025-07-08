namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class RemoveTeamMemberCommandHandler(
    ITeamMemberRepository teamMemberRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveTeamMemberCommand>
{
    public async Task Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var teamMember = await teamMemberRepository.GetByIdAsync(request.TeamMemberId)
            ?? throw new NotFoundException($"Team member with ID {request.TeamMemberId} not found.");

        teamMemberRepository.Remove(teamMember);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}