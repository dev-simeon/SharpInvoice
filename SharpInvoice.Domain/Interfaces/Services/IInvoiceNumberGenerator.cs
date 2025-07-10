namespace SharpInvoice.Core.Interfaces.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateInvoiceNumberAsync(string businessId);
} 