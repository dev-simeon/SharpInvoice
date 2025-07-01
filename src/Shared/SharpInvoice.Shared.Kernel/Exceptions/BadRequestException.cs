namespace SharpInvoice.Shared.Kernel.Exceptions;
 
public class BadRequestException(string message) : ApplicationException(message)
{
} 