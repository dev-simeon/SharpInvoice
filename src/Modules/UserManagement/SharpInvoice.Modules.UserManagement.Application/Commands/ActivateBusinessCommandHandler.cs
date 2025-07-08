namespace SharpInvoice.Modules.UserManagement.Application.Commands;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;
using SharpInvoice.Shared.Infrastructure.Interfaces;
using SharpInvoice.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

public class ActivateBusinessCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActivateBusinessCommand>
{
    public async Task Handle(ActivateBusinessCommand request, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(request.BusinessId)
            ?? throw new NotFoundException($"Business with ID {request.BusinessId} not found.");

        business.Activate();

        await businessRepository.UpdateAsync(business);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}