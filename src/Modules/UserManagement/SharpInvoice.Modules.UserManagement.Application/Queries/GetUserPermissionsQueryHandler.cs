namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetUserPermissionsQueryHandler(
    ITeamMemberRepository teamMemberRepository, 
    IRoleRepository roleRepository) 
    : IRequestHandler<GetUserPermissionsQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var teamMember = await teamMemberRepository.GetByUserIdAndBusinessIdAsync(request.UserId, request.BusinessId);
        if (teamMember == null)
        {
            return Enumerable.Empty<string>();
        }

        var role = await roleRepository.GetByIdWithPermissionsAsync(teamMember.RoleId);
        if (role == null)
        {
            return Enumerable.Empty<string>();
        }

        return role.Permissions
            .Select(rp => rp.Permission.Name);
    }
} 