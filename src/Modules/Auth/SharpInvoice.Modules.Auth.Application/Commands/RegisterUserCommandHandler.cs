namespace SharpInvoice.Modules.Auth.Application.Commands;

using MediatR;
using SharpInvoice.Modules.Auth.Application.Dtos;
using SharpInvoice.Modules.Auth.Application.Interfaces;
using SharpInvoice.Modules.Auth.Domain.Entities;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterUserCommand, RegisterResponseDto>
{
    public async Task<RegisterResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var hashedPassword = passwordService.HashPassword(request.Password);
        var user = User.Create(request.Email, request.FirstName, request.LastName, hashedPassword);

        await userRepository.AddAsync(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponseDto(user.Id.ToString(), "Registration successful. Please check your email to confirm your account.");
    }
}