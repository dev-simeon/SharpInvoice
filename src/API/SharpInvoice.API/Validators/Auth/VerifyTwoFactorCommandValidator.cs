using FluentValidation;
using SharpInvoice.Modules.Auth.Application.Commands;

namespace SharpInvoice.API.Validators.Auth;

/// <summary>
/// Validator for the VerifyTwoFactorCommand used during 2FA verification.
/// </summary>
public class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyTwoFactorCommandValidator"/> class.
    /// </summary>
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");
    }
}