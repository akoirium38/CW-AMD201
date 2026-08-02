using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = new[]
{
    "http://localhost:7070",              // Local frontend
    "https://cw-amd201.onrender.com"     // render frontedn
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

string ocelotConfig = "ocelot.json";

if (builder.Environment.IsEnvironment("Docker"))
{
    ocelotConfig = "ocelot.Docker.json";
}
else if (builder.Environment.IsProduction())
{
    ocelotConfig = "ocelot.Render.json";
}

builder.Configuration
    .AddJsonFile(
        ocelotConfig,
        optional: false,
        reloadOnChange: false);

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowFrontend");

await app.UseOcelot();

app.Run();