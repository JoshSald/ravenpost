using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Data;
using RavenPost.Api.Models;

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

supplies.MapGet("/", async (AppDbContext db) =>
    await db.Supplies.AsNoTracking().ToListAsync()
);

supplies.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var supply = await db.Supplies.FindAsync(id);
    return supply is null ? Results.NotFound() : Results.Ok(supply);
});

supplies.MapPost("/", async (Supply supply, AppDbContext db) =>
{
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

app.Run();