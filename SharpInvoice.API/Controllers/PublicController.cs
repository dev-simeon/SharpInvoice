using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Core.Entities;
using SharpInvoice.Core.Enums;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Core.Interfaces.Services;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for public access operations (no authentication required)
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PublicController"/> class.
/// </remarks>
/// <param name="invoiceService">The invoice service.</param>
/// <param name="pdfService">The PDF service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[AllowAnonymous]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class PublicController(
    IInvoiceService invoiceService,
    IPdfService pdfService,
    ILogger<PublicController> logger) : ApiControllerBase
{

    /// <summary>
    /// Views a public invoice by its secure token
    /// </summary>
    /// <remarks>
    /// Allows public viewing of an invoice using a secure token. This endpoint does not require authentication
    /// and is designed to be shared with clients for invoice viewing and payment.
    /// 
    /// The secure token is typically generated when an invoice is created and can be included in email links.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/public/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6/view?token=secure_token_here
    /// 
    /// This endpoint returns the invoice details including business information, client details, 
    /// line items, and payment status.
    /// 
    /// </remarks>
    /// <param name="invoiceId">The unique identifier of the invoice</param>
    /// <param name="token">The secure access token for the invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public invoice view data</returns>
    /// <response code="200">Invoice retrieved successfully.</response>
    /// <response code="400">Invalid invoice ID or token format.</response>
    /// <response code="404">Invoice not found or token is invalid.</response>
    /// <response code="410">Invoice link has expired or been revoked.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("invoice/{invoiceId:guid}/view")]
    [ProducesResponseType(typeof(PublicInvoiceViewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ViewPublicInvoiceAsync(
        [FromRoute] Guid invoiceId,
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Invalid("Access token is required.");
        }

        logger.LogInformation("Public invoice view requested for: {InvoiceId}", invoiceId);

        try
        {
            // Get the invoice entity with related data
            var invoice = await invoiceService.GetInvoiceEntityByIdAsync(invoiceId, cancellationToken);
            
            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            // In a real implementation, you would validate the secure token here
            // For now, we'll create a placeholder validation
            if (!IsValidInvoiceToken(invoiceId, token))
            {
                return NotFound("Invalid or expired access token.");
            }

            // Create public view DTO with necessary information
            var publicView = new PublicInvoiceViewDto
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Status = invoice.Status.ToString(),
                Currency = invoice.Currency,
                SubTotal = invoice.SubTotal,
                Tax = invoice.Tax,
                Total = invoice.Total,
                AmountPaid = invoice.AmountPaid,
                AmountDue = invoice.Total - invoice.AmountPaid,
                Notes = invoice.Notes,
                Terms = invoice.Terms,
                Business = new PublicBusinessInfoDto
                {
                    Name = invoice.Business.Name,
                    Email = invoice.Business.Email,
                    Phone = invoice.Business.PhoneNumber,
                    Address = invoice.Business.Address,
                    Website = invoice.Business.Website,
                    LogoUrl = invoice.Business.LogoUrl
                },
                Client = new PublicClientInfoDto
                {
                    Name = invoice.Client.Name,
                    Email = invoice.Client.Email,
                    Phone = invoice.Client.Phone,
                    Address = invoice.Client.Address
                },
                Items = invoice.Items.Select(item => new PublicInvoiceItemDto
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Unit = item.Unit,
                    Total = item.Total
                }).ToList(),
                PaymentInstructions = GeneratePaymentInstructions(invoice),
                CanPay = invoice.Status != InvoiceStatus.Paid && 
                        invoice.Status != InvoiceStatus.Void
            };

            logger.LogInformation("Public invoice view successful for: {InvoiceId}", invoiceId);
            return Success(publicView);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving public invoice view for: {InvoiceId}", invoiceId);
            throw;
        }
    }

    /// <summary>
    /// Downloads an invoice as PDF using secure token
    /// </summary>
    /// <remarks>
    /// Generates and downloads an invoice as a PDF file using a secure access token.
    /// This endpoint does not require authentication and can be shared with clients.
    /// 
    /// The PDF includes all invoice details, business branding, and payment instructions.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/public/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6/pdf?token=secure_token_here
    /// 
    /// The response will be a PDF file with appropriate headers for download.
    /// 
    /// </remarks>
    /// <param name="invoiceId">The unique identifier of the invoice</param>
    /// <param name="token">The secure access token for the invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file download</returns>
    /// <response code="200">PDF generated and returned successfully.</response>
    /// <response code="400">Invalid invoice ID or token format.</response>
    /// <response code="404">Invoice not found or token is invalid.</response>
    /// <response code="410">Invoice link has expired or been revoked.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("invoice/{invoiceId:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadInvoicePdfAsync(
        [FromRoute] Guid invoiceId,
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Invalid("Access token is required.");
        }

        logger.LogInformation("Public PDF download requested for invoice: {InvoiceId}", invoiceId);

        try
        {
            // Get the invoice entity with related data
            var invoice = await invoiceService.GetInvoiceEntityByIdAsync(invoiceId, cancellationToken);
            
            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            // Validate the secure token
            if (!IsValidInvoiceToken(invoiceId, token))
            {
                return NotFound("Invalid or expired access token.");
            }

            // Generate PDF
            var pdfBytes = pdfService.GenerateInvoicePdf(invoice);
            var fileName = $"Invoice-{invoice.InvoiceNumber}.pdf";

            logger.LogInformation("PDF generated successfully for invoice: {InvoiceId}", invoiceId);

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating PDF for invoice: {InvoiceId}", invoiceId);
            throw;
        }
    }

    /// <summary>
    /// Creates a payment page for an invoice
    /// </summary>
    /// <remarks>
    /// Returns payment information and options for an invoice. This can be used to integrate
    /// with payment processors like Stripe or to display payment instructions.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/public/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6/payment?token=secure_token_here
    /// 
    /// </remarks>
    /// <param name="invoiceId">The unique identifier of the invoice</param>
    /// <param name="token">The secure access token for the invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment page information</returns>
    /// <response code="200">Payment information retrieved successfully.</response>
    /// <response code="400">Invalid invoice ID or token format.</response>
    /// <response code="404">Invoice not found or token is invalid.</response>
    /// <response code="410">Invoice cannot be paid (already paid, expired, etc.).</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("invoice/{invoiceId:guid}/payment")]
    [ProducesResponseType(typeof(PaymentPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaymentPageAsync(
        [FromRoute] Guid invoiceId,
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Invalid("Access token is required.");
        }

        logger.LogInformation("Payment page requested for invoice: {InvoiceId}", invoiceId);

        try
        {
            var invoice = await invoiceService.GetInvoiceEntityByIdAsync(invoiceId, cancellationToken);
            
            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            if (!IsValidInvoiceToken(invoiceId, token))
            {
                return NotFound("Invalid or expired access token.");
            }

            // Check if invoice can be paid
            if (invoice.Status == InvoiceStatus.Paid)
            {
                return StatusCode(410, "Invoice has already been paid.");
            }

            if (invoice.Status == InvoiceStatus.Void)
            {
                return StatusCode(410, "Invoice has been voided and cannot be paid.");
            }

            var paymentPage = new PaymentPageDto
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                BusinessName = invoice.Business.Name,
                AmountDue = invoice.Total - invoice.AmountPaid,
                Currency = invoice.Currency,
                DueDate = invoice.DueDate,
                IsOverdue = invoice.DueDate < DateTime.UtcNow,
                PaymentMethods = GetAvailablePaymentMethods(),
                PaymentInstructions = GeneratePaymentInstructions(invoice)
            };

            return Success(paymentPage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving payment page for invoice: {InvoiceId}", invoiceId);
            throw;
        }
    }

    /// <summary>
    /// Validates an invoice token (placeholder implementation)
    /// </summary>
    /// <param name="invoiceId">The invoice ID</param>
    /// <param name="token">The token to validate</param>
    /// <returns>True if token is valid, false otherwise</returns>
    private static bool IsValidInvoiceToken(Guid invoiceId, string token)
    {
        // Placeholder implementation
        // In a real system, this would validate against stored tokens, check expiry, etc.
        // For now, we'll just check that the token is not empty and has a reasonable length
        return !string.IsNullOrWhiteSpace(token) && token.Length >= 10;
    }

    /// <summary>
    /// Generates payment instructions for an invoice
    /// </summary>
    /// <param name="invoice">The invoice</param>
    /// <returns>Payment instructions text</returns>
    private static string GeneratePaymentInstructions(Invoice invoice)
    {
        return $"Please pay the amount of {invoice.Currency} {invoice.Total:F2} by {invoice.DueDate:yyyy-MM-dd}. " +
               "You can pay online using the payment options below or contact us for alternative payment methods.";
    }

    /// <summary>
    /// Gets available payment methods
    /// </summary>
    /// <returns>List of available payment methods</returns>
    private static List<string> GetAvailablePaymentMethods()
    {
        return new List<string> { "Credit Card", "Debit Card", "Bank Transfer" };
    }
}

/// <summary>
/// Public invoice view data transfer object
/// </summary>
public sealed record PublicInvoiceViewDto
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal Total { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal AmountDue { get; init; }
    public string? Notes { get; init; }
    public string? Terms { get; init; }
    public bool CanPay { get; init; }
    public string PaymentInstructions { get; init; } = string.Empty;
    public required PublicBusinessInfoDto Business { get; init; }
    public required PublicClientInfoDto Client { get; init; }
    public required List<PublicInvoiceItemDto> Items { get; init; }
}

/// <summary>
/// Public business information data transfer object
/// </summary>
public sealed record PublicBusinessInfoDto
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Website { get; init; }
    public string? LogoUrl { get; init; }
}

/// <summary>
/// Public client information data transfer object
/// </summary>
public sealed record PublicClientInfoDto
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

/// <summary>
/// Public invoice item data transfer object
/// </summary>
public sealed record PublicInvoiceItemDto
{
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Unit { get; init; }
    public decimal Total { get; init; }
}

/// <summary>
/// Payment page data transfer object
/// </summary>
public sealed record PaymentPageDto
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string BusinessName { get; init; } = string.Empty;
    public decimal AmountDue { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public bool IsOverdue { get; init; }
    public string PaymentInstructions { get; init; } = string.Empty;
    public List<string> PaymentMethods { get; init; } = new();
}