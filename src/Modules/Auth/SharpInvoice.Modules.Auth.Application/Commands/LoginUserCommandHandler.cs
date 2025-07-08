namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.UserManagement.Application.Queries;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthInterfaces = SharpInvoice.Modules.Auth.Application.Interfaces;

public class LoginUserCommandHandler(
    AuthInterfaces.IUserRepository userRepository,
    IPasswordService passwordService,
    IAuthService authService,
    IUnitOfWork unitOfWork,
    IMediator mediator) : IRequestHandler<LoginUserCommand, LoginResponseDto>
{
    public async Task<LoginResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);

        if (user == null || !passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            throw new UnauthorizedAccessException("Please confirm your email before logging in.");
        }

        if (user.TwoFactorEnabled)
        {
            user.GenerateTwoFactorCode();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new LoginResponseDto(true, "A verification code has been sent to your email.");
        }

        var businessId = await mediator.Send(new GetBusinessIdByOwnerQuery(user.Id), cancellationToken);
        if (businessId == System.Guid.Empty)
        {
            var tempToken = await authService.GenerateTokensAsync(user, System.Guid.Empty, Enumerable.Empty<string>(), Enumerable.Empty<string>(), true);
            return new LoginResponseDto(false, AuthResponse: tempToken);
        }

        var roles = await mediator.Send(new GetUserRolesQuery(user.Id, businessId), cancellationToken);
        var permissions = await mediator.Send(new GetUserPermissionsQuery(user.Id, businessId), cancellationToken);

        var authResponse = await authService.GenerateTokensAsync(user, businessId, roles, permissions, false);
        return new LoginResponseDto(false, AuthResponse: authResponse);
    }
}