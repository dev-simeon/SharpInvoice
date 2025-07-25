using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

public class CreatePaymentIntentRequest
{
    [Required]
    public Guid InvoiceId { get; set; }
} 