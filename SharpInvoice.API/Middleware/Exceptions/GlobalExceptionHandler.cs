using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Core.Shared;
using System.Net;
using System.Text.Json;

namespace SharpInvoice.API.Middleware.Exceptions;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception has occurred while executing the request {Path}", 
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = GetStatusCode(exception),
            Title = GetTitle(exception),
            Detail = GetDetail(exception),
            Instance = httpContext.Request.Path,
            Type = GetType(exception)
        };

        // Add additional details for developers in development environment
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = new
            {
                message = exception.Message,
                stackTrace = exception.StackTrace,
                source = exception.Source
            };

            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    message = exception.InnerException.Message,
                    stackTrace = exception.InnerException.StackTrace
                };
            }
        }

        problemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;
        
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails, options, cancellationToken);
        
        return true;
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            // Application exceptions
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            
            // Domain exceptions
            EntityNotFoundException => StatusCodes.Status404NotFound,
            AuthenticationException => StatusCodes.Status401Unauthorized,
            BusinessRuleViolationException => StatusCodes.Status422UnprocessableEntity,
            DomainValidationException => StatusCodes.Status400BadRequest,
            InactiveBusinessException => StatusCodes.Status400BadRequest,
            ClientWithInvoicesException => StatusCodes.Status400BadRequest,
            TokenExpiredException => StatusCodes.Status401Unauthorized,
            InvoiceOperationException => StatusCodes.Status422UnprocessableEntity,
            PaymentOperationException => StatusCodes.Status422UnprocessableEntity,
            
            _ => StatusCodes.Status500InternalServerError
        };
    }
    
    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            // Application exceptions
            ValidationException => "Validation Error",
            NotFoundException => "Resource Not Found",
            UnauthorizedException => "Authentication Required",
            ForbiddenException => "Access Denied",
            
            // Domain exceptions
            EntityNotFoundException => "Resource Not Found",
            AuthenticationException => "Authentication Error",
            BusinessRuleViolationException => "Business Rule Violation",
            DomainValidationException => "Validation Error",
            InactiveBusinessException => "Inactive Business",
            ClientWithInvoicesException => "Operation Not Allowed",
            TokenExpiredException => "Token Expired",
            InvoiceOperationException => "Invoice Operation Error",
            PaymentOperationException => "Payment Operation Error",
            
            _ => "Server Error"
        };
    }

    private static string GetType(Exception exception)
    {
        return exception switch
        {
            // Application exceptions
            ValidationException => "https://api.sharpinvoice.com/errors/validation",
            NotFoundException => "https://api.sharpinvoice.com/errors/not-found",
            UnauthorizedException => "https://api.sharpinvoice.com/errors/unauthorized",
            ForbiddenException => "https://api.sharpinvoice.com/errors/forbidden",
            
            // Domain exceptions
            EntityNotFoundException => "https://api.sharpinvoice.com/errors/not-found",
            AuthenticationException => "https://api.sharpinvoice.com/errors/authentication",
            BusinessRuleViolationException => "https://api.sharpinvoice.com/errors/business-rule",
            DomainValidationException => "https://api.sharpinvoice.com/errors/validation",
            InactiveBusinessException => "https://api.sharpinvoice.com/errors/inactive-business",
            ClientWithInvoicesException => "https://api.sharpinvoice.com/errors/client-with-invoices",
            TokenExpiredException => "https://api.sharpinvoice.com/errors/token-expired",
            InvoiceOperationException => "https://api.sharpinvoice.com/errors/invoice-operation",
            PaymentOperationException => "https://api.sharpinvoice.com/errors/payment-operation",
            
            _ => "https://api.sharpinvoice.com/errors/internal-server-error"
        };
    }

    private static string GetDetail(Exception exception)
    {
        return exception switch
        {
            ApplicationException appEx => appEx.Message,
            DomainException domainEx => domainEx.Message,
            _ => "An error occurred while processing your request. Please contact support if the issue persists."
        };
    }
} 