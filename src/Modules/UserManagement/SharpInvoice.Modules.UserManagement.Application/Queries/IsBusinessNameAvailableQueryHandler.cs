namespace SharpInvoice.Modules.UserManagement.Application.Queries;

using MediatR;
using SharpInvoice.Modules.UserManagement.Application.Interfaces;

public class IsBusinessNameAvailableQueryHandler(IBusinessRepository businessRepository) : IRequestHandler<IsBusinessNameAvailableQuery, bool>
{
    public async Task<bool> Handle(IsBusinessNameAvailableQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Country))
            return false;
            
        var business = await businessRepository.GetByNameAndCountryAsync(request.Name, request.Country);
            
        return business == null;
    }
}