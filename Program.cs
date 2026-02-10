using System.Linq;
using Microsoft.EntityFrameworkCore;
using RavenPost.Api.Data;
using Scalar.AspNetCore;
using RavenPost.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=ravenpost.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}

app.MapGet("/", () => "Raven Post is operational. The birds are restless.");

app.MapSupplyEndpoints();
app.MapDispatchEndpoints();
app.MapReportEndpoints();

app.Run();
