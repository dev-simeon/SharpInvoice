namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the request to invite a team member.
/// </summary>
/// <param name="Email">The email of the user to invite.</param>
/// <param name="RoleId">The ID of the role to assign to the new member.</param>
public record InviteTeamMemberRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] Guid RoleId
); 