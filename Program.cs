using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Data;

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
{
    return await db.Supplies.AsNoTracking().ToListAsync();
});

app.Run();