namespace SharpInvoice.Shared.Kernel.Exceptions;

/// <summary>
/// Represents errors that occur during external authentication processes (e.g., OAuth with Google/Facebook).
/// </summary>
public class ExternalAuthenticationException : ApplicationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalAuthenticationException"/> class.
    /// </summary>
    public ExternalAuthenticationException()
        : base("An error occurred during external authentication.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalAuthenticationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ExternalAuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalAuthenticationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ExternalAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}