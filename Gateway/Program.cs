using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:7070")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Configuration
    .AddJsonFile(
        "ocelot.json",
        optional: false,
        reloadOnChange: true);

builder.Services.AddOcelot(
    builder.Configuration);

var app = builder.Build();

app.UseCors("AllowFrontend");

await app.UseOcelot();

app.Run();