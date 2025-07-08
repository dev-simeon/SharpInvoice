namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System;

public class GetBusinessDetailsQuery(Guid businessId) : IRequest<BusinessDetailsDto>
{
    public Guid BusinessId { get; } = businessId;
}