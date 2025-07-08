using FluentValidation;
using SharpInvoice.Modules.Auth.Application.Commands;

namespace SharpInvoice.API.Validators.Auth;

/// <summary>
/// Validator for the ForgotPasswordCommand used during password reset requests.
/// </summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordCommandValidator"/> class.
    /// </summary>
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}