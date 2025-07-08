using FluentValidation;
using SharpInvoice.Modules.Auth.Application.Commands;

namespace SharpInvoice.API.Validators.Auth;

/// <summary>
/// Validator for the RefreshTokenRequest used during token refresh.
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenRequestValidator"/> class.
    /// </summary>
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}