namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using System.Collections.Generic;

public class GetMyBusinessesQuery : IRequest<IEnumerable<BusinessDto>>
{
}