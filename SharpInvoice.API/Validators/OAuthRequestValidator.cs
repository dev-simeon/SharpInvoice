using FluentValidation;
using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Validators;

/// <summary>
/// Validator for OAuth login requests.
/// </summary>
public class OAuthLoginRequestValidator : AbstractValidator<OAuthLoginRequest>
{
    private static readonly string[] SupportedProviders = { "google", "facebook" };

    public OAuthLoginRequestValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("OAuth provider is required.")
            .Must(BeValidProvider)
            .WithMessage("OAuth provider must be one of: google, facebook.");

        RuleFor(x => x.ReturnUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.ReturnUrl))
            .WithMessage("Return URL must be a valid URL.");
    }

    private static bool BeValidProvider(string provider)
    {
        return !string.IsNullOrEmpty(provider) && 
               SupportedProviders.Contains(provider.ToLowerInvariant());
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) && 
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validator for OAuth callback requests.
/// </summary>
public class OAuthCallbackRequestValidator : AbstractValidator<OAuthCallbackRequest>
{
    private static readonly string[] SupportedProviders = { "google", "facebook" };

    public OAuthCallbackRequestValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("OAuth provider is required.")
            .Must(BeValidProvider)
            .WithMessage("OAuth provider must be one of: google, facebook.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Authorization code is required.")
            .When(x => string.IsNullOrEmpty(x.Error))
            .WithMessage("Authorization code is required when no error is present.");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State parameter is required for security validation.")
            .MinimumLength(10)
            .WithMessage("State parameter must be at least 10 characters.");
    }

    private static bool BeValidProvider(string provider)
    {
        return !string.IsNullOrEmpty(provider) && 
               SupportedProviders.Contains(provider.ToLowerInvariant());
    }
}