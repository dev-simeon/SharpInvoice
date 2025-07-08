namespace SharpInvoice.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Examples;
using SharpInvoice.Modules.UserManagement.Application.Commands;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Queries;
using Swashbuckle.AspNetCore.Filters;

/// <summary>
/// Provides endpoints for managing the authenticated user's profile.
/// </summary>
[ApiController]
[Route("api/user")]
[Authorize]
[Tags("User Profile")]
[Produces("application/json")]
public class UserController(ISender sender) : ApiControllerBase
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
        var query = new GetMyProfileQuery();
        var user = await sender.Send(query);
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
        var command = new UpdateMyProfileCommand(dto);
        await sender.Send(command);
        return NoContent();
    }
}