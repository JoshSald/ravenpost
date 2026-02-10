using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Models;

namespace RavenPost.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Supply> Supplies => Set<Supply>();
    public DbSet<Dispatch> Dispatches => Set<Dispatch>();
    public DbSet<DispatchItem> DispatchItems => Set<DispatchItem>();
}
