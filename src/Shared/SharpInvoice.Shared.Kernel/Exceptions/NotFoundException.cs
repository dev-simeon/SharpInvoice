namespace SharpInvoice.Shared.Kernel.Exceptions;

public class NotFoundException(string message) : ApplicationException(message)
{
} 