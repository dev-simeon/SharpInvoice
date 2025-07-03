namespace SharpInvoice.Modules.UserManagement.Application.Dtos;

using System;
using System.ComponentModel.DataAnnotations;
 
/// <summary>
/// Represents the request to update a team member's role.
/// </summary>
/// <param name="NewRoleId">The ID of the new role.</param>
public record UpdateTeamMemberRoleRequest(
    [property: Required] Guid NewRoleId
); 