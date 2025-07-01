namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System;

public record TeamMemberDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime JoinedAt
); 