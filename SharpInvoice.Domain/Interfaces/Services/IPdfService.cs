using System.IO;
using SharpInvoice.Core.Domain.Entities;

namespace SharpInvoice.Core.Interfaces.Services;

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice);
    Task<Stream> GenerateInvoicePdfStreamAsync(Invoice invoice);
} 