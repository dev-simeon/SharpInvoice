namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System;

public class UpdateBusinessDetailsCommand(Guid businessId, UpdateBusinessDetailsDto dto) : IRequest
{
    public Guid BusinessId { get; } = businessId;
    public UpdateBusinessDetailsDto Dto { get; } = dto;
}