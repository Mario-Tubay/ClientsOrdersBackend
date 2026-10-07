using System.Globalization;
using ClientOrderService.API.Data;
using ClientOrderService.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientOrderService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ClientOrderDbContext _context;

    public DashboardController(ClientOrderDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var completedOrders = await _context.Orders.CountAsync(o => o.Status == "Completed");
        var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
        var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled");
        var activeClients = await _context.Clients.CountAsync(c => c.IsActive);

        var completionRate = totalOrders > 0
            ? (int)Math.Round((double)completedOrders / totalOrders * 100)
            : 0;

        var totalRevenue = await _context.Orders
            .Where(o => o.Status != "Cancelled")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        var recentOrders = await _context.Orders
            .Include(o => o.Client)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new RecentOrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                ClientName = o.Client != null ? o.Client.FullName : "Sin Asignar",
                Date = o.OrderDate.ToString("dd MMM yyyy", new CultureInfo("es-ES")),
                Status = o.Status,
                Total = o.TotalAmount
            })
            .ToListAsync();

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var startOfPeriod = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

        var ordersInPeriod = await _context.Orders
            .Where(o => o.OrderDate >= startOfPeriod)
            .Select(o => new { o.OrderDate })
            .ToListAsync();

        var monthlyActivity = new List<MonthlyActivityDto>();
        int maxMonthlyOrders = 1;

        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = DateTime.UtcNow.AddMonths(-i);
            var count = ordersInPeriod.Count(o => o.OrderDate.Year == targetMonth.Year && o.OrderDate.Month == targetMonth.Month);
            if (count > maxMonthlyOrders)
            {
                maxMonthlyOrders = count;
            }

            var monthName = targetMonth.ToString("MMMM", new CultureInfo("es-ES"));
            monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

            monthlyActivity.Add(new MonthlyActivityDto
            {
                Month = monthName,
                Orders = count,
                Percentage = 0
            });
        }

        foreach (var item in monthlyActivity)
        {
            item.Percentage = maxMonthlyOrders > 0
                ? (int)Math.Round((double)item.Orders / maxMonthlyOrders * 100)
                : 0;
            if (item.Orders > 0 && item.Percentage == 0)
            {
                item.Percentage = 15;
            }
        }

        return Ok(new DashboardStatsDto
        {
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            ActiveClients = activeClients,
            CompletionRate = completionRate,
            TotalRevenue = totalRevenue,
            RecentOrders = recentOrders,
            MonthlyActivity = monthlyActivity
        });
    }
}
