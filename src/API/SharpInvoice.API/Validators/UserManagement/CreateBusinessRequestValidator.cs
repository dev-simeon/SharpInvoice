using FluentValidation;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

namespace SharpInvoice.API.Validators.UserManagement;

/// <summary>
/// Validator for the CreateBusinessRequest used when creating a new business.
/// </summary>
public class CreateBusinessRequestValidator : AbstractValidator<CreateBusinessRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBusinessRequestValidator"/> class.
    /// </summary>
    public CreateBusinessRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Business name is required.")
            .MinimumLength(2).WithMessage("Business name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Business name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Business email is required.")
            .EmailAddress().WithMessage("A valid business email address is required.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .Length(2).WithMessage("Country must be a 2-letter ISO country code.");
    }
} 