namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public class CreateBusinessCommandHandler(
    IBusinessRepository businessRepository,
    ICurrentUserProvider currentUserProvider,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    ITeamMemberRepository teamMemberRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBusinessCommand, BusinessDto>
{
    public async Task<BusinessDto> Handle(CreateBusinessCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var business = Business.Create(request.Name, userId, request.Country);
        business.UpdateDetails(request.Name, request.Email, null, null);

        var ownerRole = await GetOrCreateOwnerRoleAsync();

        var teamMember = TeamMember.Create(userId, business.Id, ownerRole.Id);

        await businessRepository.AddAsync(business);
        await teamMemberRepository.AddAsync(teamMember);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BusinessDto(business.Id, business.Name);
    }

    private async Task<Role> GetOrCreateOwnerRoleAsync()
    {
        var ownerRole = await roleRepository.GetByNameAsync("Owner");
        if (ownerRole == null)
        {
            ownerRole = Role.Create("Owner", "Full administrative access to the business.");
            
            var allPermissions = await permissionRepository.GetAllAsync();
            foreach (var perm in allPermissions)
            {
                ownerRole.AddPermission(perm);
            }
            await roleRepository.AddAsync(ownerRole);
        }
        return ownerRole;
    }
}