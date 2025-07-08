namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class InviteTeamMemberCommandHandler(
    IInvitationRepository invitationRepository,
    ITeamMemberRepository teamMemberRepository,
    IUserRepository userRepository,
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<InviteTeamMemberCommand>
{
    public async Task Handle(InviteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(request.BusinessId)
            ?? throw new NotFoundException($"Business with ID {request.BusinessId} not found.");

        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            var isAlreadyMember = await teamMemberRepository.IsTeamMemberAsync(existingUser.Id, request.BusinessId);
            if (isAlreadyMember)
            {
                throw new BadRequestException("User is already a member of this business.");
            }
        }

        var teamMembers = await teamMemberRepository.GetByBusinessIdAsync(request.BusinessId);
        var invitation = Invitation.Create(request.BusinessId, request.Email, request.RoleId, 24, teamMembers.ToList());
        await invitationRepository.AddAsync(invitation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}