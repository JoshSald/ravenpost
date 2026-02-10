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

app.MapGet("/", () => "Raven Post is operational. The birds are restless.");

app.Run();
