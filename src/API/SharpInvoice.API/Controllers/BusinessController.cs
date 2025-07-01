namespace SharpInvoice.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System.Net.Mime;
using System.Security.Claims;

/// <summary>
/// Provides endpoints for managing businesses.
/// </summary>
[ApiController]
[Route("api/businesses")]
[Authorize]
[Tags("Businesses")]
public class BusinessController(IBusinessService businessService, IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var id) ? id : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    /// <summary> Checks if a business name is available in a specific country. </summary>
    [AllowAnonymous]
    [HttpGet("is-name-available")]
    [EndpointSummary("Check if a business name is available.")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsNameAvailable([FromQuery] string name, [FromQuery] string country)
    {
        // This service method would need to be implemented
        // var isAvailable = await businessService.IsNameAvailableAsync(name, country);
        // return Ok(isAvailable);
        await Task.CompletedTask; // Placeholder
        return Ok(true);
    }

    /// <summary> Gets the details of the currently authenticated user's business. </summary>
    [HttpGet("details")]
    [EndpointSummary("Get business details.")]
    [ProducesResponseType<BusinessDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusinessDetails()
    {
        // You would get the businessId from the user's JWT claims
        var businessId = Guid.Parse(User.FindFirstValue("business_id")!);
        var details = await businessService.GetBusinessDetailsAsync(businessId);
        return Ok(details);
    }

    /// <summary> Updates the details of the currently authenticated user's business. </summary>
    [HttpPut("details")]
    [EndpointSummary("Update business details.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBusinessDetails([FromBody] UpdateBusinessDetailsDto dto)
    {
        var businessId = Guid.Parse(User.FindFirstValue("business_id")!);
        await businessService.UpdateBusinessDetailsAsync(businessId, dto);
        return NoContent();
    }

    /// <summary>
    /// Creates a new business for the authenticated user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await businessService.CreateBusinessForUserAsync(userId, request.Name, request.Email, request.Country);
        return CreatedAtAction(nameof(GetBusiness), new { businessId = result.Id }, result);
    }

    /// <summary>
    /// Gets the details of a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to retrieve.</param>
    [HttpGet("{businessId}")]
    [ProducesResponseType(typeof(BusinessDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusiness(Guid businessId)
    {
        var result = await businessService.GetBusinessDetailsAsync(businessId);
        return Ok(result);
    }

    /// <summary>
    /// Gets all businesses associated with the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BusinessDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyBusinesses()
    {
        var userId = GetCurrentUserId();
        var result = await businessService.GetBusinessesForUserAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Updates the details of a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to update.</param>
    /// <param name="dto">The new details for the business.</param>
    [HttpPut("{businessId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBusiness(Guid businessId, [FromBody] UpdateBusinessDetailsDto dto)
    {
        await businessService.UpdateBusinessDetailsAsync(businessId, dto);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to deactivate.</param>
    [HttpPost("{businessId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateBusiness(Guid businessId)
    {
        await businessService.DeactivateBusinessAsync(businessId);
        return NoContent();
    }

    /// <summary>
    /// Activates a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to activate.</param>
    [HttpPost("{businessId}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateBusiness(Guid businessId)
    {
        await businessService.ActivateBusinessAsync(businessId);
        return NoContent();
    }
}

/// <summary>
/// Represents the request to create a new business.
/// </summary>
/// <param name="Name">The name of the business.</param>
/// <param name="Email">The contact email for the business.</param>
/// <param name="Country">The country where the business is located.</param>
public record CreateBusinessRequest(string Name, string Email, string Country);