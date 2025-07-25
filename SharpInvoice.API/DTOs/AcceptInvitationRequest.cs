using System.ComponentModel.DataAnnotations;

namespace SharpInvoice.API.Dtos;

/// <summary>
/// Represents the data required to accept a team member invitation.
/// </summary>
public class AcceptInvitationRequest
{
    /// <summary>
    /// The unique token for the invitation.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
} 