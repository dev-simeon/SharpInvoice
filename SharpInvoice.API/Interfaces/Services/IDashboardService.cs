using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync(string businessId);
} 