namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetMyBusinessesQueryHandler(
    IBusinessRepository businessRepository,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<GetMyBusinessesQuery, IEnumerable<BusinessDto>>
{
    public async Task<IEnumerable<BusinessDto>> Handle(GetMyBusinessesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var businesses = await businessRepository.GetByUserIdAsync(userId);
        return businesses.Select(b => new BusinessDto(b.Id, b.Name));
    }
}