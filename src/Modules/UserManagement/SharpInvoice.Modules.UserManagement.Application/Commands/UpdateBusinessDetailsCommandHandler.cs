namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class UpdateBusinessDetailsCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBusinessDetailsCommand>
{
    public async Task Handle(UpdateBusinessDetailsCommand request, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(request.BusinessId)
            ?? throw new NotFoundException($"Business with ID {request.BusinessId} not found.");

        business.UpdateDetails(request.Dto.Name, request.Dto.Email, request.Dto.PhoneNumber, request.Dto.Website);
        business.UpdateAddress(request.Dto.Address, request.Dto.City, request.Dto.State, request.Dto.ZipCode, request.Dto.Country ?? business.Country);
        business.UpdateBranding(request.Dto.LogoUrl, JsonSerializer.Serialize(request.Dto.ThemeSettings));

        await businessRepository.UpdateAsync(business);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}