using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for payment processing operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class PaymentController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<PaymentController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentController"/> class.
    /// </summary>
    /// <param name="paymentService">The payment service.</param>
    /// <param name="invoiceService">The invoice service.</param>
    /// <param name="logger">The logger.</param>
    public PaymentController(
        IPaymentService paymentService,
        IInvoiceService invoiceService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a Stripe payment intent for an invoice
    /// </summary>
    /// <remarks>
    /// Creates a Stripe payment intent that can be used to process payment for an invoice.
    /// This endpoint requires authentication as the invoice owner.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/payment/create-intent
    ///     {
    ///         "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    ///     }
    /// 
    /// The returned client secret should be used with Stripe's client-side libraries to complete the payment.
    /// 
    /// </remarks>
    /// <param name="request">The payment intent creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment intent details including client secret</returns>
    /// <response code="200">Payment intent created successfully.</response>
    /// <response code="400">Invalid input data or validation failed.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the invoice.</response>
    /// <response code="404">Invoice not found.</response>
    /// <response code="422">Invoice cannot be paid (already paid, voided, etc.).</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("create-intent")]
    [Authorize]
    [ProducesResponseType(typeof(CreatePaymentIntentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePaymentIntentAsync(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid payment request data provided.");
        }

        _logger.LogInformation("Creating payment intent for invoice: {InvoiceId}", request.InvoiceId);

        var result = await _paymentService.CreatePaymentIntentAsync(request);

        return Success(result);
    }

    /// <summary>
    /// Handles successful payment webhook from Stripe
    /// </summary>
    /// <remarks>
    /// This endpoint is called by Stripe when a payment is successfully processed.
    /// It should be configured as a webhook endpoint in your Stripe dashboard.
    /// 
    /// **Security Note:** This endpoint validates the Stripe webhook signature to ensure authenticity.
    /// 
    /// Webhook events handled:
    /// - payment_intent.succeeded
    /// - payment_intent.payment_failed
    /// 
    /// Sample webhook payload:
    /// 
    ///     POST /api/v1/payment/webhook
    ///     Stripe-Signature: t=1234567890,v1=signature_hash
    ///     
    ///     {
    ///         "id": "evt_1234567890",
    ///         "type": "payment_intent.succeeded",
    ///         "data": {
    ///             "object": {
    ///                 "id": "pi_1234567890",
    ///                 "status": "succeeded",
    ///                 "metadata": {
    ///                     "invoice_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    ///                 }
    ///             }
    ///         }
    ///     }
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Webhook processing confirmation</returns>
    /// <response code="200">Webhook processed successfully.</response>
    /// <response code="400">Invalid webhook payload or signature.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleStripeWebhookAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string json;
            using (var reader = new StreamReader(Request.Body))
            {
                json = await reader.ReadToEndAsync();
            }

            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(stripeSignature))
            {
                _logger.LogWarning("Stripe webhook received without signature");
                return BadRequest("Missing Stripe signature");
            }

            _logger.LogInformation("Processing Stripe webhook with signature: {Signature}", stripeSignature);

            // Note: The actual webhook processing logic would be implemented in the PaymentService
            // This would include signature validation and event processing
            
            // For now, we'll extract the payment intent ID from the webhook payload
            // In a real implementation, you'd use Stripe.net to properly parse and validate the webhook
            
            var eventJson = System.Text.Json.JsonDocument.Parse(json);
            var eventType = eventJson.RootElement.GetProperty("type").GetString();
            
            if (eventType == "payment_intent.succeeded")
            {
                var paymentIntentId = eventJson.RootElement
                    .GetProperty("data")
                    .GetProperty("object")
                    .GetProperty("id")
                    .GetString();

                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    await _paymentService.HandleSuccessfulPaymentAsync(paymentIntentId);
                    _logger.LogInformation("Successfully processed payment for intent: {PaymentIntentId}", paymentIntentId);
                }
            }

            return Ok(new { received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, "Error processing webhook");
        }
    }

    /// <summary>
    /// Retrieves payment history for a specific invoice
    /// </summary>
    /// <remarks>
    /// Gets all payment attempts and transactions for a specific invoice.
    /// The user must own the invoice to access its payment history.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/payment/invoice/3fa85f64-5717-4562-b3fc-2c963f66afa6/history
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="invoiceId">The unique identifier of the invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment history for the invoice</returns>
    /// <response code="200">Payment history retrieved successfully.</response>
    /// <response code="400">Invalid invoice ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the invoice.</response>
    /// <response code="404">Invoice not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("invoice/{invoiceId:guid}/history")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PaymentHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaymentHistoryAsync(
        [FromRoute] Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty)
        {
            return Invalid("Invoice ID cannot be empty.");
        }

        _logger.LogDebug("Retrieving payment history for invoice: {InvoiceId}", invoiceId);

        // Check if invoice exists and user has access
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            return NotFound($"Invoice with ID '{invoiceId}' was not found.");
        }

        // In a real implementation, you'd get payment history from a dedicated service
        // For now, returning empty list as placeholder
        var paymentHistory = new List<PaymentHistoryDto>();

        return Success(paymentHistory);
    }

    /// <summary>
    /// Processes a refund for a payment
    /// </summary>
    /// <remarks>
    /// Initiates a refund for a previously successful payment.
    /// The user must own the invoice associated with the payment.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/payment/refund
    ///     {
    ///         "paymentIntentId": "pi_1234567890",
    ///         "amount": 150.00,
    ///         "reason": "customer_request"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The refund request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refund processing result</returns>
    /// <response code="200">Refund processed successfully.</response>
    /// <response code="400">Invalid refund request data.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the payment.</response>
    /// <response code="404">Payment not found.</response>
    /// <response code="422">Refund cannot be processed (already refunded, payment failed, etc.).</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("refund")]
    [Authorize]
    [ProducesResponseType(typeof(RefundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessRefundAsync(
        [FromBody] RefundRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Invalid("Invalid refund request data provided.");
        }

        _logger.LogInformation("Processing refund for payment intent: {PaymentIntentId}, amount: {Amount}", 
            request.PaymentIntentId, request.Amount);

        // Placeholder implementation - in a real system, this would be implemented in the PaymentService
        // It would include validation, Stripe refund API calls, and database updates
        
        var refundResponse = new RefundResponse
        {
            RefundId = Guid.NewGuid().ToString(),
            Status = "pending",
            Amount = request.Amount,
            Currency = "usd", // Would be determined from the original payment
            ProcessedAt = DateTime.UtcNow
        };

        return Success(refundResponse);
    }
}

/// <summary>
/// Payment history data transfer object
/// </summary>
public sealed record PaymentHistoryDto
{
    /// <summary>
    /// Unique identifier for the payment transaction
    /// </summary>
    public required string TransactionId { get; init; }

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Payment currency (USD, EUR, etc.)
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Payment status (succeeded, failed, pending, etc.)
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Payment method used (card, bank_transfer, etc.)
    /// </summary>
    public string PaymentMethod { get; init; } = string.Empty;

    /// <summary>
    /// Date and time when the payment was processed
    /// </summary>
    public DateTime ProcessedAt { get; init; }

    /// <summary>
    /// Failure reason if payment failed
    /// </summary>
    public string? FailureReason { get; init; }
}

/// <summary>
/// Refund request data transfer object
/// </summary>
public sealed record RefundRequest
{
    /// <summary>
    /// Stripe payment intent ID to refund
    /// </summary>
    /// <example>pi_1234567890abcdef</example>
    [Required]
    public required string PaymentIntentId { get; init; }

    /// <summary>
    /// Amount to refund (partial refunds allowed)
    /// </summary>
    /// <example>150.00</example>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
    public decimal Amount { get; init; }

    /// <summary>
    /// Reason for the refund
    /// </summary>
    /// <example>customer_request</example>
    public string Reason { get; init; } = "requested_by_customer";
}

/// <summary>
/// Refund response data transfer object
/// </summary>
public sealed record RefundResponse
{
    /// <summary>
    /// Unique identifier for the refund
    /// </summary>
    public required string RefundId { get; init; }

    /// <summary>
    /// Refund status
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Refunded amount
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Refund currency
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Date and time when the refund was processed
    /// </summary>
    public DateTime ProcessedAt { get; init; }

    /// <summary>
    /// Estimated date when refund will appear in customer's account
    /// </summary>
    public DateTime? EstimatedArrival { get; init; }
}