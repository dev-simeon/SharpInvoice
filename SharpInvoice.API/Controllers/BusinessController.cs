using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for business management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BusinessController"/> class.
/// </remarks>
/// <param name="businessService">The business service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class BusinessController(IBusinessService businessService, ILogger<BusinessController> logger) : ApiControllerBase
{

    /// <summary>
    /// Creates a new business profile
    /// </summary>
    /// <remarks>
    /// Creates a new business profile for the authenticated user. Each user can have multiple businesses.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/business
    ///     {
    ///         "name": "Acme Consulting LLC",
    ///         "email": "contact@acmeconsulting.com",
    ///         "phone": "+1-555-0123",
    ///         "website": "https://acmeconsulting.com",
    ///         "taxId": "12-3456789",
    ///         "address": {
    ///             "street": "123 Business Ave",
    ///             "city": "New York",
    ///             "state": "NY",
    ///             "postalCode": "10001",
    ///             "country": "United States"
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The business creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created business profile</returns>
    /// <response code="201">Business created successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="409">Business name already exists for this user.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBusinessAsync(
        [FromBody] CreateBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid business data provided.");
        }

        logger.LogInformation("Creating business: {BusinessName} for user: {UserId}", 
            request.Name, CurrentUserId);

        var result = await businessService.CreateBusinessAsync(CurrentUserId!, request, cancellationToken);

        return CreatedAtAction(nameof(GetBusinessByIdAsync), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a business by its unique identifier
    /// </summary>
    /// <remarks>
    /// Gets a specific business by its ID. The user must own the business to access it.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/business/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the business</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The business details</returns>
    /// <response code="200">Business retrieved successfully.</response>
    /// <response code="400">Invalid business ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own this business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBusinessByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Invalid("Business ID cannot be empty.");
        }

        logger.LogDebug("Retrieving business: {BusinessId}", id);

        var business = await businessService.GetBusinessByIdAsync(id, cancellationToken);

        if (business == null)
        {
            return NotFound($"Business with ID '{id}' was not found.");
        }

        return Success(business);
    }

    /// <summary>
    /// Retrieves all businesses owned by the current user
    /// </summary>
    /// <remarks>
    /// Gets all business profiles associated with the authenticated user account.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/business/my-businesses
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's businesses</returns>
    /// <response code="200">Businesses retrieved successfully.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("my-businesses")]
    [ProducesResponseType(typeof(IEnumerable<BusinessDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserBusinessesAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Retrieving businesses for user: {UserId}", CurrentUserId);

        var businesses = await businessService.GetBusinessesByUserIdAsync(CurrentUserId!, cancellationToken);

        return Success(businesses);
    }

    /// <summary>
    /// Updates an existing business profile
    /// </summary>
    /// <remarks>
    /// Updates an existing business profile with new information. The user must own the business.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/v1/business/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///         "name": "Acme Consulting LLC",
    ///         "email": "info@acmeconsulting.com",
    ///         "phone": "+1-555-0123",
    ///         "website": "https://acmeconsulting.com",
    ///         "taxId": "12-3456789",
    ///         "address": {
    ///             "street": "456 Updated Ave",
    ///             "city": "New York",
    ///             "state": "NY",
    ///             "postalCode": "10002",
    ///             "country": "United States"
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the business to update</param>
    /// <param name="request">The updated business data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated business profile</returns>
    /// <response code="200">Business updated successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own this business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="409">Business name already exists for this user.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateBusinessAsync(
        [FromRoute] string id,
        [FromBody] CreateBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (!ModelState.IsValid)
        {
            return Invalid("Invalid business data provided.");
        }

        logger.LogInformation("Updating business: {BusinessId}", id);

        var result = await businessService.UpdateBusinessAsync(id, request, cancellationToken);

        return Success(result);
    }

    /// <summary>
    /// Deletes a business profile
    /// </summary>
    /// <remarks>
    /// Permanently deletes a business profile. This action cannot be undone.
    /// All associated invoices, clients, and data will also be deleted.
    /// 
    /// **Warning:** This is a destructive operation that cannot be reversed.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/v1/business/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the business to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="204">Business deleted successfully.</response>
    /// <response code="400">Invalid business ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own this business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="422">Business cannot be deleted due to existing dependencies.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBusinessAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Invalid("Business ID cannot be empty.");
        }

        logger.LogWarning("Deleting business: {BusinessId} by user: {UserId}", id, CurrentUserId);

        await businessService.DeleteBusinessAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Uploads or updates the business logo
    /// </summary>
    /// <remarks>
    /// Uploads a new logo image for the business. Supported formats: PNG, JPG, JPEG.
    /// Maximum file size: 5MB. Recommended dimensions: 400x400 pixels.
    /// 
    /// Sample request (multipart/form-data):
    /// 
    ///     POST /api/v1/business/3fa85f64-5717-4562-b3fc-2c963f66afa6/logo
    ///     Content-Type: multipart/form-data
    ///     Authorization: Bearer {your-access-token}
    ///     
    ///     [file upload data]
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the business</param>
    /// <param name="logoFile">The logo image file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload confirmation with logo URL</returns>
    /// <response code="200">Logo uploaded successfully.</response>
    /// <response code="400">Invalid file format, size, or business ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own this business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="413">File size too large.</response>
    /// <response code="422">Invalid file format or corrupt file.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("{id:guid}/logo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413RequestEntityTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadBusinessLogoAsync(
        [FromRoute] string id,
        [FromForm, Required] IFormFile logoFile,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (logoFile == null || logoFile.Length == 0)
        {
            return Invalid("Logo file is required.");
        }

        // Validate file size (5MB max)
        if (logoFile.Length > 5 * 1024 * 1024)
        {
            return StatusCode(StatusCodes.Status413RequestEntityTooLarge, 
                new ProblemDetails 
                { 
                    Status = StatusCodes.Status413RequestEntityTooLarge,
                    Title = "File Too Large",
                    Detail = "Logo file must be smaller than 5MB."
                });
        }

        // Validate file type
        var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg" };
        if (!allowedTypes.Contains(logoFile.ContentType.ToLowerInvariant()))
        {
            return BadRequest("Only PNG, JPG, and JPEG files are supported.");
        }

        logger.LogInformation("Uploading logo for business: {BusinessId}, file size: {FileSize}", 
            id, logoFile.Length);

        var logoUrl = await businessService.UploadBusinessLogoAsync(id, logoFile, cancellationToken);

        return Success(new { LogoUrl = logoUrl, Message = "Logo uploaded successfully." });
    }

    /// <summary>
    /// Updates business branding and theme settings
    /// </summary>
    /// <remarks>
    /// Updates the business branding including color scheme, theme settings, and visual customizations.
    /// This endpoint allows setting custom colors, fonts, and other branding elements.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/v1/business/3fa85f64-5717-4562-b3fc-2c963f66afa6/branding
    ///     Authorization: Bearer {your-access-token}
    ///     {
    ///         "logoUrl": "https://mycompany.com/logo.png",
    ///         "themeSettings": {
    ///             "primaryColor": "#007bff",
    ///             "secondaryColor": "#6c757d",
    ///             "accentColor": "#28a745",
    ///             "fontFamily": "Roboto, sans-serif",
    ///             "headerBackgroundColor": "#f8f9fa",
    ///             "textColor": "#212529"
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the business</param>
    /// <param name="request">The branding update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated business information</returns>
    /// <response code="200">Branding updated successfully.</response>
    /// <response code="400">Invalid business ID or branding data.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own this business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPut("{id:guid}/branding")]
    [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateBusinessBrandingAsync(
        [FromRoute] string id,
        [FromBody] UpdateBusinessBrandingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (!ModelState.IsValid)
        {
            return Invalid("Invalid branding data provided.");
        }

        logger.LogInformation("Updating branding for business: {BusinessId}", id);

        var result = await businessService.UpdateBusinessBrandingAsync(id, request, cancellationToken);

        logger.LogInformation("Branding updated successfully for business: {BusinessId}", id);

        return Success(result);
    }

    /// <summary>
    /// Gets business statistics and metrics
    /// </summary>
    /// <remarks>
    /// Retrieves key business metrics including total invoices, revenue, and client count.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/business/3fa85f64-5717-4562-b3fc-2c963f66afa6/stats?period=month
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the business</param>
    /// <param name="period">Time period for statistics (month, quarter, year)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business statistics and metrics</returns>
    /// <response code="200">Statistics retrieved successfully.</response>
    /// <response code="400">Invalid business ID or period parameter.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own this business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{id:guid}/stats")]
    [ProducesResponseType(typeof(BusinessStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBusinessStatsAsync(
        [FromRoute] string id,
        [FromQuery] string period = "month",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Invalid("Business ID cannot be empty.");
        }

        var validPeriods = new[] { "month", "quarter", "year" };
        if (!validPeriods.Contains(period.ToLowerInvariant()))
        {
            return Invalid("Period must be one of: month, quarter, year.");
        }

        logger.LogDebug("Retrieving statistics for business: {BusinessId}, period: {Period}", id, period);

        var stats = await businessService.GetBusinessStatsAsync(id, period, cancellationToken);

        return Success(stats);
    }
}

/// <summary>
/// Business statistics data transfer object
/// </summary>
public sealed record BusinessStatsDto
{
    /// <summary>
    /// Total number of invoices
    /// </summary>
    public int TotalInvoices { get; init; }

    /// <summary>
    /// Total revenue amount
    /// </summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>
    /// Number of paid invoices
    /// </summary>
    public int PaidInvoices { get; init; }

    /// <summary>
    /// Number of pending invoices
    /// </summary>
    public int PendingInvoices { get; init; }

    /// <summary>
    /// Number of overdue invoices
    /// </summary>
    public int OverdueInvoices { get; init; }

    /// <summary>
    /// Total number of clients
    /// </summary>
    public int TotalClients { get; init; }

    /// <summary>
    /// Average invoice amount
    /// </summary>
    public decimal AverageInvoiceAmount { get; init; }

    /// <summary>
    /// Time period for these statistics
    /// </summary>
    public string Period { get; init; } = string.Empty;
}