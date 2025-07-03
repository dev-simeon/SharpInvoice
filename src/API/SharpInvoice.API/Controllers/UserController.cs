namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using SharpInvoice.API.Examples;

/// <summary>
/// Provides endpoints for managing the authenticated user's profile.
/// </summary>
[ApiController]
[Route("api/user")]
[Authorize]
[Tags("User Profile")]
[Produces("application/json")]
public class UserController(IProfileService profileService, ICurrentUserProvider currentUserProvider) : ControllerBase
{
    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    /// <returns>The user's profile information including personal details.</returns>
    [HttpGet("me")]
    [EndpointName("Get Current User Profile")]
    [EndpointSummary("Get the current user's profile")]
    [EndpointDescription("Retrieves the profile information for the currently authenticated user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProfileDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ProfileDtoExample))]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var user = await profileService.GetProfileAsync(userId);
        return Ok(user);
    }

    /// <summary>
    /// Updates the profile of the currently authenticated user.
    /// </summary>
    /// <param name="dto">The updated user details including name, avatar, and contact information.</param>
    /// <returns>No content if the update was successful.</returns>
    [HttpPut("me")]
    [EndpointName("Update Current User Profile")]
    [EndpointSummary("Update the current user's profile")]
    [EndpointDescription("Updates the personal information and settings for the currently authenticated user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [SwaggerRequestExample(typeof(UpdateProfileDto), typeof(UpdateProfileDtoExample))]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        await profileService.UpdateProfileAsync(userId, dto);
        return NoContent();
    }
} 