namespace BO.DTOs;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int TotalActiveUsers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}


