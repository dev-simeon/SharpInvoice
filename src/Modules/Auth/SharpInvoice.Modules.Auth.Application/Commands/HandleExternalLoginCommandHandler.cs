namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using System;
using SharpInvoice.Modules.UserManagement.Application.Queries;
using SharpInvoice.Shared.Kernel.Exceptions;
using AuthInterfaces = SharpInvoice.Modules.Auth.Application.Interfaces;

public class HandleExternalLoginCommandHandler(
    AuthInterfaces.IUserRepository userRepository,
    IExternalLoginRepository externalLoginRepository,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    IMediator mediator) : IRequestHandler<HandleExternalLoginCommand, AuthResponseDto>
{
    private HttpContext HttpContext => httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");

    public async Task<AuthResponseDto> Handle(HandleExternalLoginCommand request, CancellationToken cancellationToken)
    {
        var info = await HttpContext.AuthenticateAsync("Identity.External") ?? throw new ExternalAuthenticationException("Error loading external login information.");

        if (info.Principal is null || info.Properties is null)
        {
            throw new ExternalAuthenticationException("External principal or properties are null.");
        }

        var provider = info.Properties.Items["LoginProvider"] ?? throw new ExternalAuthenticationException("Login provider not found.");
        var providerKey = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new ExternalAuthenticationException("Provider key not found.");

        var externalLogin = await externalLoginRepository.GetByProviderAndKeyAsync(provider, providerKey);
        if (externalLogin != null)
        {
            var user = await userRepository.GetByIdAsync(externalLogin.UserId);
            return await CreateAuthResponseForUser(user ?? throw new NotFoundException("User not found."), cancellationToken);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            throw new ExternalAuthenticationException("Email claim not received from external provider. Cannot create account.");
        }

        var existingUser = await userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            var newLogin = ExternalLogin.Create(existingUser.Id, provider, providerKey);
            await externalLoginRepository.AddAsync(newLogin);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return await CreateAuthResponseForUser(existingUser, cancellationToken);
        }

        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;

        var newUser = User.Create(email, firstName, lastName, string.Empty);
        newUser.ConfirmEmail();

        await userRepository.AddAsync(newUser);

        var newExternalLogin = ExternalLogin.Create(newUser.Id, provider, providerKey);
        await externalLoginRepository.AddAsync(newExternalLogin);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseForUser(newUser, cancellationToken, true);
    }

    private async Task<AuthResponseDto> CreateAuthResponseForUser(User user, CancellationToken cancellationToken, bool requiresProfileCompletion = false)
    {
        var businessId = await mediator.Send(new GetBusinessIdByOwnerQuery(user.Id), cancellationToken);

        if (requiresProfileCompletion || businessId == Guid.Empty)
        {
            var tempToken = await GenerateAndSaveTokens(user, Guid.Empty, Enumerable.Empty<string>(), Enumerable.Empty<string>(), true, cancellationToken);
            return tempToken;
        }

        var roles = await mediator.Send(new GetUserRolesQuery(user.Id, businessId), cancellationToken);
        var permissions = await mediator.Send(new GetUserPermissionsQuery(user.Id, businessId), cancellationToken);

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