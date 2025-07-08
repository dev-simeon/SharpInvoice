namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System.Security.Claims;

/// <summary>
/// Provides endpoints for managing team members.
/// </summary>
[ApiController]
[Route("api/team")]
[Authorize]
[Tags("Team Members")]
public class TeamMemberController(ITeamMemberService teamMemberService) : ApiControllerBase
{
    /// <summary>
    /// Gets all team members for a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <returns>A list of team members for the specified business.</returns>
    [HttpGet("{businessId}/members")]
    [EndpointName("Get Team Members")]
    [EndpointSummary("Get all team members for a business")]
    [EndpointDescription("Retrieves a list of all team members associated with a specific business")]
    [ProducesResponseType(typeof(IEnumerable<TeamMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamMembers([FromRoute] Guid businessId)
    {
        var teamMembers = await teamMemberService.GetTeamMembersAsync(businessId);
        return Ok(teamMembers);
    }

    /// <summary>
    /// Invites a new team member to join a business.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <param name="request">The invitation details including email and role.</param>
    /// <returns>No content if the invitation was sent successfully.</returns>
    [HttpPost("{businessId}/invite")]
    [EndpointName("Invite Team Member")]
    [EndpointSummary("Invite a new team member")]
    [EndpointDescription("Sends an invitation email to a user to join the specified business")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InviteTeamMember([FromRoute] Guid businessId, [FromBody] InviteTeamMemberRequest request)
    {
        await teamMemberService.InviteTeamMemberAsync(businessId, request.Email, request.RoleId);
        return NoContent();
    }

    /// <summary>
    /// Accepts a team invitation using a token.
    /// </summary>
    /// <param name="request">The request containing the invitation token.</param>
    /// <returns>No content if the invitation was accepted successfully.</returns>
    [HttpPost("accept-invitation")]
    [EndpointName("Accept Team Invitation")]
    [EndpointSummary("Accept a team invitation")]
    [EndpointDescription("Accepts a team invitation using the provided token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        await teamMemberService.AcceptInvitationAsync(request.Token);
        return NoContent();
    }

    /// <summary>
    /// Removes a team member from a business.
    /// </summary>
    /// <param name="teamMemberId">The ID of the team member to remove.</param>
    /// <returns>No content if the team member was removed successfully.</returns>
    [HttpDelete("{teamMemberId}")]
    [EndpointName("Remove Team Member")]
    [EndpointSummary("Remove a team member")]
    [EndpointDescription("Removes a team member from the business")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTeamMember([FromRoute] Guid teamMemberId)
    {
        await teamMemberService.RemoveTeamMemberAsync(teamMemberId);
        return NoContent();
    }

    /// <summary>
    /// Updates a team member's role.
    /// </summary>
    /// <param name="teamMemberId">The ID of the team member to update.</param>
    /// <param name="request">The request containing the new role ID.</param>
    /// <returns>No content if the role was updated successfully.</returns>
    [HttpPut("{teamMemberId}/role")]
    [EndpointName("Update Team Member Role")]
    [EndpointSummary("Update a team member's role")]
    [EndpointDescription("Changes the role of a specific team member")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeamMemberRole([FromRoute] Guid teamMemberId, [FromBody] UpdateTeamMemberRoleRequest request)
    {
        await teamMemberService.UpdateTeamMemberRoleAsync(teamMemberId, request.NewRoleId);
        return NoContent();
    }

    /// <summary>
    /// Gets the roles for the current user in a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <returns>A list of role names for the user.</returns>
    [HttpGet("{businessId}/my-roles")]
    [EndpointName("Get My Roles")]
    [EndpointSummary("Get current user's roles in a business")]
    [EndpointDescription("Retrieves the roles assigned to the current user in the specified business")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyRoles([FromRoute] Guid businessId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roles = await teamMemberService.GetUserRolesAsync(userId, businessId);
        return Ok(roles);
    }

    /// <summary>
    /// Gets the permissions for the current user in a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <returns>A list of permission names for the user.</returns>
    [HttpGet("{businessId}/my-permissions")]
    [EndpointName("Get My Permissions")]
    [EndpointSummary("Get current user's permissions in a business")]
    [EndpointDescription("Retrieves the permissions assigned to the current user in the specified business")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPermissions([FromRoute] Guid businessId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var permissions = await teamMemberService.GetUserPermissionsAsync(userId, businessId);
        return Ok(permissions);
    }
}

// Request DTOs to replace MediatR commands
public record InviteTeamMemberRequest(string Email, Guid RoleId);
public record AcceptInvitationRequest(string Token);
public record UpdateTeamMemberRoleRequest(Guid NewRoleId);