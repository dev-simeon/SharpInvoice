namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class UnlinkAccountCommandHandler(
    IExternalLoginRepository externalLoginRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UnlinkAccountCommand>
{
    public async Task Handle(UnlinkAccountCommand request, CancellationToken cancellationToken)
    {
        var logins = await externalLoginRepository.GetByUserIdAsync(request.UserId);
        var loginToRemove = logins.FirstOrDefault(l => l.LoginProvider == request.Provider);

        if (loginToRemove != null)
        {
            await externalLoginRepository.DeleteAsync(loginToRemove.Id);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}