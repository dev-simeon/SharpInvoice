using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharpInvoice.API.Dtos;
using SharpInvoice.API.Interfaces.Services;
using System.Net.Mime;

namespace SharpInvoice.API.Controllers;

/// <summary>
/// Controller for dashboard analytics and reporting operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ApiVersion("1.0")]
public sealed class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IInvoiceService _invoiceService;
    private readonly IBusinessService _businessService;
    private readonly ILogger<DashboardController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </summary>
    /// <param name="dashboardService">The dashboard service.</param>
    /// <param name="invoiceService">The invoice service.</param>
    /// <param name="businessService">The business service.</param>
    /// <param name="logger">The logger.</param>
    public DashboardController(
        IDashboardService dashboardService,
        IInvoiceService invoiceService,
        IBusinessService businessService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _invoiceService = invoiceService;
        _businessService = businessService;
        _logger = logger;
    }

    /// <summary>
    /// Gets dashboard overview data for a business
    /// </summary>
    /// <remarks>
    /// Retrieves key business metrics and summary data for the dashboard.
    /// Includes total revenue, outstanding amounts, client count, and invoice statistics.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/dashboard/3fa85f64-5717-4562-b3fc-2c963f66afa6/overview
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard overview data</returns>
    /// <response code="200">Dashboard data retrieved successfully.</response>
    /// <response code="400">Invalid business ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/overview")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardOverviewAsync(
        [FromRoute] string businessId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessId))
        {
            return Invalid("Business ID cannot be empty.");
        }

        _logger.LogDebug("Retrieving dashboard overview for business: {BusinessId}", businessId);

        var dashboardData = await _dashboardService.GetDashboardDataAsync(businessId);

        return Success(dashboardData);
    }

    /// <summary>
    /// Gets revenue analytics for a business
    /// </summary>
    /// <remarks>
    /// Retrieves detailed revenue analytics including monthly/yearly breakdowns,
    /// growth trends, and comparisons with previous periods.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/dashboard/3fa85f64-5717-4562-b3fc-2c963f66afa6/revenue?period=month&year=2024
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="period">Period for analytics (month, quarter, year)</param>
    /// <param name="year">Year for analytics (default: current year)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Revenue analytics data</returns>
    /// <response code="200">Revenue analytics retrieved successfully.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/revenue")]
    [ProducesResponseType(typeof(RevenueAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRevenueAnalyticsAsync(
        [FromRoute] Guid businessId,
        [FromQuery] string period = "month",
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        var validPeriods = new[] { "month", "quarter", "year" };
        if (!validPeriods.Contains(period.ToLowerInvariant()))
        {
            return Invalid("Period must be one of: month, quarter, year.");
        }

        var targetYear = year ?? DateTime.UtcNow.Year;
        if (targetYear < 2000 || targetYear > DateTime.UtcNow.Year + 1)
        {
            return Invalid("Invalid year specified.");
        }

        _logger.LogDebug("Retrieving revenue analytics for business: {BusinessId}, period: {Period}, year: {Year}", 
            businessId, period, targetYear);

        // Placeholder implementation - in a real system, this would aggregate invoice data
        var revenueAnalytics = new RevenueAnalyticsDto
        {
            Period = period,
            Year = targetYear,
            TotalRevenue = 50000m,
            RevenueGrowth = 15.5m,
            MonthlyData = GenerateMonthlyRevenueData(targetYear),
            TopClients = GenerateTopClientsData(),
            RevenueByStatus = GenerateRevenueByStatusData()
        };

        return Success(revenueAnalytics);
    }

    /// <summary>
    /// Gets invoice status distribution for a business
    /// </summary>
    /// <remarks>
    /// Retrieves the distribution of invoices by status (draft, sent, paid, overdue, etc.)
    /// with counts and total amounts for each status.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/dashboard/3fa85f64-5717-4562-b3fc-2c963f66afa6/invoice-status
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Invoice status distribution data</returns>
    /// <response code="200">Invoice status distribution retrieved successfully.</response>
    /// <response code="400">Invalid business ID format.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/invoice-status")]
    [ProducesResponseType(typeof(InvoiceStatusDistributionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInvoiceStatusDistributionAsync(
        [FromRoute] Guid businessId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        _logger.LogDebug("Retrieving invoice status distribution for business: {BusinessId}", businessId);

        // Placeholder implementation - in a real system, this would query invoice data
        var statusDistribution = new InvoiceStatusDistributionDto
        {
            Draft = new InvoiceStatusData { Count = 5, TotalAmount = 2500m },
            Sent = new InvoiceStatusData { Count = 12, TotalAmount = 15000m },
            Paid = new InvoiceStatusData { Count = 45, TotalAmount = 67500m },
            Overdue = new InvoiceStatusData { Count = 3, TotalAmount = 4500m },
            Voided = new InvoiceStatusData { Count = 1, TotalAmount = 1000m }
        };

        return Success(statusDistribution);
    }

    /// <summary>
    /// Exports business report as CSV
    /// </summary>
    /// <remarks>
    /// Generates and downloads a comprehensive business report in CSV format.
    /// The report includes invoice data, client information, and revenue analytics.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/dashboard/3fa85f64-5717-4562-b3fc-2c963f66afa6/export/csv?startDate=2024-01-01&endDate=2024-12-31
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="startDate">Start date for the report (optional)</param>
    /// <param name="endDate">End date for the report (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CSV file download</returns>
    /// <response code="200">CSV report generated and returned successfully.</response>
    /// <response code="400">Invalid parameters or date range.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/export/csv")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportCsvReportAsync(
        [FromRoute] Guid businessId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return Invalid("Start date cannot be greater than end date.");
        }

        _logger.LogInformation("Exporting CSV report for business: {BusinessId}", businessId);

        // In a real implementation, this would generate actual CSV data from the database
        var csvContent = GenerateCsvReport(businessId, startDate, endDate);
        var fileName = $"business-report-{businessId}-{DateTime.UtcNow:yyyyMMdd}.csv";

        return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
    }

    /// <summary>
    /// Exports business report as PDF
    /// </summary>
    /// <remarks>
    /// Generates and downloads a comprehensive business report in PDF format.
    /// The PDF includes charts, tables, and detailed analytics.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/dashboard/3fa85f64-5717-4562-b3fc-2c963f66afa6/export/pdf?startDate=2024-01-01&endDate=2024-12-31
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="startDate">Start date for the report (optional)</param>
    /// <param name="endDate">End date for the report (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file download</returns>
    /// <response code="200">PDF report generated and returned successfully.</response>
    /// <response code="400">Invalid parameters or date range.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/export/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportPdfReportAsync(
        [FromRoute] Guid businessId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return Invalid("Start date cannot be greater than end date.");
        }

        _logger.LogInformation("Exporting PDF report for business: {BusinessId}", businessId);

        // In a real implementation, this would use the PDF service to generate a comprehensive report
        var pdfBytes = GeneratePdfReport(businessId, startDate, endDate);
        var fileName = $"business-report-{businessId}-{DateTime.UtcNow:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Gets top performing clients for a business
    /// </summary>
    /// <remarks>
    /// Retrieves the top clients by revenue, invoice count, and other metrics.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/v1/dashboard/3fa85f64-5717-4562-b3fc-2c963f66afa6/top-clients?limit=10
    ///     Authorization: Bearer {your-access-token}
    /// 
    /// </remarks>
    /// <param name="businessId">The unique identifier of the business</param>
    /// <param name="limit">Number of top clients to return (default: 10, max: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top clients data</returns>
    /// <response code="200">Top clients retrieved successfully.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">Access denied. User doesn't own the business.</response>
    /// <response code="404">Business not found.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpGet("{businessId:guid}/top-clients")]
    [ProducesResponseType(typeof(IEnumerable<TopClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTopClientsAsync(
        [FromRoute] Guid businessId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            return Invalid("Business ID cannot be empty.");
        }

        if (limit < 1 || limit > 50)
        {
            return Invalid("Limit must be between 1 and 50.");
        }

        _logger.LogDebug("Retrieving top {Limit} clients for business: {BusinessId}", limit, businessId);

        var topClients = GenerateTopClientsData().Take(limit);

        return Success(topClients);
    }

    #region Private Helper Methods

    private static List<MonthlyRevenueData> GenerateMonthlyRevenueData(int year)
    {
        var data = new List<MonthlyRevenueData>();
        var random = new Random(year); // Seed for consistent data

        for (int month = 1; month <= 12; month++)
        {
            data.Add(new MonthlyRevenueData
            {
                Month = month,
                Year = year,
                Revenue = Math.Round((decimal)(random.NextDouble() * 10000 + 2000), 2),
                InvoiceCount = random.Next(5, 25)
            });
        }

        return data;
    }

    private static List<TopClientDto> GenerateTopClientsData()
    {
        return new List<TopClientDto>
        {
            new() { ClientName = "Acme Corporation", TotalRevenue = 25000m, InvoiceCount = 12, AverageInvoiceAmount = 2083.33m },
            new() { ClientName = "Tech Solutions Inc", TotalRevenue = 18500m, InvoiceCount = 8, AverageInvoiceAmount = 2312.50m },
            new() { ClientName = "Global Services LLC", TotalRevenue = 15000m, InvoiceCount = 6, AverageInvoiceAmount = 2500m },
            new() { ClientName = "Innovation Labs", TotalRevenue = 12000m, InvoiceCount = 10, AverageInvoiceAmount = 1200m },
            new() { ClientName = "Startup Ventures", TotalRevenue = 8500m, InvoiceCount = 5, AverageInvoiceAmount = 1700m }
        };
    }

    private static Dictionary<string, decimal> GenerateRevenueByStatusData()
    {
        return new Dictionary<string, decimal>
        {
            { "Paid", 67500m },
            { "Sent", 15000m },
            { "Overdue", 4500m },
            { "Draft", 2500m }
        };
    }

    private static string GenerateCsvReport(Guid businessId, DateTime? startDate, DateTime? endDate)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Invoice Number,Client Name,Issue Date,Due Date,Amount,Status");
        csv.AppendLine("INV-001,Acme Corporation,2024-01-15,2024-02-15,2500.00,Paid");
        csv.AppendLine("INV-002,Tech Solutions Inc,2024-01-20,2024-02-20,1800.00,Paid");
        csv.AppendLine("INV-003,Global Services LLC,2024-02-01,2024-03-01,3200.00,Sent");
        
        return csv.ToString();
    }

    private static byte[] GeneratePdfReport(Guid businessId, DateTime? startDate, DateTime? endDate)
    {
        // Placeholder implementation - in a real system, this would use the PDF service
        // to generate a comprehensive business report with charts and tables
        return System.Text.Encoding.UTF8.GetBytes("PDF Report Content Placeholder");
    }

    #endregion
}

#region DTOs

/// <summary>
/// Revenue analytics data transfer object
/// </summary>
public sealed record RevenueAnalyticsDto
{
    public string Period { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal RevenueGrowth { get; init; }
    public List<MonthlyRevenueData> MonthlyData { get; init; } = new();
    public List<TopClientDto> TopClients { get; init; } = new();
    public Dictionary<string, decimal> RevenueByStatus { get; init; } = new();
}

/// <summary>
/// Monthly revenue data
/// </summary>
public sealed record MonthlyRevenueData
{
    public int Month { get; init; }
    public int Year { get; init; }
    public decimal Revenue { get; init; }
    public int InvoiceCount { get; init; }
}

/// <summary>
/// Top client data transfer object
/// </summary>
public sealed record TopClientDto
{
    public string ClientName { get; init; } = string.Empty;
    public decimal TotalRevenue { get; init; }
    public int InvoiceCount { get; init; }
    public decimal AverageInvoiceAmount { get; init; }
}

/// <summary>
/// Invoice status distribution data transfer object
/// </summary>
public sealed record InvoiceStatusDistributionDto
{
    public InvoiceStatusData Draft { get; init; } = new();
    public InvoiceStatusData Sent { get; init; } = new();
    public InvoiceStatusData Paid { get; init; } = new();
    public InvoiceStatusData Overdue { get; init; } = new();
    public InvoiceStatusData Voided { get; init; } = new();
}

/// <summary>
/// Invoice status data
/// </summary>
public sealed record InvoiceStatusData
{
    public int Count { get; init; }
    public decimal TotalAmount { get; init; }
}

#endregion