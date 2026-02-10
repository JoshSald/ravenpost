using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Data;
using RavenPost.Api.Models;

namespace RavenPost.Api.Endpoints;

public static class DispatchEndpoints
{
    public static void MapDispatchEndpoints(this WebApplication app)
    {
        var dispatches = app.MapGroup("/dispatches");

        dispatches.MapPost("/", async (DispatchRequest request, AppDbContext db) =>
        {
            if (request.Items is null || request.Items.Count == 0)
                return Results.BadRequest("Dispatch must contain at least one item");

            var dispatch = new Dispatch
            {
                CreatedAt = DateTime.UtcNow
            };

            decimal total = 0;

            foreach (var item in request.Items)
            {
                var supply = await db.Supplies.FindAsync(item.SupplyId);
                if (supply is null)
                    return Results.BadRequest($"Supply {item.SupplyId} not found");

                var line = supply.Price * item.Quantity;

                dispatch.Items.Add(new DispatchItem
                {
                    SupplyId = supply.Id,
                    Quantity = item.Quantity,
                    LineCost = line
                });

                total += line;
            }

            dispatch.TotalCost = total;

            db.Dispatches.Add(dispatch);
            await db.SaveChangesAsync();

            return Results.Created($"/dispatches/{dispatch.Id}", dispatch);
        });

        dispatches.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Dispatches
                .AsNoTracking()
                .Select(d => new
                {
                    d.Id,
                    d.CreatedAt,
                    d.TotalCost,
                    ItemCount = d.Items.Count
                })
                .ToListAsync();
        });

        dispatches.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var result = await db.Dispatches
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.CreatedAt,
                    d.TotalCost,
                    Items = d.Items.Select(i => new
                    {
                        i.Id,
                        i.Quantity,
                        i.LineCost,
                        Supply = new
                        {
                            i.Supply.Id,
                            i.Supply.Name,
                            i.Supply.Category,
                            i.Supply.Price
                        }
                    })
                })
                .FirstOrDefaultAsync();

            return result is null ? Results.NotFound() : Results.Ok(result);
        });
    }

    record DispatchRequest(List<DispatchLine> Items);
    record DispatchLine(int SupplyId, int Quantity);
}
