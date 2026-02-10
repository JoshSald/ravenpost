using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Data;

namespace RavenPost.Api.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this WebApplication app)
    {
        var reports = app.MapGroup("/reports");

        reports.MapGet("/daily", async (string? date, AppDbContext db) =>
        {
            DateTime target;

            if (string.IsNullOrWhiteSpace(date) || date.ToLower() == "today")
                target = DateTime.UtcNow.Date;
            else if (!DateTime.TryParse(date, out target))
                return Results.BadRequest("Use YYYY-MM-DD or 'today'");

            var start = target.Date;
            var end = start.AddDays(1);

            var dailyDispatches = db.Dispatches
                .Where(d => d.CreatedAt >= start && d.CreatedAt < end);

            var orderCount = await dailyDispatches.CountAsync();
            var revenue = await dailyDispatches.SumAsync(d => (decimal?)d.TotalCost) ?? 0;

            var topSupplies = await db.DispatchItems
                .Where(i => i.Dispatch.CreatedAt >= start && i.Dispatch.CreatedAt < end)
                .GroupBy(i => new { i.SupplyId, i.Supply.Name })
                .Select(g => new
                {
                    g.Key.SupplyId,
                    g.Key.Name,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(3)
                .ToListAsync();

            return Results.Ok(new
            {
                Date = start,
                DispatchCount = orderCount,
                TotalRevenue = revenue,
                TopSupplies = topSupplies
            });
        });
    }
}
