namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await passwordResetTokenRepository.GetByTokenAsync(request.Token);
        if (storedToken == null || storedToken.UserEmail != request.Email || storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return false;
        }

        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return false;
        }

        var newPasswordHash = passwordService.HashPassword(request.NewPassword);
        user.SetPasswordHash(newPasswordHash);

        // Invalidate the token after use
        storedToken.MarkAsUsed();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}