namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class AcceptInvitationCommandHandler(
    IInvitationRepository invitationRepository,
    ITeamMemberRepository teamMemberRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AcceptInvitationCommand>
{
    public async Task Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByTokenAsync(request.Token)
            ?? throw new NotFoundException("Invitation not found.");

        invitation.Accept();

        var user = await userRepository.GetByEmailAsync(invitation.InvitedUserEmail);
        if (user == null)
        {
            // This is a simplified approach. A real-world application would
            // likely redirect to a registration page with the email pre-filled.
            throw new NotFoundException($"User with email {invitation.InvitedUserEmail} not found. Please register first.");
        }

        var teamMember = TeamMember.Create(user.Id, invitation.BusinessId, invitation.RoleId);
        
        await teamMemberRepository.AddAsync(teamMember);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}