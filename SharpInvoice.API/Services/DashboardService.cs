using SharpInvoice.API.Dtos;
using SharpInvoice.API.Interfaces.Services;
using SharpInvoice.Core.Interfaces;

namespace SharpInvoice.API.Services;

public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<DashboardDto> GetDashboardDataAsync(string businessId)
    {
        var totalRevenue = await _unitOfWork.Invoices.GetTotalRevenueAsync(businessId);
        var totalOutstanding = await _unitOfWork.Invoices.GetTotalOutstandingAsync(businessId);
        var totalClients = await _unitOfWork.Clients.CountAsync(c => c.BusinessId == businessId);
        var totalInvoices = await _unitOfWork.Invoices.CountAsync(i => i.BusinessId == businessId);

        return new DashboardDto
        {
            TotalRevenue = totalRevenue,
            TotalOutstanding = totalOutstanding,
            TotalClients = totalClients,
            TotalInvoices = totalInvoices
        };
    }
} 