namespace SharpInvoice.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Examples;
using SharpInvoice.Modules.UserManagement.Application.Commands;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Queries;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provides endpoints for managing team members within a business.
/// </summary>
[ApiController]
[Route("api/team-members")]
[Authorize]
[Tags("Team Management")]
[Produces("application/json")]
public class TeamMemberController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Invites a new member to the business.
    /// </summary>
    /// <param name="businessId">The ID of the business to invite the member to.</param>
    /// <param name="request">The invitation details including email address and role ID.</param>
    /// <returns>A success message if the invitation was sent.</returns>
    [HttpPost("invite")]
    [EndpointName("Invite Team Member")]
    [EndpointSummary("Invite a new team member to a business")]
    [EndpointDescription("Sends an invitation email to a new team member with a specific role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(InviteTeamMemberRequest), typeof(InviteTeamMemberRequestExample))]
    public async Task<IActionResult> InviteTeamMember([FromRoute] Guid businessId, [FromBody] InviteTeamMemberRequest request)
    {
        var command = new InviteTeamMemberCommand(businessId, request.Email, request.RoleId);
        await sender.Send(command);
        return Ok();
    }

    /// <summary>
    /// Accepts a pending invitation via a token.
    /// </summary>
    /// <param name="token">The invitation token from the email link.</param>
    /// <returns>A confirmation message that the invitation was accepted.</returns>
    [HttpGet("accept-invitation")]
    [EndpointName("Accept Team Invitation")]
    [EndpointSummary("Accept a team invitation")]
    [EndpointDescription("Validates and processes an invitation token to add a user to a business team")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> AcceptInvitation([FromQuery] string token)
    {
        var command = new AcceptInvitationCommand(token);
        await sender.Send(command);
        return Ok("Invitation accepted successfully.");
    }

    /// <summary>
    /// Gets all team members for a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <returns>A list of team members associated with the business.</returns>
    [HttpGet]
    [EndpointName("Get Team Members")]
    [EndpointSummary("List all team members of a business")]
    [EndpointDescription("Retrieves information about all users who are part of a business team")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TeamMemberDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TeamMemberListExample))]
    public async Task<IActionResult> GetTeamMembers([FromRoute] Guid businessId)
    {
        var query = new GetTeamMembersQuery(businessId);
        var members = await sender.Send(query);
        return Ok(members);
    }

    /// <summary>
    /// Removes a team member from a business.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <param name="teamMemberId">The ID of the team member to remove.</param>
    /// <returns>No content if the removal was successful.</returns>
    [HttpDelete("{teamMemberId}")]
    [EndpointName("Remove Team Member")]
    [EndpointSummary("Remove a member from a business team")]
    [EndpointDescription("Permanently removes a user from a business team")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> RemoveTeamMember([FromRoute] Guid businessId, [FromRoute] Guid teamMemberId)
    {
        var command = new RemoveTeamMemberCommand(teamMemberId);
        await sender.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Updates the role of a team member within a business.
    /// </summary>
    /// <param name="businessId">The ID of the business.</param>
    /// <param name="teamMemberId">The ID of the team member to update.</param>
    /// <param name="request">The role update details containing the new role ID.</param>
    /// <returns>No content if the update was successful.</returns>
    [HttpPut("{teamMemberId}/role")]
    [EndpointName("Update Team Member Role")]
    [EndpointSummary("Change a team member's role")]
    [EndpointDescription("Updates the permissions of a team member by assigning them a different role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(UpdateTeamMemberRoleRequest), typeof(UpdateTeamMemberRoleRequestExample))]
    public async Task<IActionResult> UpdateTeamMemberRole([FromRoute] Guid businessId, [FromRoute] Guid teamMemberId, [FromBody] UpdateTeamMemberRoleRequest request)
    {
        var command = new UpdateTeamMemberRoleCommand(teamMemberId, request.NewRoleId);
        await sender.Send(command);
        return NoContent();
    }
}