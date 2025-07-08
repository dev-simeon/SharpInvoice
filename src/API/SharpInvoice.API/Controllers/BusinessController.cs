namespace SharpInvoice.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Examples;
using SharpInvoice.Modules.UserManagement.Application.Commands;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Queries;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;

/// <summary>
/// Provides endpoints for managing businesses.
/// </summary>
[ApiController]
[Route("api/business")]
[Authorize]
[Tags("Businesses")]
public class BusinessController(ISender sender) : ApiControllerBase
{

    /// <summary>
    /// Checks if a business name is available in a specific country.
    /// </summary>
    /// <param name="name">The business name to check for availability.</param>
    /// <param name="country">The country code where the business would be registered.</param>
    /// <returns>True if the business name is available, false otherwise.</returns>
    [AllowAnonymous]
    [HttpGet("is-name-available")]
    [EndpointName("Check Business Name Availability")]
    [EndpointSummary("Check if a business name is available")]
    [EndpointDescription("Checks if the specified business name is available for registration in a particular country")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<IActionResult> IsNameAvailable([FromQuery] string name, [FromQuery] string country)
    {
        var query = new IsBusinessNameAvailableQuery(name, country);
        var isAvailable = await sender.Send(query);
        return Ok(isAvailable);
    }

    /// <summary>
    /// Gets the details of the currently authenticated user's business.
    /// </summary>
    /// <returns>Detailed information about the user's business.</returns>
    [HttpGet("details")]
    [EndpointName("Get Current Business Details")]
    [EndpointSummary("Get business details for current user")]
    [EndpointDescription("Retrieves detailed information about the authenticated user's business")]
    [ProducesResponseType<BusinessDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BusinessDetailsDtoExample))]
    public async Task<IActionResult> GetBusinessDetails()
    {
        var businessId = Guid.Parse(User.FindFirstValue("business_id")!);
        var query = new GetBusinessDetailsQuery(businessId);
        var details = await sender.Send(query);
        return Ok(details);
    }

    /// <summary>
    /// Updates the details of the currently authenticated user's business.
    /// </summary>
    /// <param name="dto">The updated business details.</param>
    /// <returns>No content if the update was successful.</returns>
    [HttpPut("details")]
    [EndpointName("Update Current Business Details")]
    [EndpointSummary("Update business details for current user")]
    [EndpointDescription("Updates the information for the authenticated user's business")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(UpdateBusinessDetailsDto), typeof(UpdateBusinessDetailsDtoExample))]
    public async Task<IActionResult> UpdateBusinessDetails([FromBody] UpdateBusinessDetailsDto dto)
    {
        var businessId = Guid.Parse(User.FindFirstValue("business_id")!);
        var command = new UpdateBusinessDetailsCommand(businessId, dto);
        await sender.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Creates a new business for the authenticated user.
    /// </summary>
    /// <param name="request">The business creation details including name, email, and country.</param>
    /// <returns>The newly created business information.</returns>
    [HttpPost]
    [EndpointName("Create Business")]
    [EndpointSummary("Create a new business")]
    [EndpointDescription("Creates a new business for the currently authenticated user")]
    [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [SwaggerRequestExample(typeof(CreateBusinessRequest), typeof(CreateBusinessRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(BusinessDtoExample))]
    public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessRequest request)
    {
        var command = new CreateBusinessCommand(request);
        var result = await sender.Send(command);
        return CreatedAtAction(nameof(GetBusiness), new { businessId = result.Id }, result);
    }

    /// <summary>
    /// Gets the details of a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to retrieve.</param>
    /// <returns>Detailed information about the requested business.</returns>
    [HttpGet("{businessId}")]
    [EndpointName("Get Business Details")]
    [EndpointSummary("Get details for a specific business")]
    [EndpointDescription("Retrieves comprehensive information about a specific business by ID")]
    [ProducesResponseType(typeof(BusinessDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BusinessDetailsDtoExample))]
    public async Task<IActionResult> GetBusiness([FromRoute] Guid businessId)
    {
        var query = new GetBusinessDetailsQuery(businessId);
        var result = await sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets all businesses associated with the authenticated user.
    /// </summary>
    /// <returns>A list of businesses the user owns or is a member of.</returns>
    [HttpGet]
    [EndpointName("Get User's Businesses")]
    [EndpointSummary("Get all businesses for current user")]
    [EndpointDescription("Retrieves a list of all businesses that the authenticated user owns or is a member of")]
    [ProducesResponseType(typeof(IEnumerable<BusinessDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BusinessListExample))]
    public async Task<IActionResult> GetMyBusinesses()
    {
        var query = new GetMyBusinessesQuery();
        var result = await sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Updates the details of a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to update.</param>
    /// <param name="dto">The new details for the business.</param>
    /// <returns>No content if the update was successful.</returns>
    [HttpPut("{businessId}")]
    [EndpointName("Update Business")]
    [EndpointSummary("Update a specific business")]
    [EndpointDescription("Updates the details of a specific business by ID")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(UpdateBusinessDetailsDto), typeof(UpdateBusinessDetailsDtoExample))]
    public async Task<IActionResult> UpdateBusiness([FromRoute] Guid businessId, [FromBody] UpdateBusinessDetailsDto dto)
    {
        var command = new UpdateBusinessDetailsCommand(businessId, dto);
        await sender.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Uploads a new logo for a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to update.</param>
    /// <param name="logo">The logo file to upload.</param>
    /// <returns>No content if the upload was successful.</returns>
    [HttpPost("{businessId}/logo")]
    [EndpointName("Upload Business Logo")]
    [EndpointSummary("Upload logo for a business")]
    [EndpointDescription("Uploads and associates a logo image with a specific business")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBusinessLogo([FromRoute] Guid businessId, [FromForm] IFormFile logo)
    {
        if (logo == null || logo.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var command = new UploadBusinessLogoCommand(businessId, logo.OpenReadStream(), logo.FileName);
        await sender.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to deactivate.</param>
    /// <returns>No content if the deactivation was successful.</returns>
    [HttpPost("{businessId}/deactivate")]
    [EndpointName("Deactivate Business")]
    [EndpointSummary("Deactivate a business")]
    [EndpointDescription("Marks a specific business as inactive without deleting it")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = "ManageBusiness")]
    public async Task<IActionResult> DeactivateBusiness([FromRoute] Guid businessId)
    {
        var command = new DeactivateBusinessCommand(businessId);
        await sender.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Activates a specific business.
    /// </summary>
    /// <param name="businessId">The ID of the business to activate.</param>
    /// <returns>No content if the activation was successful.</returns>
    [HttpPost("{businessId}/activate")]
    [EndpointName("Activate Business")]
    [EndpointSummary("Activate a business")]
    [EndpointDescription("Marks a previously deactivated business as active again")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = "ManageBusiness")]
    public async Task<IActionResult> ActivateBusiness([FromRoute] Guid businessId)
    {
        var command = new ActivateBusinessCommand(businessId);
        await sender.Send(command);
        return NoContent();
    }
}