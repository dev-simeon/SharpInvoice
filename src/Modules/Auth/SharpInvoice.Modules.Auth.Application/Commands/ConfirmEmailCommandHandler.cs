namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;

public class ConfirmEmailCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmEmailCommand, bool>
{
    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null || user.EmailConfirmationToken != request.Token)
        {
            return false;
        }

        user.ConfirmEmail();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}