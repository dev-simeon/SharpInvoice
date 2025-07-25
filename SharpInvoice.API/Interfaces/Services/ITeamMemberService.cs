using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Interfaces.Services;

/// <summary>
/// Defines the contract for team member and invitation management.
/// </summary>
public interface ITeamMemberService
{
    /// <summary>
    /// Invites a new team member to a business.
    /// </summary>
    /// <param name="businessId">The ID of the business to invite the member to.</param>
    /// <param name="inviterId">The ID of the user sending the invitation.</param>
    /// <param name="request">The invitation request details.</param>
    Task InviteTeamMemberAsync(string businessId, string inviterId, InviteTeamMemberRequest request);

    /// <summary>
    /// Accepts a team member invitation.
    /// </summary>
    /// <param name="userId">The ID of the user accepting the invitation.</param>
    /// <param name="request">The invitation acceptance details.</param>
    Task AcceptInvitationAsync(string userId, AcceptInvitationRequest request);
} 