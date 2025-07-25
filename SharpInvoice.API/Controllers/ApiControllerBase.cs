using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Base controller for API endpoints with standard configurations and helper methods.
/// </summary>
[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[EnableRateLimiting("api")]

public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID from the JWT claims.
    /// </summary>
    protected string CurrentUserId => User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? string.Empty;

    /// <summary>
    /// Gets the current user's email from the JWT claims.
    /// </summary>
    protected string CurrentUserEmail => User.FindFirst("email")?.Value ?? string.Empty;

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    /// <typeparam name="T">The response data type.</typeparam>
    /// <param name="data">The response data.</param>
    /// <returns>An action result with the data.</returns>
    protected IActionResult Success<T>(T data)
    {
        return Ok(data);
    }

    /// <summary>
    /// Creates a successful response with no content.
    /// </summary>
    /// <returns>A no content result.</returns>
    protected IActionResult Success()
    {
        return NoContent();
    }

    /// <summary>
    /// Creates a created response with data and location URI.
    /// </summary>
    /// <typeparam name="T">The response data type.</typeparam>
    /// <param name="data">The response data.</param>
    /// <param name="actionName">The action name for location header.</param>
    /// <param name="routeValues">The route values for location header.</param>
    /// <returns>A created result with location header and data.</returns>
    protected IActionResult Created<T>(T data, string actionName, object? routeValues = null)
    {
        return CreatedAtAction(actionName, routeValues, data);
    }

    /// <summary>
    /// Creates a created response with location URI.
    /// </summary>
    /// <param name="uri">The location URI.</param>
    /// <returns>A created result with location header.</returns>
    protected IActionResult Created(string uri)
    {
        return Created(uri, null!);
    }

    /// <summary>
    /// Creates a not found response with optional details.
    /// </summary>
    /// <param name="detail">The error detail message.</param>
    /// <returns>A not found result.</returns>
    protected IActionResult NotFound(string? detail = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource Not Found",
            Detail = detail ?? "The requested resource was not found."
        };

        return NotFound(problemDetails);
    }

    /// <summary>
    /// Creates a bad request response with validation details.
    /// </summary>
    /// <param name="detail">The error detail message.</param>
    /// <returns>A bad request result.</returns>
    protected IActionResult Invalid(string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = detail
        };

        return BadRequest(problemDetails);
    }

    /// <summary>
    /// Creates a forbidden response with details.
    /// </summary>
    /// <param name="detail">The error detail message.</param>
    /// <returns>A forbidden result.</returns>
    protected IActionResult Forbidden(string? detail = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Access Denied",
            Detail = detail ?? "You do not have permission to access this resource."
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    /// <summary>
    /// Creates an unauthorized response with details.
    /// </summary>
    /// <param name="detail">The error detail message.</param>
    /// <returns>An unauthorized result.</returns>
    protected IActionResult Unauthorized(string? detail = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication Required",
            Detail = detail ?? "You must be authenticated to access this resource."
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }
}