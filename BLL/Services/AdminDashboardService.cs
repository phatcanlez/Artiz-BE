using BO.DTOs;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var fromDate = now.AddMonths(-11);

        var totalUsers = await _context.Users.CountAsync();
        var totalActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
        var totalProducts = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);

        var monthly = await _context.Orders
            .Where(o => o.CreatedAt >= new DateTime(fromDate.Year, fromDate.Month, 1))
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(x => x.TotalAmount),
                OrdersCount = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalUsers = totalUsers,
            TotalActiveUsers = totalActiveUsers,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthly
        };
    }
}


