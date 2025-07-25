namespace SharpInvoice.Core.Interfaces.Services;

/// <summary>
/// Interface for localization and currency formatting services.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Formats currency based on the provided locale.
    /// </summary>
    /// <param name="amount">The amount to format.</param>
    /// <param name="currency">The currency code (USD, EUR, etc.).</param>
    /// <param name="locale">The locale code (en-US, de-DE, etc.).</param>
    /// <returns>Formatted currency string.</returns>
    string FormatCurrency(decimal amount, string currency, string? locale = null);

    /// <summary>
    /// Formats date based on the provided locale.
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <param name="locale">The locale code.</param>
    /// <returns>Formatted date string.</returns>
    string FormatDate(DateTime date, string? locale = null);

    /// <summary>
    /// Gets supported currencies.
    /// </summary>
    /// <returns>List of supported currencies with their details.</returns>
    Task<IEnumerable<CurrencyInfo>> GetSupportedCurrenciesAsync();

    /// <summary>
    /// Gets supported locales.
    /// </summary>
    /// <returns>List of supported locales.</returns>
    Task<IEnumerable<LocaleInfo>> GetSupportedLocalesAsync();

    /// <summary>
    /// Validates if currency is supported.
    /// </summary>
    /// <param name="currency">Currency code to validate.</param>
    /// <returns>True if supported, false otherwise.</returns>
    bool IsCurrencySupported(string currency);
}

/// <summary>
/// Represents currency information.
/// </summary>
public class CurrencyInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; }
}

/// <summary>
/// Represents locale information.
/// </summary>
public class LocaleInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}