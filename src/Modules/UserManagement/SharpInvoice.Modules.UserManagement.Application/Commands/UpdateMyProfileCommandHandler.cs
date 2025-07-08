namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class UpdateMyProfileCommandHandler(
    IUserRepository userRepository,
    ICurrentUserProvider currentUserProvider,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateMyProfileCommand>
{
    public async Task Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found.");

        user.UpdateProfile(
            request.ProfileDto.FirstName,
            request.ProfileDto.LastName
        );

        user.UpdateContactInfo(
            request.ProfileDto.AvatarUrl,
            request.ProfileDto.PhoneNumber
        );

        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}