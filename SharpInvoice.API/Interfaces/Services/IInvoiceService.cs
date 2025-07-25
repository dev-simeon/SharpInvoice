using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Entities;

namespace SharpInvoice.API.Interfaces.Services;

/// <summary>
/// Interface for invoice management services.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="request">The invoice creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created invoice DTO.</returns>
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invoice by its identifier.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The invoice DTO if found; otherwise, null.</returns>
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an invoice entity by its identifier with related data.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The invoice entity if found; otherwise, null.</returns>
    Task<Invoice?> GetInvoiceEntityByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all invoices for a specific business.
    /// </summary>
    /// <param name="businessId">The business identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of invoice DTOs.</returns>
    Task<IEnumerable<InvoiceDto>> GetInvoicesByBusinessAsync(string businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="request">The updated invoice data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated invoice DTO.</returns>
    Task<InvoiceDto> UpdateInvoiceAsync(Guid invoiceId, CreateInvoiceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an invoice (not implemented - invoices should be voided instead).
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an invoice as sent.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkInvoiceAsSentAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Emails an invoice to the client.
    /// </summary>
    /// <param name="invoiceId">The invoice identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EmailInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
} 