using FluentValidation;
using SharpInvoice.API.Dtos;
using SharpInvoice.Core.Interfaces.Services;

namespace SharpInvoice.API.Validators;

/// <summary>
/// Validator for CreateInvoiceRequest.
/// </summary>
public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    private readonly ILocalizationService _localizationService;

    public CreateInvoiceRequestValidator(ILocalizationService localizationService)
    {
        _localizationService = localizationService;

        RuleFor(x => x.BusinessId)
            .NotEmpty()
            .WithMessage("Business ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be exactly 3 characters")
            .Must(currency => _localizationService.IsCurrencySupported(currency))
            .WithMessage("Invalid or unsupported currency code");

        RuleFor(x => x.IssueDate)
            .NotEmpty()
            .WithMessage("Issue date is required")
            .Must(BeValidDate)
            .WithMessage("Issue date cannot be in the future");

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .WithMessage("Due date is required")
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date must be on or after issue date");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 1000 characters");

        RuleFor(x => x.Terms)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Terms))
            .WithMessage("Terms must not exceed 2000 characters");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one invoice item is required")
            .Must(items => items.Count <= 50)
            .WithMessage("Invoice cannot have more than 50 items");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateInvoiceItemRequestValidator());
    }

    private static bool BeValidBusinessId(string businessId)
    {
        return !string.IsNullOrEmpty(businessId) && businessId.StartsWith("biz_");
    }

    private static bool BeValidDate(DateTime date)
    {
        return date.Date <= DateTime.UtcNow.Date;
    }


}

/// <summary>
/// Validator for CreateInvoiceItemRequest.
/// </summary>
public class CreateInvoiceItemRequestValidator : AbstractValidator<CreateInvoiceItemRequest>
{
    public CreateInvoiceItemRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Item description is required")
            .MaximumLength(200)
            .WithMessage("Item description must not exceed 200 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Quantity must not exceed 10,000");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Unit price must not exceed 1,000,000");

        // Total is calculated automatically, no validation needed
    }
}