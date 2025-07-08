namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Queries;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Linq;
using System;
using AuthInterfaces = SharpInvoice.Modules.Auth.Application.Interfaces;

public class VerifyTwoFactorCommandHandler(
    AuthInterfaces.IUserRepository userRepository,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IMediator mediator) : IRequestHandler<VerifyTwoFactorCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.TwoFactorEnabled || user.TwoFactorCode != request.Code || user.TwoFactorCodeExpiry < DateTime.UtcNow)
        {
            throw new InvalidTokenException("Invalid or expired 2FA code.");
        }

        var businessId = await mediator.Send(new GetBusinessIdByOwnerQuery(user.Id), cancellationToken);
        if (businessId == Guid.Empty)
        {
            throw new InvalidOperationException("User is not associated with any active business.");
        }

        var roles = await mediator.Send(new GetUserRolesQuery(user.Id, businessId), cancellationToken);
        var permissions = await mediator.Send(new GetUserPermissionsQuery(user.Id, businessId), cancellationToken);

        user.ClearTwoFactorCode();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GenerateAndSaveTokens(user, businessId, roles, permissions, false, cancellationToken);
    }

    private async Task<AuthResponseDto> GenerateAndSaveTokens(User user, Guid businessId, IEnumerable<string> roles, IEnumerable<string> permissions, bool requiresProfileCompletion, CancellationToken cancellationToken)
    {
        var token = tokenGenerator.GenerateToken(user, businessId, roles, permissions);
        var refreshToken = user.GenerateRefreshToken();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AuthResponseDto(user.Id.ToString(), user.Email, token, refreshToken, requiresProfileCompletion);
    }
}