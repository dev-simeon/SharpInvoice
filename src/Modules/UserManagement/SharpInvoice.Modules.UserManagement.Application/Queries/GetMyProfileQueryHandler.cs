namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class GetMyProfileQueryHandler(
    IUserRepository userRepository,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<GetMyProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        return new ProfileDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.AvatarUrl
        );
    }
}