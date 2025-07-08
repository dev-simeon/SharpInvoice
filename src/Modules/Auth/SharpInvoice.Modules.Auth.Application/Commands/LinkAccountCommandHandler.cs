namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public class LinkAccountCommandHandler(
    IExternalLoginRepository externalLoginRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<LinkAccountCommand, bool>
{
    public async Task<bool> Handle(LinkAccountCommand request, CancellationToken cancellationToken)
    {
        var existingLogin = await externalLoginRepository.GetByProviderAndKeyAsync(request.Provider, request.ProviderKey);
        if (existingLogin != null)
        {
            // This external account is already linked to someone, possibly this same user.
            // If it's linked to a different user, we should not allow it.
            return existingLogin.UserId == request.UserId;
        }

        var newLogin = ExternalLogin.Create(request.UserId, request.Provider, request.ProviderKey);
        await externalLoginRepository.AddAsync(newLogin);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}