namespace SharpInvoice.API.ErrorHandling;

using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var problemDetails = new ValidationProblemDetails(
                validationException.Errors.GroupBy(e => e.PropertyName)
                                     .ToDictionary(
                                         g => g.Key,
                                         g => g.Select(e => e.ErrorMessage).ToArray()
                                     ))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Detail = "Please refer to the errors property for details."
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        return false;
    }
}