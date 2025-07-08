namespace SharpInvoice.Shared.Kernel.Exceptions;

using System;

public class ForbidException : Exception
{
    public ForbidException(string message) : base(message)
    {
    }
}