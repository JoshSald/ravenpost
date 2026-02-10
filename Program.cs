

using System.Linq;
using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Data;
using RavenPost.Api.Models;
using RavenPost.Api.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=ravenpost.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}

app.MapGet("/", () => "Raven Post is operational. The birds are restless.");

var supplies = app.MapGroup("/supplies");

supplies.MapGet("/", async (
    string? q,
    string? category,
    decimal? minPrice,
    decimal? maxPrice,
    AppDbContext db) =>
{
    var query = db.Supplies.AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
        query = query.Where(s => s.Name.ToLower().Contains(q.ToLower()));

    if (!string.IsNullOrWhiteSpace(category))
        query = query.Where(s => s.Category == category);

    if (minPrice.HasValue)
        query = query.Where(s => s.Price >= minPrice.Value);

    if (maxPrice.HasValue)
        query = query.Where(s => s.Price <= maxPrice.Value);

    return await query.AsNoTracking().ToListAsync();
});

supplies.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var supply = await db.Supplies.FindAsync(id);
    return supply is null ? Results.NotFound() : Results.Ok(supply);
});

supplies.MapPost("/", async (CreateSupplyDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        return Results.BadRequest("Name is required");

    if (string.IsNullOrWhiteSpace(dto.Category))
        return Results.BadRequest("Category is required");

    if (dto.Price <= 0)
        return Results.BadRequest("Price must be greater than 0");

    var supply = new Supply
    {
        Name = dto.Name,
        Category = dto.Category,
        Price = dto.Price
    };

    db.Supplies.Add(supply);
    await db.SaveChangesAsync();

    return Results.Created($"/supplies/{supply.Id}", supply);
});

supplies.MapPut("/{id:int}", async (int id, Supply input, AppDbContext db) =>
{
    var supply = await db.Supplies.FindAsync(id);
    if (supply is null) return Results.NotFound();

    supply.Name = input.Name;
    supply.Category = input.Category;
    supply.Price = input.Price;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

supplies.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var supply = await db.Supplies.FindAsync(id);
    if (supply is null) return Results.NotFound();

    db.Supplies.Remove(supply);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

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

app.Run();

record DispatchRequest(List<DispatchLine> Items);
record DispatchLine(int SupplyId, int Quantity);