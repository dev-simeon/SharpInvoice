namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user != null)
        {
            user.GeneratePasswordResetToken();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}