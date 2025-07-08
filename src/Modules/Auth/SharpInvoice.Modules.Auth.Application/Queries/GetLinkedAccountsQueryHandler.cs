namespace SharpInvoice.Modules.Auth.Application.Queries;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetLinkedAccountsQueryHandler(
    IExternalLoginRepository externalLoginRepository,
    IUserRepository userRepository)
    : IRequestHandler<GetLinkedAccountsQuery, IEnumerable<ExternalAccountDto>>
{
    public async Task<IEnumerable<ExternalAccountDto>> Handle(GetLinkedAccountsQuery request, CancellationToken cancellationToken)
    {
        var externalLogins = await externalLoginRepository.GetByUserIdAsync(request.UserId);
        var user = await userRepository.GetByIdAsync(request.UserId);
        var userEmail = user?.Email ?? string.Empty;

        return externalLogins.Select(login => new ExternalAccountDto(
            login.LoginProvider,
            login.LoginProvider, // Using provider name as display name for now
            userEmail,
            login.CreatedAt)
        );
    }
}