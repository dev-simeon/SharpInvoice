namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

public record UpdateMyProfileCommand(UpdateProfileDto ProfileDto) : IRequest;