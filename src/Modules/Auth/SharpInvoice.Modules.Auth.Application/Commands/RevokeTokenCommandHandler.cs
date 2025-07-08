namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public class RevokeTokenCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeTokenCommand, bool>
{
    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (user == null)
        {
            return false;
        }

        user.RevokeRefreshToken();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}