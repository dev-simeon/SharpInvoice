using System.ComponentModel.DataAnnotations;
using SharpInvoice.Core.Enums;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Represents the data required to invite a team member to a business.
/// </summary>
public class InviteTeamMemberRequest
{
    /// <summary>
    /// The email address of the user to invite.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The role to assign to the invited team member.
    /// </summary>
    [Required]
    public BusinessRole Role { get; set; }
} 