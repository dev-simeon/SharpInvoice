namespace SharpInvoice.API.Dtos;

public class DashboardDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int TotalClients { get; set; }
    public int TotalInvoices { get; set; }
} 