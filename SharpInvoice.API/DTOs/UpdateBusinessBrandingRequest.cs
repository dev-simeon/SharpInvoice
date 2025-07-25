using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Request to update business branding and theme settings
/// </summary>
public class UpdateBusinessBrandingRequest
{
    /// <summary>
    /// The business logo URL
    /// </summary>
    [Url(ErrorMessage = "Logo URL must be a valid URL")]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Theme settings as a JSON object containing colors, fonts, and other visual customizations
    /// </summary>
    [Required]
    public ThemeSettings ThemeSettings { get; set; } = new();
}

/// <summary>
/// Theme settings for business branding
/// </summary>
public class ThemeSettings
{
    /// <summary>
    /// Primary brand color (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary color must be a valid hex color (e.g., #007bff)")]
    public string? PrimaryColor { get; set; } = "#007bff";

    /// <summary>
    /// Secondary brand color (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Secondary color must be a valid hex color (e.g., #6c757d)")]
    public string? SecondaryColor { get; set; } = "#6c757d";

    /// <summary>
    /// Accent color for highlights and CTAs (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Accent color must be a valid hex color (e.g., #28a745)")]
    public string? AccentColor { get; set; } = "#28a745";

    /// <summary>
    /// Font family for text elements
    /// </summary>
    [StringLength(100, ErrorMessage = "Font family cannot exceed 100 characters")]
    public string? FontFamily { get; set; } = "Roboto, sans-serif";

    /// <summary>
    /// Background color for headers (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Header background color must be a valid hex color (e.g., #f8f9fa)")]
    public string? HeaderBackgroundColor { get; set; } = "#f8f9fa";

    /// <summary>
    /// Main text color (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Text color must be a valid hex color (e.g., #212529)")]
    public string? TextColor { get; set; } = "#212529";

    /// <summary>
    /// Background color for the main content area (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Background color must be a valid hex color (e.g., #ffffff)")]
    public string? BackgroundColor { get; set; } = "#ffffff";

    /// <summary>
    /// Border color for elements (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Border color must be a valid hex color (e.g., #dee2e6)")]
    public string? BorderColor { get; set; } = "#dee2e6";

    /// <summary>
    /// Converts the theme settings to JSON string for storage
    /// </summary>
    /// <returns>JSON representation of the theme settings</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    /// <summary>
    /// Creates theme settings from JSON string
    /// </summary>
    /// <param name="json">JSON string containing theme settings</param>
    /// <returns>ThemeSettings object</returns>
    public static ThemeSettings FromJson(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "{}")
        {
            return new ThemeSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<ThemeSettings>(json, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            }) ?? new ThemeSettings();
        }
        catch
        {
            return new ThemeSettings();
        }
    }
}