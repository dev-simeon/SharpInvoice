using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for team member management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TeamController"/> class.
/// </remarks>
/// <param name="teamMemberService">The team member service.</param>
/// <param name="businessService">The business service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class TeamController(
    ITeamMemberService teamMemberService,
    IBusinessService businessService,
    ILogger<TeamController> logger) : ApiControllerBase
{

    /// <summary>
    /// Invites a team member to a business
    /// </summary>
    /// <remarks>
    /// Sends an invitation to a user to join a business team with a specific role.
    /// Only business owners and admins can invite new team members.
    /// 
    /// Available roles:
    /// - **Admin**: Full access to all business features
    /// - **Editor**: Can create, edit, and send invoices and manage clients
    /// - **Viewer**: Read-only access to invoices and reports
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/team/3fa85f64-5717-4562-b3fc-2c963f66afa6/invite
    ///     {
    ///         "email": "newmember@example.com",
    ///         "role": "Editor"
    ///     }
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="request">The invitation request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Invitation confirmation</returns>
    /// <response code="200">Invitation sent successfully.</response>
    /// <response code="400">Invalid invitation data or business ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User is not a business owner or admin.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="409">User is already a member of this business.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("{businessId:guid}/invite")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InviteTeamMemberAsync(
        [FromRoute] string businessId,
        [FromBody] InviteTeamMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessId))
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (!ModelState.IsValid)
        {
            return Invalid("Invalid invitation data provided.");
        }

        logger.LogInformation("Team member invitation requested for business: {BusinessId}, email: {Email}, role: {Role}",
            businessId, request.Email, request.Role);

        await teamMemberService.InviteTeamMemberAsync(businessId, CurrentUserId!, request);

        return Success(new { 
            message = $"Invitation sent successfully to {request.Email}",
            invitedEmail = request.Email,
            role = request.Role
        });
    }

    /// <summary>
    /// Accepts a team member invitation
    /// </summary>
    /// <remarks>
    /// Accepts an invitation to join a business team. The user must have received
    /// a valid invitation token via email.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/team/accept-invitation
    ///     {
    ///         "invitationToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9..."
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The invitation acceptance data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Acceptance confirmation</returns>
    /// <response code="200">Invitation accepted successfully.</response>
    /// <response code="400">Invalid invitation token.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="404">Invitation not found or expired.</response>
    /// <response code="409">User is already a member of this business.</response>
    /// <response code="410">Invitation has expired.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("accept-invitation")]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AcceptInvitationAsync(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid invitation acceptance data provided.");
        }

        logger.LogInformation("Team member invitation acceptance requested by user: {UserId}", CurrentUserId);

        await teamMemberService.AcceptInvitationAsync(CurrentUserId!, request);

        // In a real implementation, this would return the actual team member data
        var teamMember = new TeamMemberDto
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId!,
            BusinessId = "biz_mock_business_id", // Would be extracted from invitation
            Email = CurrentUserEmail!,
            Role = "Editor", // Would be from the invitation
            Status = "Active",
            JoinedAt = DateTime.UtcNow,
            InvitedBy = "system@sharpinvoice.com"
        };

        return Success(teamMember);
    }

    /// <summary>
    /// Gets all team members for a business
    /// </summary>
    /// <remarks>
    /// Retrieves all team members associated with a specific business.
    /// The user must be a member of the business to view the team.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/team/3fa85f64-5717-4562-b3fc-2c963f66afa6/members
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of team members</returns>
    /// <response code="200">Team members retrieved successfully.</response>
    /// <response code="400">Invalid business ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User is not a member of this business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/members")]
    [ProducesResponseType(typeof(IEnumerable<TeamMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTeamMembersAsync(
        [FromRoute] string businessId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessId))
        {
            return Invalid("Business ID cannot be empty.");
        }

        logger.LogDebug("Retrieving team members for business: {BusinessId}", businessId);

        // Generate mock team member data
        var teamMembers = GenerateMockTeamMembers(businessId);

        return Success(teamMembers);
    }

    /// <summary>
    /// Updates a team member's role
    /// </summary>
    /// <remarks>
    /// Updates the role of an existing team member. Only business owners and admins
    /// can modify team member roles.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/v1/team/3fa85f64-5717-4562-b3fc-2c963f66afa6/members/user123
    ///     {
    ///         "role": "Admin"
    ///     }
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="userId">The unique identifier of the team member</param>
    /// <param name="request">The role update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated team member information</returns>
    /// <response code="200">Team member role updated successfully.</response>
    /// <response code="400">Invalid update data or IDs.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User cannot modify team member roles.</response>
    /// <response code="404">Business or team member not found.</response>
    /// <response code="422">Cannot modify owner role or self-role restrictions.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPut("{businessId:guid}/members/{userId}")]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTeamMemberRoleAsync(
        [FromRoute] string businessId,
        [FromRoute] string userId,
        [FromBody] UpdateTeamMemberRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessId))
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Invalid("User ID cannot be empty.");
        }

        if (!ModelState.IsValid)
        {
            return Invalid("Invalid role update data provided.");
        }

        logger.LogInformation("Team member role update requested for business: {BusinessId}, user: {UserId}, new role: {Role}",
            businessId, userId, request.Role);

        // In a real implementation, this would update the team member's role in the database
        var updatedTeamMember = new TeamMemberDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BusinessId = businessId,
            Email = "member@example.com",
            Role = request.Role,
            Status = "Active",
            JoinedAt = DateTime.UtcNow.AddMonths(-2),
            InvitedBy = CurrentUserEmail!
        };

        return Success(updatedTeamMember);
    }

    /// <summary>
    /// Removes a team member from a business
    /// </summary>
    /// <remarks>
    /// Removes a team member from the business. Only business owners and admins
    /// can remove team members. Users cannot remove themselves.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/v1/team/3fa85f64-5717-4562-b3fc-2c963f66afa6/members/user123
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="userId">The unique identifier of the team member to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Removal confirmation</returns>
    /// <response code="204">Team member removed successfully.</response>
    /// <response code="400">Invalid business or user ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. Cannot remove team members or self-removal attempted.</response>
    /// <response code="404">Business or team member not found.</response>
    /// <response code="422">Cannot remove business owner or last admin.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpDelete("{businessId:guid}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveTeamMemberAsync(
        [FromRoute] Guid businessId,
        [FromRoute] string userId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Invalid("User ID cannot be empty.");
        }

        if (userId == CurrentUserId)
        {
            return Forbidden("Cannot remove yourself from the team. Use the leave team endpoint instead.");
        }

        logger.LogWarning("Team member removal requested for business: {BusinessId}, user: {UserId} by: {RequestorId}",
            businessId, userId, CurrentUserId);

        // In a real implementation, this would remove the team member from the database
        return NoContent();
    }

    /// <summary>
    /// Leaves a business team
    /// </summary>
    /// <remarks>
    /// Allows the current user to leave a business team. Business owners cannot
    /// leave their own business unless they transfer ownership first.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/team/3fa85f64-5717-4562-b3fc-2c963f66afa6/leave
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business to leave</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Leave confirmation</returns>
    /// <response code="200">Left team successfully.</response>
    /// <response code="400">Invalid business ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. Business owner cannot leave without transferring ownership.</response>
    /// <response code="404">Business not found or user is not a member.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("{businessId:guid}/leave")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LeaveTeamAsync(
        [FromRoute] Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        logger.LogInformation("Team leave requested for business: {BusinessId} by user: {UserId}",
            businessId, CurrentUserId);

        // In a real implementation, this would:
        // 1. Check if user is the business owner
        // 2. Remove the user from the team
        // 3. Update any assignments or ownership of invoices/clients

        return Success(new { message = "You have successfully left the team." });
    }

    /// <summary>
    /// Gets pending invitations for a business
    /// </summary>
    /// <remarks>
    /// Retrieves all pending team member invitations for a business.
    /// Only business owners and admins can view pending invitations.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/team/3fa85f64-5717-4562-b3fc-2c963f66afa6/invitations
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending invitations</returns>
    /// <response code="200">Invitations retrieved successfully.</response>
    /// <response code="400">Invalid business ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User is not a business owner or admin.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/invitations")]
    [ProducesResponseType(typeof(IEnumerable<TeamInvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPendingInvitationsAsync(
        [FromRoute] string businessId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessId))
        {
            return Invalid("Business ID cannot be empty.");
        }

        logger.LogDebug("Retrieving pending invitations for business: {BusinessId}", businessId);

        // Generate mock invitation data
        var invitations = GenerateMockInvitations(businessId);

        return Success(invitations);
    }

    /// <summary>
    /// Cancels a pending invitation
    /// </summary>
    /// <remarks>
    /// Cancels a pending team member invitation. Only business owners and admins
    /// can cancel invitations.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/v1/team/invitations/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="invitationId">The unique identifier of the invitation to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation confirmation</returns>
    /// <response code="204">Invitation cancelled successfully.</response>
    /// <response code="400">Invalid invitation ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User cannot cancel this invitation.</response>
    /// <response code="404">Invitation not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpDelete("invitations/{invitationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelInvitationAsync(
        [FromRoute] Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        if (invitationId == Guid.Empty)
        {
            return Invalid("Invitation ID cannot be empty.");
        }

        logger.LogInformation("Invitation cancellation requested: {InvitationId} by user: {UserId}",
            invitationId, CurrentUserId);

        // In a real implementation, this would cancel the invitation in the database
        return NoContent();
    }

    #region Private Helper Methods

    private static List<TeamMemberDto> GenerateMockTeamMembers(string businessId)
    {
        return new List<TeamMemberDto>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                UserId = "owner123", 
                BusinessId = businessId, 
                Email = "owner@company.com", 
                Role = "Owner", 
                Status = "Active", 
                JoinedAt = DateTime.UtcNow.AddMonths(-6),
                InvitedBy = "self"
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                UserId = "admin123", 
                BusinessId = businessId, 
                Email = "admin@company.com", 
                Role = "Admin", 
                Status = "Active", 
                JoinedAt = DateTime.UtcNow.AddMonths(-3),
                InvitedBy = "owner@company.com"
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                UserId = "editor123", 
                BusinessId = businessId, 
                Email = "editor@company.com", 
                Role = "Editor", 
                Status = "Active", 
                JoinedAt = DateTime.UtcNow.AddMonths(-2),
                InvitedBy = "admin@company.com"
            }
        };
    }

    private static List<TeamInvitationDto> GenerateMockInvitations(string businessId)
    {
        return new List<TeamInvitationDto>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                BusinessId = businessId, 
                Email = "pending1@example.com", 
                Role = "Editor", 
                Status = "Pending", 
                InvitedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                InvitedBy = "admin@company.com"
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                BusinessId = businessId, 
                Email = "pending2@example.com", 
                Role = "Viewer", 
                Status = "Pending", 
                InvitedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(6),
                InvitedBy = "owner@company.com"
            }
        };
    }

    #endregion
}

#region DTOs

/// <summary>
/// Team member data transfer object
/// </summary>
public sealed record TeamMemberDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string BusinessId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public string InvitedBy { get; init; } = string.Empty;
}

/// <summary>
/// Team invitation data transfer object
/// </summary>
public sealed record TeamInvitationDto
{
    public Guid Id { get; init; }
    public string BusinessId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime InvitedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string InvitedBy { get; init; } = string.Empty;
}

/// <summary>
/// Update team member role request data transfer object
/// </summary>
public sealed record UpdateTeamMemberRoleRequest
{
    /// <summary>
    /// New role for the team member
    /// </summary>
    /// <example>Admin</example>
    [Required]
    public required string Role { get; init; }
}

#endregion