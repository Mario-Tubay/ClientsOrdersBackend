namespace ClientOrderService.API.DTOs;

public class RecentOrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal Total { get; set; }
}

public class MonthlyActivityDto
{
    public string Month { get; set; } = string.Empty;
    public int Orders { get; set; }
    public int Percentage { get; set; }
}

public class DashboardStatsDto
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ActiveClients { get; set; }
    public int CompletionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RecentOrderSummaryDto> RecentOrders { get; set; } = new();
    public List<MonthlyActivityDto> MonthlyActivity { get; set; } = new();
}
