namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System;

/// <summary>
/// Represents a team member in a business.
/// </summary>
/// <param name="Id">The unique identifier for the team member relationship.</param>
/// <param name="UserId">The unique identifier of the user who is a team member.</param>
/// <param name="UserName">The display name of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="RoleId">The unique identifier of the role assigned to this team member.</param>
/// <param name="RoleName">The name of the role (e.g., "Admin", "Editor", "Viewer").</param>
/// <param name="JoinedAt">The date and time when the user joined the team.</param>
public record TeamMemberDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime JoinedAt
);