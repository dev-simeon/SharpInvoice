using FluentValidation;
using SharpInvoice.Modules.Auth.Application.Commands;

namespace SharpInvoice.API.Validators.Auth
{
    /// <summary>
    /// Validates LogoutRequest for token revocation.
    /// </summary>
    public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutRequestValidator"/> class.
        /// </summary>
        public LogoutRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required")
                .MinimumLength(20).WithMessage("Invalid refresh token format");
        }
    }
}