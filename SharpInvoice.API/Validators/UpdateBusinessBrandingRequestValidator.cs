using FluentValidation;
using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Validators;

/// <summary>
/// Validator for updating business branding requests.
/// </summary>
public class UpdateBusinessBrandingRequestValidator : AbstractValidator<UpdateBusinessBrandingRequest>
{
    public UpdateBusinessBrandingRequestValidator()
    {
        RuleFor(x => x.LogoUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("Logo URL must be a valid URL.");

        RuleFor(x => x.ThemeSettings)
            .NotNull()
            .WithMessage("Theme settings are required.");

        RuleFor(x => x.ThemeSettings.PrimaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.PrimaryColor))
            .WithMessage("Primary color must be a valid hex color (e.g., #007bff).");

        RuleFor(x => x.ThemeSettings.SecondaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.SecondaryColor))
            .WithMessage("Secondary color must be a valid hex color (e.g., #6c757d).");

        RuleFor(x => x.ThemeSettings.AccentColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.AccentColor))
            .WithMessage("Accent color must be a valid hex color (e.g., #28a745).");

        RuleFor(x => x.ThemeSettings.FontFamily)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.FontFamily))
            .WithMessage("Font family cannot exceed 100 characters.");

        RuleFor(x => x.ThemeSettings.HeaderBackgroundColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.HeaderBackgroundColor))
            .WithMessage("Header background color must be a valid hex color (e.g., #f8f9fa).");

        RuleFor(x => x.ThemeSettings.TextColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.TextColor))
            .WithMessage("Text color must be a valid hex color (e.g., #212529).");

        RuleFor(x => x.ThemeSettings.BackgroundColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.BackgroundColor))
            .WithMessage("Background color must be a valid hex color (e.g., #ffffff).");

        RuleFor(x => x.ThemeSettings.BorderColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => !string.IsNullOrEmpty(x.ThemeSettings?.BorderColor))
            .WithMessage("Border color must be a valid hex color (e.g., #dee2e6).");
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) && 
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}