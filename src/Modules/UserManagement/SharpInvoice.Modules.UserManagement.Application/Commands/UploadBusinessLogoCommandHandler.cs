namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class UploadBusinessLogoCommandHandler(
    IBusinessRepository businessRepository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadBusinessLogoCommand>
{
    public async Task Handle(UploadBusinessLogoCommand request, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(request.BusinessId)
            ?? throw new NotFoundException($"Business with ID {request.BusinessId} not found.");

        var logoUrl = await fileStorageService.SaveFileAsync(request.LogoStream, request.FileName, "logos");
        business.UpdateBranding(logoUrl, business.ThemeSettings);

        await businessRepository.UpdateAsync(business);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}