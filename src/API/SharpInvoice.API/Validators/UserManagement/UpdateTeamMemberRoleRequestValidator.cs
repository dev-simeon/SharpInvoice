using FluentValidation;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

namespace SharpInvoice.API.Validators.UserManagement;

/// <summary>
/// Validator for the UpdateTeamMemberRoleRequest used when updating a team member's role.
/// </summary>
public class UpdateTeamMemberRoleRequestValidator : AbstractValidator<UpdateTeamMemberRoleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTeamMemberRoleRequestValidator"/> class.
    /// </summary>
    public UpdateTeamMemberRoleRequestValidator()
    {
        RuleFor(x => x.NewRoleId)
            .NotEmpty().WithMessage("New role ID is required.");
    }
} 