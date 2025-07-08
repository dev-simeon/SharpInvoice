namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Queries;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System;
using AuthInterfaces = SharpInvoice.Modules.Auth.Application.Interfaces;

public class RefreshTokenCommandHandler(
    AuthInterfaces.IUserRepository userRepository,
    IAuthService authService,
    IMediator mediator) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken)
            ?? throw new InvalidTokenException("Invalid refresh token.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new InvalidTokenException("Refresh token has expired.");
        }

        var businessId = await mediator.Send(new GetBusinessIdByOwnerQuery(user.Id), cancellationToken);
        if (businessId == Guid.Empty)
        {
            throw new InvalidOperationException("User is not associated with any active business.");
        }

        var roles = await mediator.Send(new GetUserRolesQuery(user.Id, businessId), cancellationToken);
        var permissions = await mediator.Send(new GetUserPermissionsQuery(user.Id, businessId), cancellationToken);

        return await authService.GenerateTokensAsync(user, businessId, roles, permissions, false);
    }
}