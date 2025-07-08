namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

public class CreateBusinessCommand(string name, string email, string country) : IRequest<BusinessDto>
{
    public string Name { get; } = name;
    public string Email { get; } = email;
    public string Country { get; } = country;
}