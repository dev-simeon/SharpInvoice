using AutoMapper;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;
using SharpInvoice.Core.Interfaces;
using SharpInvoice.API.Middleware.Exceptions;
using SharpInvoice.Core.Entities;
using SharpInvoice.API.Interfaces.Services;

namespace SharpInvoice.API.Services;

/// <summary>
/// Provides invoice management services with improved validation, logging, and performance.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoiceService"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="invoiceNumberGenerator">The invoice number generator.</param>
/// <param name="pdfService">The PDF service.</param>
/// <param name="emailService">The email service.</param>
/// <param name="mapper">The AutoMapper instance.</param>
/// <param name="logger">The logger.</param>
public sealed class InvoiceService(
    IUnitOfWork unitOfWork,
    IInvoiceNumberGenerator invoiceNumberGenerator,
    IPdfService pdfService,
    IEmailService emailService,
    IMapper mapper,
    ILogger<InvoiceService> logger) : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator = invoiceNumberGenerator ?? throw new ArgumentNullException(nameof(invoiceNumberGenerator));
    private readonly IPdfService _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<InvoiceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="request">The invoice creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created invoice DTO.</returns>
    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Creating invoice for business: {BusinessId}, client: {ClientId}", 
            request.BusinessId, request.ClientId);

        try
        {
            // Validate business and client exist
            await ValidateBusinessAndClientExistAsync(request.BusinessId, request.ClientId, cancellationToken);

            // Use transaction for consistency
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var invoiceNumber = await _invoiceNumberGenerator.GenerateInvoiceNumberAsync(request.BusinessId);

                var invoice = new Invoice(
                    request.BusinessId,
                    request.ClientId,
                    invoiceNumber,
                    request.Currency
                );

                invoice.UpdateDetails(request.IssueDate, request.DueDate, request.Notes, request.Terms);

                foreach (var item in request.Items)
                {
                    invoice.AddItem(item.Description, item.Quantity, item.UnitPrice, item.Unit);
                }

                await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice created successfully: {InvoiceId}, Number: {InvoiceNumber}", 
                    invoice.Id, invoice.InvoiceNumber);

                return _mapper.Map<InvoiceDto>(invoice);
            });
        }
        catch (Exception ex) when (ex is not ValidationException and not NotFoundException)
        {
            _logger.LogError(ex, "Error creating invoice for business: {BusinessId}", request.BusinessId);
            throw;
        }
    }

    /// <summary>
    /// Deletes an invoice (not implemented - invoices should be voided instead).
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Attempted to delete invoice: {InvoiceId}. Invoices should be voided instead.", invoiceId);
        throw new BusinessLogicException("Invoices cannot be deleted. They should be voided instead.");
    }

    /// <summary>
    /// Gets an invoice by its identifier.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The invoice DTO if found; otherwise, null.</returns>
    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        }

        _logger.LogDebug("Getting invoice by ID: {InvoiceId}", invoiceId);

        try
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId, cancellationToken);
            return invoice is null ? null : _mapper.Map<InvoiceDto>(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice by ID: {InvoiceId}", invoiceId);
            throw;
        }
    }

    public async Task<Invoice?> GetInvoiceEntityByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
    }

    public async Task<IEnumerable<InvoiceDto>> GetInvoicesByBusinessAsync(string businessId, CancellationToken cancellationToken = default)
    {
        var invoices = await _unitOfWork.Invoices.FindAsync(i => i.BusinessId == businessId);
        return invoices.Select(ToInvoiceDto);
    }

    public async Task MarkInvoiceAsSentAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice != null)
        {
            invoice.MarkAsSent();
            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<InvoiceDto> UpdateInvoiceAsync(Guid invoiceId, CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
        if (invoice == null)
        {
            throw new Exception("Invoice not found.");
        }

        invoice.UpdateDetails(request.IssueDate, request.DueDate, request.Notes, request.Terms);

        // This is a simplified update. A more robust implementation would handle item updates, additions, and removals.
        invoice.Items.Clear();
        foreach (var item in request.Items)
        {
            invoice.AddItem(item.Description, item.Quantity, item.UnitPrice, item.Unit);
        }

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        return ToInvoiceDto(invoice);
    }

    public async Task EmailInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await GetInvoiceEntityByIdAsync(invoiceId, cancellationToken);
        if (invoice == null || string.IsNullOrEmpty(invoice.Client.Email))
        {
            // Or throw an exception
            return;
        }

        var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
        var subject = $"Invoice from {invoice.Business.Name}";
        var body = $"Please find attached your invoice {invoice.InvoiceNumber}.";
        
        await _emailService.SendEmailWithAttachmentAsync(invoice.Client.Email, subject, body, pdfBytes, $"Invoice-{invoice.InvoiceNumber}.pdf");
        
        invoice.MarkAsSent();
        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();
    }



    private InvoiceDto ToInvoiceDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            BusinessId = invoice.BusinessId,
            ClientId = invoice.ClientId,
            InvoiceNumber = invoice.InvoiceNumber,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            SubTotal = invoice.SubTotal,
            Tax = invoice.Tax,
            Total = invoice.Total,
            AmountPaid = invoice.AmountPaid,
            Currency = invoice.Currency,
            Status = invoice.Status,
            Notes = invoice.Notes,
            Terms = invoice.Terms,
            Items = [.. invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.Total
            })]
        };
    }

    /// <summary>
    /// Validates that the business and client exist.
    /// </summary>
    /// <param name="businessId">The business identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ValidateBusinessAndClientExistAsync(string businessId, string clientId, CancellationToken cancellationToken = default)
    {
        var business = await _unitOfWork.Businesses.GetByIdAsync(businessId);
        if (business is null)
        {
            throw new NotFoundException("Business", businessId);
        }

        var client = await _unitOfWork.Clients.GetByIdAsync(clientId) ?? throw new NotFoundException("Client", clientId);

        // Verify the client belongs to the business

        if (client.BusinessId != businessId.ToString())
        {
            throw new BusinessLogicException("Client does not belong to the specified business.");
        }
    }
} 