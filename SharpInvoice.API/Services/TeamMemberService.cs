using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Enums;
using SharpInvoice.API.Interfaces.Services;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.Core.Entities;

namespace SharpInvoice.API.Services;

/// <summary>
/// Provides services for managing team members and invitations.
/// </summary>
public class TeamMemberService(IUnitOfWork unitOfWork) : ITeamMemberService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Accepts a team member invitation and adds the user to the business.
    /// </summary>
    /// <param name="userId">The ID of the user accepting the invitation.</param>
    /// <param name="request">The invitation acceptance request.</param>
    public async Task AcceptInvitationAsync(string userId, AcceptInvitationRequest request)
    {
        var invitations = await _unitOfWork.Invitations.FindAsync(i => i.Token == request.Token && i.Status == InvitationStatus.Pending);
        var invitation = invitations.FirstOrDefault();

        if (invitation == null || invitation.IsExpired())
        {
            throw new Exception("Invalid or expired invitation token.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.Email != invitation.InvitedUserEmail)
        {
            throw new Exception("Invitation is not for this user.");
        }

        invitation.Accept();
        
        var teamMember = new TeamMember(
            userId,
            invitation.BusinessId,
            invitation.RoleId
        );

        await _unitOfWork.TeamMembers.AddAsync(teamMember);
        await _unitOfWork.Invitations.UpdateAsync(invitation);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Invites a new team member to a business by creating an invitation and sending an email.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <param name="inviterId">The ID of the user sending the invitation.</param>
    /// <param name="request">The invitation request details.</param>
    public async Task InviteTeamMemberAsync(string businessId, string inviterId, InviteTeamMemberRequest request)
    {
        var inviter = await _unitOfWork.Users.GetByIdAsync(inviterId);
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId);
        if (inviter == null || business == null)
        {
            throw new Exception("Invalid inviter or business.");
        }
        
        var existingTeamMember = await _unitOfWork.TeamMembers.FindAsync(tm => tm.BusinessId == businessId && tm.User.Email == request.Email);
        if(existingTeamMember.Any())
        {
            throw new Exception("User is already a member of this business.");
        }

        var invitation = new Invitation(
            businessId,
            request.Email,
            request.Role.ToString(),
            7 * 24 // 7 days in hours
        );

        await _unitOfWork.Invitations.AddAsync(invitation);
        await _unitOfWork.SaveChangesAsync();

        // TODO: Implement email sending
        // var invitationLink = $"https://your-frontend-url/accept-invitation?token={invitation.Token}";
        // await _emailService.SendEmailAsync(
        //     to: request.Email,
        //     subject: $"You have been invited to join {business.Name}",
        //     body: $"Please accept the invitation by clicking this link: {invitationLink}"
        // );
    }
} 