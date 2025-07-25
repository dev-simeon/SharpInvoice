using FluentValidation;
using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Validators;

/// <summary>
/// Validator for CreateClientRequest.
/// </summary>
public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty()
            .WithMessage("Business ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Client name is required")
            .MaximumLength(100)
            .WithMessage("Client name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.&,]+$")
            .WithMessage("Client name contains invalid characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Address)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage("Address must not exceed 200 characters");

        RuleFor(x => x.Country)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Country))
            .WithMessage("Country must not exceed 50 characters");

        // Note: Locale field not in DTO, removing validation
    }

    private static bool BeValidBusinessId(string businessId)
    {
        // Business IDs should start with "biz_" prefix per KeyGenerator
        return !string.IsNullOrEmpty(businessId) && businessId.StartsWith("biz_");
    }

    private static bool BeValidLocale(string? locale)
    {
        if (string.IsNullOrEmpty(locale))
            return true;

        // Basic locale format validation (e.g., en-US, de-DE)
        return System.Text.RegularExpressions.Regex.IsMatch(locale, @"^[a-z]{2}-[A-Z]{2}$");
    }
}