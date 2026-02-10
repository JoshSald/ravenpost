using RavenPost.Api.Models;

namespace RavenPost.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Supplies.Any())
            return;

        var supplies = new List<Supply>
        {
            new() { Name = "Black Ink Scroll", Category = "Scrolls", Price = 5 },
            new() { Name = "Cipher Sheet", Category = "Scrolls", Price = 8 },
            new() { Name = "Wax Seal Kit", Category = "Seals", Price = 3 },
            new() { Name = "Royal Parchment", Category = "Scrolls", Price = 12 },
            new() { Name = "Night Raven", Category = "Birds", Price = 25 },
            new() { Name = "War Raven", Category = "Birds", Price = 40 },
            new() { Name = "Dispatch Tube", Category = "Containers", Price = 6 },
            new() { Name = "Maester Satchel", Category = "Containers", Price = 15 }
        };

        db.Supplies.AddRange(supplies);
        db.SaveChanges();
    }
}