using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using SharpInvoice.Modules.Auth.Application.Commands;
using SharpInvoice.Modules.Auth.Application.Dtos;

namespace SharpInvoice.API.Examples
{
    /// <summary>
    /// Provides example data for RegisterUserCommand in Swagger documentation.
    /// </summary>
    public class RegisterUserCommandExample : IExamplesProvider<RegisterUserCommand>
    {
        /// <summary>
        /// Gets example instance of RegisterUserCommand.
        /// </summary>
        /// <returns>Sample RegisterUserCommand data.</returns>
        public RegisterUserCommand GetExamples()
        {
            return new RegisterUserCommand(
                Email: "john.doe@example.com",
                FirstName: "John",
                LastName: "Doe",
                Password: "StrongP@ssw0rd!"
            );
        }
    }

    /// <summary>
    /// Provides example data for LoginUserCommand in Swagger documentation.
    /// </summary>
    public class LoginUserCommandExample : IExamplesProvider<LoginUserCommand>
    {
        /// <summary>
        /// Gets example instance of LoginUserCommand.
        /// </summary>
        /// <returns>Sample LoginUserCommand data.</returns>
        public LoginUserCommand GetExamples()
        {
            return new LoginUserCommand(
                Email: "john.doe@example.com",
                Password: "StrongP@ssw0rd!"
            );
        }
    }

    /// <summary>
    /// Provides example data for RefreshTokenRequest in Swagger documentation.
    /// </summary>
    public class RefreshTokenRequestExample : IExamplesProvider<RefreshTokenRequest>
    {
        /// <summary>
        /// Gets example instance of RefreshTokenRequest.
        /// </summary>
        /// <returns>Sample RefreshTokenRequest data.</returns>
        public RefreshTokenRequest GetExamples()
        {
            return new RefreshTokenRequest(
                RefreshToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
            );
        }
    }

    /// <summary>
    /// Provides example data for ForgotPasswordCommand in Swagger documentation.
    /// </summary>
    public class ForgotPasswordCommandExample : IExamplesProvider<ForgotPasswordCommand>
    {
        /// <summary>
        /// Gets example instance of ForgotPasswordCommand.
        /// </summary>
        /// <returns>Sample ForgotPasswordCommand data.</returns>
        public ForgotPasswordCommand GetExamples()
        {
            return new ForgotPasswordCommand(
                Email: "john.doe@example.com"
            );
        }
    }

    /// <summary>
    /// Provides example data for ResetPasswordCommand in Swagger documentation.
    /// </summary>
    public class ResetPasswordCommandExample : IExamplesProvider<ResetPasswordCommand>
    {
        /// <summary>
        /// Gets example instance of ResetPasswordCommand.
        /// </summary>
        /// <returns>Sample ResetPasswordCommand data.</returns>
        public ResetPasswordCommand GetExamples()
        {
            return new ResetPasswordCommand(
                Email: "john.doe@example.com",
                Token: "1a2b3c4d5e6f7g8h9i0j",
                NewPassword: "NewStrongP@ssw0rd!"
            );
        }
    }

    /// <summary>
    /// Provides example data for VerifyTwoFactorCommand in Swagger documentation.
    /// </summary>
    public class VerifyTwoFactorCommandExample : IExamplesProvider<VerifyTwoFactorCommand>
    {
        /// <summary>
        /// Gets example instance of VerifyTwoFactorCommand.
        /// </summary>
        /// <returns>Sample VerifyTwoFactorCommand data.</returns>
        public VerifyTwoFactorCommand GetExamples()
        {
            return new VerifyTwoFactorCommand(
                Email: "john.doe@example.com",
                Code: "123456"
            );
        }
    }

    /// <summary>
    /// Provides example data for AuthResponseDto in Swagger documentation.
    /// </summary>
    public class AuthResponseDtoExample : IExamplesProvider<AuthResponseDto>
    {
        /// <summary>
        /// Gets example instance of AuthResponseDto.
        /// </summary>
        /// <returns>Sample AuthResponseDto data.</returns>
        public AuthResponseDto GetExamples()
        {
            return new AuthResponseDto(
                UserId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                Email: "john.doe@example.com",
                Token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                RefreshToken: "6e8b4567-1234-5678-abcd-1234567890ab"
            );
        }
    }

    /// <summary>
    /// Provides example data for LoginResponseDto in Swagger documentation.
    /// </summary>
    public class LoginResponseDtoExample : IExamplesProvider<LoginResponseDto>
    {
        /// <summary>
        /// Gets example instance of LoginResponseDto.
        /// </summary>
        /// <returns>Sample LoginResponseDto data.</returns>
        public LoginResponseDto GetExamples()
        {
            return new LoginResponseDto(
                IsTwoFactorRequired: true,
                Message: "Please check your email for a verification code."
            );
        }
    }

    /// <summary>
    /// Provides example data for RegisterResponseDto in Swagger documentation.
    /// </summary>
    public class RegisterResponseDtoExample : IExamplesProvider<RegisterResponseDto>
    {
        /// <summary>
        /// Gets example instance of RegisterResponseDto.
        /// </summary>
        /// <returns>Sample RegisterResponseDto data.</returns>
        public RegisterResponseDto GetExamples()
        {
            return new RegisterResponseDto(
                UserId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                Message: "Registration successful. Please check your email to confirm your account."
            );
        }
    }

    /// <summary>
    /// Provides example data for LogoutRequest in Swagger documentation.
    /// </summary>
    public class LogoutRequestExample : IExamplesProvider<LogoutRequest>
    {
        /// <summary>
        /// Gets example instance of LogoutRequest.
        /// </summary>
        /// <returns>Sample LogoutRequest data.</returns>
        public LogoutRequest GetExamples()
        {
            return new LogoutRequest(
                RefreshToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiZXhhbXBsZSIsInN1YiI6IjEyMzQ1Njc4OTAifQ.eBPN8pdcxWJ8hLiyvFgZgSZGv3EsH9iQ6J5so8L4Lac"
            );
        }
    }
}