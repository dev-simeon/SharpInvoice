using FluentValidation;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

namespace SharpInvoice.API.Validators.UserManagement;

/// <summary>
/// Validator for the UpdateBusinessDetailsDto used when updating business details.
/// </summary>
public class UpdateBusinessDetailsDtoValidator : AbstractValidator<UpdateBusinessDetailsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBusinessDetailsDtoValidator"/> class.
    /// </summary>
    public UpdateBusinessDetailsDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Business name is required.")
            .MinimumLength(2).WithMessage("Business name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Business name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("A valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-\(\)]+$").When(x => !string.IsNullOrEmpty(x.PhoneNumber)).WithMessage("Phone number must be in a valid format.");

        RuleFor(x => x.Website)
            .Must(BeValidUrl).When(x => !string.IsNullOrEmpty(x.Website)).WithMessage("Website must be a valid URL.");
    }

    /// <summary>
    /// Checks if the provided URL is valid.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid or empty; otherwise, false.</returns>
    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
} 