namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

[ApiController]
[Route("api/businesses/{businessId}/team")]
[Authorize]
[Tags("Team Management")]
public class TeamMemberController : ControllerBase
{
    private readonly ITeamMemberService _teamMemberService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TeamMemberController(ITeamMemberService teamMemberService, IHttpContextAccessor httpContextAccessor)
    {
        _teamMemberService = teamMemberService;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var id) ? id : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    [HttpPost("invite")]
    public async Task<IActionResult> InviteTeamMember(Guid businessId, [FromBody] InviteTeamMemberRequest request)
    {
        await _teamMemberService.InviteTeamMemberAsync(businessId, request.Email, request.RoleId);
        return Ok();
    }

    [HttpGet("accept-invitation")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation([FromQuery] string token)
    {
        await _teamMemberService.AcceptInvitationAsync(token);
        return Ok("Invitation accepted successfully.");
    }

    [HttpGet]
    public async Task<IActionResult> GetTeamMembers(Guid businessId)
    {
        var members = await _teamMemberService.GetTeamMembersForBusinessAsync(businessId);
        return Ok(members);
    }

    [HttpDelete("{teamMemberId}")]
    public async Task<IActionResult> RemoveTeamMember(Guid businessId, Guid teamMemberId)
    {
        await _teamMemberService.RemoveTeamMemberAsync(teamMemberId);
        return NoContent();
    }

    [HttpPut("{teamMemberId}/role")]
    public async Task<IActionResult> UpdateTeamMemberRole(Guid businessId, Guid teamMemberId, [FromBody] UpdateTeamMemberRoleRequest request)
    {
        await _teamMemberService.UpdateTeamMemberRoleAsync(teamMemberId, request.NewRoleId);
        return NoContent();
    }
}

public record InviteTeamMemberRequest(string Email, Guid RoleId);
public record UpdateTeamMemberRoleRequest(Guid NewRoleId); 