using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace SharpInvoice.API.ErrorHandling;

/// <summary>
/// Custom implementation of <see cref="ProblemDetailsFactory"/> that provides consistent
/// error response format including validation errors.
/// </summary>
public class ValidationProblemDetailsFactory(
    IOptions<ApiBehaviorOptions> options) : ProblemDetailsFactory
{
    private readonly ApiBehaviorOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance for general errors.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The error title.</param>
    /// <param name="type">The error type URI.</param>
    /// <param name="detail">The error detail message.</param>
    /// <param name="instance">The error instance URI.</param>
    /// <returns>A <see cref="ProblemDetails"/> instance.</returns>
    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        statusCode ??= 500;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title ?? GetDefaultTitle(statusCode.Value),
            Type = type ?? $"https://httpstatuses.io/{statusCode}",
            Detail = detail,
            Instance = instance
        };

        ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

        return problemDetails;
    }

    /// <inheritdoc />
    /// <summary>
    /// Creates a <see cref="ValidationProblemDetails"/> instance for validation errors.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="modelStateDictionary">The model state dictionary containing validation errors.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The error title.</param>
    /// <param name="type">The error type URI.</param>
    /// <param name="detail">The error detail message.</param>
    /// <param name="instance">The error instance URI.</param>
    /// <returns>A <see cref="ValidationProblemDetails"/> instance.</returns>
    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        ArgumentNullException.ThrowIfNull(modelStateDictionary);

        statusCode ??= 400;

        var errors = modelStateDictionary
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
            );

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = statusCode,
            Title = title ?? GetDefaultTitle(statusCode.Value),
            Type = type ?? $"https://httpstatuses.io/{statusCode}",
            Detail = detail,
            Instance = instance
        };

        if (errors.Count == 0)
        {
            // No error messages were provided, add a generic error
            problemDetails.Errors.Add("Error", new[] { "A validation error has occurred." });
        }

        ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

        return problemDetails;
    }

    private static string GetDefaultTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Validation Error",
            500 => "Server Error",
            _ => $"Status Code {statusCode}"
        };
    }

    private void ApplyProblemDetailsDefaults(HttpContext httpContext, ProblemDetails problemDetails, int statusCode)
    {
        problemDetails.Status ??= statusCode;

        if (_options.ClientErrorMapping.TryGetValue(statusCode, out var clientErrorData))
        {
            problemDetails.Title ??= clientErrorData.Title;
            problemDetails.Type ??= clientErrorData.Link;
        }

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        if (traceId != null)
        {
            problemDetails.Extensions["traceId"] = traceId;
        }

        // Add the error occurrence time
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    }
}