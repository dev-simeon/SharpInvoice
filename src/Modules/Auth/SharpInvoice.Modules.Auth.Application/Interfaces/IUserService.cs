namespace SharpInvoice.Modules.Auth.Application.Interfaces;

using System;
using System.Threading.Tasks;

public interface IUserService
{
    Task<UserDetailsDto> GetUserByIdAsync(Guid userId);
    Task UpdateUserAsync(Guid userId, UpdateUserDto userDto);
}

public record UserDetailsDto(Guid Id, string FirstName, string LastName, string Email);
public record UpdateUserDto(string FirstName, string LastName); 