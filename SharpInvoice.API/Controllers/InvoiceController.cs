using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for invoice management operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class InvoiceController : ApiControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoiceController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceController"/> class.
    /// </summary>
    /// <param name="invoiceService">The invoice service.</param>
    /// <param name="logger">The logger.</param>
    public InvoiceController(IInvoiceService invoiceService, ILogger<InvoiceController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new invoice
    /// </summary>
    /// <remarks>
    /// Creates a new invoice for a specific business and client. The invoice number is automatically generated.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/invoice
    ///     {
    ///         "businessId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    ///         "currency": "USD",
    ///         "issueDate": "2024-01-15T00:00:00Z",
    ///         "dueDate": "2024-02-15T00:00:00Z",
    ///         "notes": "Payment terms: Net 30",
    ///         "terms": "Standard terms and conditions",
    ///         "items": [
    ///             {
    ///                 "description": "Web Development Services",
    ///                 "quantity": 40,
    ///                 "unitPrice": 150.00,
    ///                 "unit": "hours"
    ///             }
    ///         ]
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The invoice creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created invoice with generated invoice number</returns>
    /// <response code="201">Invoice created successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business or client.</response>
    /// <response code="404">Business or client not found.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateInvoiceAsync(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid invoice data provided.");
        }

        _logger.LogInformation("Creating invoice for business: {BusinessId}, client: {ClientId}", 
            request.BusinessId, request.ClientId);

        var result = await _invoiceService.CreateInvoiceAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetInvoiceByIdAsync), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves an invoice by its unique identifier
    /// </summary>
    /// <remarks>
    /// Gets a specific invoice by its ID. The user must have access to the business that owns the invoice.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The invoice details</returns>
    /// <response code="200">Invoice retrieved successfully.</response>
    /// <response code="400">Invalid invoice ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the invoice.</response>
    /// <response code="404">Invoice not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInvoiceByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        _logger.LogDebug("Retrieving invoice: {InvoiceId}", id);

        var invoice = await _invoiceService.GetInvoiceByIdAsync(id, cancellationToken);

        if (invoice == null)
        {
            return NotFound($"Invoice with ID '{id}' was not found.");
        }

        return Success(invoice);
    }

    /// <summary>
    /// Retrieves all invoices for a specific business
    /// </summary>
    /// <remarks>
    /// Gets a paginated list of invoices for a business. Supports filtering and sorting options.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/invoice/business/3fa85f64-5717-4562-b3fc-2c963f66afa6?page=1&amp;pageSize=10&amp;status=draft&amp;sortBy=issueDate&amp;sortOrder=desc
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="status">Filter by invoice status (optional)</param>
    /// <param name="sortBy">Field to sort by (issueDate, dueDate, total, status)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="searchTerm">Search term for invoice number or client name (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of invoices</returns>
    /// <response code="200">Invoices retrieved successfully.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInvoicesByBusinessAsync(
        [FromRoute] string businessId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string sortBy = "issueDate",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessId))
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (page < 1)
        {
            return Invalid("Page number must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return Invalid("Page size must be between 1 and 100.");
        }

        _logger.LogDebug("Retrieving invoices for business: {BusinessId}, page: {Page}, pageSize: {PageSize}", 
            businessId, page, pageSize);

        var invoices = await _invoiceService.GetInvoicesByBusinessAsync(businessId, cancellationToken);

        // Note: In a real implementation, you'd apply pagination, filtering, and sorting here
        // For now, returning all invoices as a simple enumerable
        var result = new PagedResult<InvoiceDto>
        {
            Items = invoices.ToList(),
            TotalCount = invoices.Count(),
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(invoices.Count() / (double)pageSize)
        };

        return Success(result);
    }

    /// <summary>
    /// Updates an existing invoice
    /// </summary>
    /// <remarks>
    /// Updates an existing invoice with new data. Only draft invoices can be fully updated.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/v1/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///         "businessId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    ///         "currency": "USD",
    ///         "issueDate": "2024-01-15T00:00:00Z",
    ///         "dueDate": "2024-02-15T00:00:00Z",
    ///         "notes": "Updated payment terms",
    ///         "terms": "Updated terms and conditions",
    ///         "items": [...]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the invoice to update</param>
    /// <param name="request">The updated invoice data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated invoice</returns>
    /// <response code="200">Invoice updated successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied or invoice cannot be modified in current status.</response>
    /// <response code="404">Invoice not found.</response>
    /// <response code="422">Business logic validation failed.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateInvoiceAsync(
        [FromRoute] Guid id,
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        if (!ModelState.IsValid)
        {
            return Invalid("Invalid invoice data provided.");
        }

        _logger.LogInformation("Updating invoice: {InvoiceId}", id);

        var result = await _invoiceService.UpdateInvoiceAsync(id, request, cancellationToken);

        return Success(result);
    }

    /// <summary>
    /// Marks an invoice as sent to the client
    /// </summary>
    /// <remarks>
    /// Changes the invoice status to 'sent' and records the sent timestamp.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6/send
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the invoice to mark as sent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Invoice marked as sent successfully.</response>
    /// <response code="400">Invalid invoice ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the invoice.</response>
    /// <response code="404">Invoice not found.</response>
    /// <response code="422">Invoice cannot be marked as sent in current status.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("{id:guid}/send")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkInvoiceAsSentAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        _logger.LogInformation("Marking invoice as sent: {InvoiceId}", id);

        await _invoiceService.MarkInvoiceAsSentAsync(id, cancellationToken);

        return Success(new { Message = "Invoice marked as sent successfully." });
    }

    /// <summary>
    /// Emails an invoice to the client
    /// </summary>
    /// <remarks>
    /// Sends the invoice as a PDF attachment to the client's email address and marks it as sent.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6/email
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the invoice to email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email sending confirmation</returns>
    /// <response code="200">Invoice emailed successfully.</response>
    /// <response code="400">Invalid invoice ID.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the invoice.</response>
    /// <response code="404">Invoice not found.</response>
    /// <response code="422">Invoice cannot be emailed or client email missing.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("{id:guid}/email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EmailInvoiceAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        _logger.LogInformation("Emailing invoice: {InvoiceId}", id);

        await _invoiceService.EmailInvoiceAsync(id, cancellationToken);

        return Success(new { Message = "Invoice emailed successfully." });
    }
}

/// <summary>
/// Represents a paginated result set
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public required IList<T> Items { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public required int TotalPages { get; init; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}