using System.Linq;
using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Data;
using RavenPost.Api.Models;
using RavenPost.Api.Dtos;

namespace RavenPost.Api.Endpoints;

public static class SuppliesEndpoints
{
    public static void MapSupplyEndpoints(this WebApplication app)
    {
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
    }
}
