using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.Core.Interfaces.Services;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for localization and currency operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LocalizationController"/> class.
/// </remarks>
/// <param name="localizationService">The localization service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class LocalizationController(ILocalizationService localizationService, ILogger<LocalizationController> logger) : ApiControllerBase
{

    /// <summary>
    /// Gets all supported currencies
    /// </summary>
    /// <remarks>
    /// Retrieves a list of all supported currencies with their symbols and formatting information.
    /// 
    /// Sample response:
    /// 
    ///     [
    ///         {
    ///             "code": "USD",
    ///             "name": "US Dollar",
    ///             "symbol": "$",
    ///             "decimalPlaces": 2
    ///         },
    ///         {
    ///             "code": "EUR",
    ///             "name": "Euro", 
    ///             "symbol": "€",
    ///             "decimalPlaces": 2
    ///         }
    ///     ]
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported currencies</returns>
    /// <response code="200">Currencies retrieved successfully.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    [HttpGet("currencies")]
    [ProducesResponseType(typeof(IEnumerable<CurrencyInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting supported currencies");

        var currencies = await localizationService.GetSupportedCurrenciesAsync();
        return Success(currencies);
    }

    /// <summary>
    /// Gets all supported locales
    /// </summary>
    /// <remarks>
    /// Retrieves a list of all supported locales for internationalization.
    /// 
    /// Sample response:
    /// 
    ///     [
    ///         {
    ///             "code": "en-US",
    ///             "name": "English (United States)",
    ///             "displayName": "English (US)"
    ///         },
    ///         {
    ///             "code": "de-DE",
    ///             "name": "German (Germany)",
    ///             "displayName": "Deutsch"
    ///         }
    ///     ]
    /// 
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported locales</returns>
    /// <response code="200">Locales retrieved successfully.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    [HttpGet("locales")]
    [ProducesResponseType(typeof(IEnumerable<LocaleInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSupportedLocalesAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting supported locales");

        var locales = await localizationService.GetSupportedLocalesAsync();
        return Success(locales);
    }

    /// <summary>
    /// Formats currency amount
    /// </summary>
    /// <remarks>
    /// Formats a currency amount based on the specified currency and locale.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/localization/format-currency
    ///     {
    ///         "amount": 1234.56,
    ///         "currency": "USD",
    ///         "locale": "en-US"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "formattedAmount": "$1,234.56"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">Currency formatting request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted currency string</returns>
    /// <response code="200">Currency formatted successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    [HttpPost("format-currency")]
    [ProducesResponseType(typeof(CurrencyFormatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult FormatCurrency([FromBody] CurrencyFormatRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Formatting currency: {Amount} {Currency} with locale {Locale}", 
            request.Amount, request.Currency, request.Locale);

        var formattedAmount = localizationService.FormatCurrency(request.Amount, request.Currency, request.Locale);

        return Success(new CurrencyFormatResponse
        {
            FormattedAmount = formattedAmount
        });
    }
}

/// <summary>
/// Request model for currency formatting.
/// </summary>
public class CurrencyFormatRequest
{
    /// <summary>
    /// Amount to format.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Locale code (e.g., en-US, de-DE). Optional.
    /// </summary>
    public string? Locale { get; set; }
}

/// <summary>
/// Response model for currency formatting.
/// </summary>
public class CurrencyFormatResponse
{
    /// <summary>
    /// Formatted currency amount.
    /// </summary>
    public string FormattedAmount { get; set; } = string.Empty;
}