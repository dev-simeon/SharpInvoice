using FluentValidation;
using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Validators;

/// <summary>
/// Validator for enabling/disabling two-factor authentication.
/// </summary>
public class EnableTwoFactorRequestValidator : AbstractValidator<EnableTwoFactorRequest>
{
    public EnableTwoFactorRequestValidator()
    {
        // Enable is a required boolean, no additional validation needed
        // The framework handles the required validation through the data annotations
    }
}

/// <summary>
/// Validator for verifying two-factor authentication codes.
/// </summary>
public class VerifyTwoFactorRequestValidator : AbstractValidator<VerifyTwoFactorRequest>
{
    public VerifyTwoFactorRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email address is required.")
            .EmailAddress()
            .WithMessage("Please provide a valid email address.")
            .MaximumLength(254)
            .WithMessage("Email address cannot exceed 254 characters.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Verification code is required.")
            .Length(6)
            .WithMessage("Verification code must be exactly 6 digits.")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must contain only digits.");
    }
}