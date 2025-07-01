namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System.Security.Claims;

/// <summary>
/// Provides endpoints for managing the authenticated user's profile.
/// </summary>
[ApiController]
[Route("api/user")]
[Authorize]
[Tags("User Profile")]
public class UserController(IProfileService profileService, IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var id) ? id : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var user = await profileService.GetProfileAsync(userId);
        return Ok(user);
    }

    /// <summary>
    /// Updates the profile of the currently authenticated user.
    /// </summary>
    /// <param name="dto">The updated user details.</param>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        await profileService.UpdateProfileAsync(userId, dto);
        return NoContent();
    }
} 