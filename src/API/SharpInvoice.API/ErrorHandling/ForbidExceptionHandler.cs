namespace SharpInvoice.API.ErrorHandling;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Shared.Kernel.Exceptions;

/// <summary>
/// Handles <see cref="ForbidException"/> instances, returning a 403 Forbidden response.
/// </summary>
public class ForbidExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Tries to handle the exception asynchronously.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ForbidException forbidException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = forbidException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
} 