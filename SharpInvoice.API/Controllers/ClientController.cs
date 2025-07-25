using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for client management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ClientController"/> class.
/// </remarks>
/// <param name="clientService">The client service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class ClientController(IClientService clientService, ILogger<ClientController> logger) : ApiControllerBase
{

    /// <summary>
    /// Creates a new client
    /// </summary>
    /// <remarks>
    /// Creates a new client for a specific business. The user must own the business.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/client
    ///     {
    ///         "businessId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "name": "Acme Corporation",
    ///         "email": "billing@acme.com",
    ///         "phone": "+1-555-0123",
    ///         "address": "123 Business Street, New York, NY 10001",
    ///         "country": "United States"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The client creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created client</returns>
    /// <response code="201">Client created successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="409">Client name already exists for this business.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateClientAsync(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid client data provided.");
        }

        logger.LogInformation("Creating client: {ClientName} for business: {BusinessId}", 
            request.Name, request.BusinessId);

        var result = await clientService.CreateClientAsync(request);

        return CreatedAtAction(nameof(GetClientByIdAsync), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a client by its unique identifier
    /// </summary>
    /// <remarks>
    /// Gets a specific client by its ID. The user must have access to the business that owns the client.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/client/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the client</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The client details</returns>
    /// <response code="200">Client retrieved successfully.</response>
    /// <response code="400">Invalid client ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the client.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClientByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Invalid("Client ID cannot be empty.");
        }

        logger.LogDebug("Retrieving client: {ClientId}", id);

        var client = await clientService.GetClientByIdAsync(id);

        if (client == null)
        {
            return NotFound($"Client with ID '{id}' was not found.");
        }

        return Success(client);
    }

    /// <summary>
    /// Retrieves all clients for a specific business
    /// </summary>
    /// <remarks>
    /// Gets all clients associated with a business. The user must own the business.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/client/business/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of clients for the business</returns>
    /// <response code="200">Clients retrieved successfully.</response>
    /// <response code="400">Invalid business ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetClientsByBusinessAsync(
        [FromRoute] Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        logger.LogDebug("Retrieving clients for business: {BusinessId}", businessId);

        var clients = await clientService.GetClientsByBusinessAsync(businessId);

        return Success(clients);
    }

    /// <summary>
    /// Updates an existing client
    /// </summary>
    /// <remarks>
    /// Updates an existing client with new information. The user must own the business.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/v1/client/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///         "businessId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "name": "Acme Corporation Ltd",
    ///         "email": "accounting@acme.com",
    ///         "phone": "+1-555-0124",
    ///         "address": "456 Updated Street, New York, NY 10002",
    ///         "country": "United States"
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the client to update</param>
    /// <param name="request">The updated client data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated client</returns>
    /// <response code="200">Client updated successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the client.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="409">Client name already exists for this business.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateClientAsync(
        [FromRoute] Guid id,
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Invalid("Client ID cannot be empty.");
        }

        if (!ModelState.IsValid)
        {
            return Invalid("Invalid client data provided.");
        }

        logger.LogInformation("Updating client: {ClientId}", id);

        var result = await clientService.UpdateClientAsync(id, request);

        return Success(result);
    }

    /// <summary>
    /// Deletes a client
    /// </summary>
    /// <remarks>
    /// Permanently deletes a client. This action cannot be undone.
    /// All associated invoices will be unlinked from this client.
    /// 
    /// **Warning:** This is a destructive operation that cannot be reversed.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/v1/client/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the client to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="204">Client deleted successfully.</response>
    /// <response code="400">Invalid client ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own this client.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="422">Client cannot be deleted due to existing dependencies.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteClientAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Invalid("Client ID cannot be empty.");
        }

        logger.LogWarning("Deleting client: {ClientId} by user: {UserId}", id, CurrentUserId);

        await clientService.DeleteClientAsync(id);

        return NoContent();
    }
}