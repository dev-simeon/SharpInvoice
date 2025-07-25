using System.IO;
using SharpInvoice.Core.Entities;

namespace SharpInvoice.Core.Interfaces.Services;

public interface IPdfService
{
    byte[] GenerateInvoicePdf(Invoice invoice);
} 