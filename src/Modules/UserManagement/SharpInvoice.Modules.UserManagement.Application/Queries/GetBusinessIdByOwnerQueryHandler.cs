namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetBusinessIdByOwnerQueryHandler(IBusinessRepository businessRepository) : IRequestHandler<GetBusinessIdByOwnerQuery, Guid>
{
    public async Task<Guid> Handle(GetBusinessIdByOwnerQuery request, CancellationToken cancellationToken)
    {
        var businesses = await businessRepository.GetByOwnerIdAsync(request.OwnerId);
        var business = businesses.FirstOrDefault();
        return business?.Id ?? Guid.Empty;
    }
} 