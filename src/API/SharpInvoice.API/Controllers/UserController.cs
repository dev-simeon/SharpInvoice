namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;

/// <summary>
/// Provides endpoints for managing user profiles.
/// </summary>
[ApiController]
[Route("api/user")]
[Authorize]
[Tags("User Profile")]
public class UserController(IProfileService profileService) : ApiControllerBase
{
    /// <summary>
    /// Gets the profile information for the currently authenticated user.
    /// </summary>
    /// <returns>The user's profile information.</returns>
    [HttpGet("me")]
    [EndpointName("Get My Profile")]
    [EndpointSummary("Get current user's profile")]
    [EndpointDescription("Retrieves the profile information for the authenticated user")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile()
    {
        var profile = await profileService.GetMyProfileAsync();
        return Ok(profile);
    }

    /// <summary>
    /// Updates the profile information for the currently authenticated user.
    /// </summary>
    /// <param name="profileDto">The updated profile information.</param>
    /// <returns>No content if the update was successful.</returns>
    [HttpPut("me")]
    [EndpointName("Update My Profile")]
    [EndpointSummary("Update current user's profile")]
    [EndpointDescription("Updates the profile information for the authenticated user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto profileDto)
    {
        await profileService.UpdateMyProfileAsync(profileDto);
        return NoContent();
    }
}