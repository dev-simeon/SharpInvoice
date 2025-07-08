using FluentValidation;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

namespace SharpInvoice.API.Validators.UserManagement;

/// <summary>
/// Validator for the InviteTeamMemberRequest used when inviting a new team member.
/// </summary>
public class InviteTeamMemberRequestValidator : AbstractValidator<InviteTeamMemberRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InviteTeamMemberRequestValidator"/> class.
    /// </summary>
    public InviteTeamMemberRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email address cannot exceed 256 characters.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("A role must be assigned to the invitation.");
    }
}