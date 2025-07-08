namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GetTeamMembersQueryHandler(ITeamMemberRepository teamMemberRepository) : IRequestHandler<GetTeamMembersQuery, IEnumerable<TeamMemberDto>>
{
    public async Task<IEnumerable<TeamMemberDto>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var teamMembers = await teamMemberRepository.GetByBusinessIdAsync(request.BusinessId);
        return teamMembers.Select(tm => new TeamMemberDto(
            tm.Id,
            tm.UserId,
            tm.User.FirstName,
            tm.User.Email,
            tm.RoleId,
            tm.Role.Name,
            tm.CreatedAt
        ));
    }
}