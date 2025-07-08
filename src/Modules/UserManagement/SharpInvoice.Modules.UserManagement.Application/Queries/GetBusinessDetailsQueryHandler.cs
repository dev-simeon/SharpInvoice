namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Dtos;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class GetBusinessDetailsQueryHandler(IBusinessRepository businessRepository) : IRequestHandler<GetBusinessDetailsQuery, BusinessDetailsDto>
{
    public async Task<BusinessDetailsDto> Handle(GetBusinessDetailsQuery request, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(request.BusinessId)
            ?? throw new NotFoundException($"Business with ID {request.BusinessId} not found.");

        return new BusinessDetailsDto(
            business.Id, business.Name, business.Email, business.PhoneNumber, business.Website,
            business.Address, business.City, business.State, business.ZipCode, business.Country,
            business.LogoUrl, business.IsActive
        );
    }
}