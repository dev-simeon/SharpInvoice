using FluentValidation;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

namespace SharpInvoice.API.Validators.UserManagement;

/// <summary>
/// Validator for the InviteTeamMemberRequest used when inviting a team member.
/// </summary>
public class InviteTeamMemberRequestValidator : AbstractValidator<InviteTeamMemberRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InviteTeamMemberRequestValidator"/> class.
    /// </summary>
    public InviteTeamMemberRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");
    }
} 