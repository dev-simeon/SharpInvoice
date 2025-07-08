namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;

public record GetMyProfileQuery : IRequest<ProfileDto>;