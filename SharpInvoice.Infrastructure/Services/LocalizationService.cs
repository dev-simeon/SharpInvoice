using System.Globalization;
using SharpInvoice.Core.Interfaces.Services;

namespace SharpInvoice.Infrastructure.Services;

/// <summary>
/// Service for localization and currency formatting.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, CurrencyInfo> _currencies;
    private readonly Dictionary<string, LocaleInfo> _locales;

    public LocalizationService()
    {
        _currencies = InitializeCurrencies();
        _locales = InitializeLocales();
    }

    public string FormatCurrency(decimal amount, string currency, string? locale = null)
    {
        var cultureInfo = string.IsNullOrEmpty(locale) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(locale);
        
        if (_currencies.TryGetValue(currency.ToUpper(), out var currencyInfo))
        {
            return amount.ToString("C", cultureInfo).Replace(cultureInfo.NumberFormat.CurrencySymbol, currencyInfo.Symbol);
        }

        return amount.ToString("C", cultureInfo);
    }

    public string FormatDate(DateTime date, string? locale = null)
    {
        var cultureInfo = string.IsNullOrEmpty(locale) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(locale);
        return date.ToString("d", cultureInfo);
    }

    public Task<IEnumerable<CurrencyInfo>> GetSupportedCurrenciesAsync()
    {
        return Task.FromResult(_currencies.Values.AsEnumerable());
    }

    public Task<IEnumerable<LocaleInfo>> GetSupportedLocalesAsync()
    {
        return Task.FromResult(_locales.Values.AsEnumerable());
    }

    public bool IsCurrencySupported(string currency)
    {
        return _currencies.ContainsKey(currency.ToUpper());
    }

    private static Dictionary<string, CurrencyInfo> InitializeCurrencies()
    {
        return new Dictionary<string, CurrencyInfo>
        {
            { "USD", new CurrencyInfo { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2 } },
            { "EUR", new CurrencyInfo { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2 } },
            { "GBP", new CurrencyInfo { Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2 } },
            { "NGN", new CurrencyInfo { Code = "NGN", Name = "Nigerian Naira", Symbol = "₦", DecimalPlaces = 2 } },
            { "CAD", new CurrencyInfo { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2 } },
            { "AUD", new CurrencyInfo { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2 } },
            { "JPY", new CurrencyInfo { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0 } },
            { "CHF", new CurrencyInfo { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", DecimalPlaces = 2 } },
            { "CNY", new CurrencyInfo { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2 } },
            { "INR", new CurrencyInfo { Code = "INR", Name = "Indian Rupee", Symbol = "₹", DecimalPlaces = 2 } }
        };
    }

    private static Dictionary<string, LocaleInfo> InitializeLocales()
    {
        return new Dictionary<string, LocaleInfo>
        {
            { "en-US", new LocaleInfo { Code = "en-US", Name = "English (United States)", DisplayName = "English (US)" } },
            { "en-GB", new LocaleInfo { Code = "en-GB", Name = "English (United Kingdom)", DisplayName = "English (UK)" } },
            { "de-DE", new LocaleInfo { Code = "de-DE", Name = "German (Germany)", DisplayName = "Deutsch" } },
            { "fr-FR", new LocaleInfo { Code = "fr-FR", Name = "French (France)", DisplayName = "Français" } },
            { "es-ES", new LocaleInfo { Code = "es-ES", Name = "Spanish (Spain)", DisplayName = "Español" } },
            { "it-IT", new LocaleInfo { Code = "it-IT", Name = "Italian (Italy)", DisplayName = "Italiano" } },
            { "pt-BR", new LocaleInfo { Code = "pt-BR", Name = "Portuguese (Brazil)", DisplayName = "Português (Brasil)" } },
            { "ja-JP", new LocaleInfo { Code = "ja-JP", Name = "Japanese (Japan)", DisplayName = "日本語" } },
            { "zh-CN", new LocaleInfo { Code = "zh-CN", Name = "Chinese (Simplified, China)", DisplayName = "中文 (简体)" } },
            { "hi-IN", new LocaleInfo { Code = "hi-IN", Name = "Hindi (India)", DisplayName = "हिन्दी" } }
        };
    }
}