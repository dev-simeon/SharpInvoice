using FluentValidation;
using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Validators;

/// <summary>
/// Validator for CreateBusinessRequest.
/// </summary>
public class CreateBusinessRequestValidator : AbstractValidator<CreateBusinessRequest>
{
    public CreateBusinessRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Business name is required")
            .MaximumLength(100)
            .WithMessage("Business name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.&]+$")
            .WithMessage("Business name contains invalid characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Website)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Invalid website URL format");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(50)
            .WithMessage("Country must not exceed 50 characters");

        RuleFor(x => x.Address)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage("Address must not exceed 255 characters");

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.State))
            .WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.ZipCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.ZipCode))
            .WithMessage("Zip code must not exceed 20 characters");
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}