namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class DeactivateBusinessCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateBusinessCommand>
{
    public async Task Handle(DeactivateBusinessCommand request, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(request.BusinessId)
            ?? throw new NotFoundException($"Business with ID {request.BusinessId} not found.");

        business.Deactivate();
        
        await businessRepository.UpdateAsync(business);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}