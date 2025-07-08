namespace SharpInvoice.Shared.Kernel.Exceptions;

public class InvalidTokenException(string message) : ApplicationException(message)
{
}