namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetUserRolesQueryHandler(ITeamMemberRepository teamMemberRepository) : IRequestHandler<GetUserRolesQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var teamMember = await teamMemberRepository.GetByUserIdAndBusinessIdAsync(request.UserId, request.BusinessId);
        if (teamMember == null)
        {
            return Enumerable.Empty<string>();
        }
        return new[] { teamMember.Role.Name };
    }
} 