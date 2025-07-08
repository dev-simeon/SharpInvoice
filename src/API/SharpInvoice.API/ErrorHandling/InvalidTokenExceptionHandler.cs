namespace SharpInvoice.API.ErrorHandling;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Shared.Kernel.Exceptions;

internal sealed class InvalidTokenExceptionHandler(ILogger<InvalidTokenExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InvalidTokenException invalidTokenException)
        {
            return false;
        }

        logger.LogWarning(
            invalidTokenException,
            "Invalid token provided: {Message}",
            invalidTokenException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = invalidTokenException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}