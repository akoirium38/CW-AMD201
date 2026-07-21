using AuthService.API.Services;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AuthServiceAPIContext") ?? throw new InvalidOperationException("Connection string 'AuthServiceAPIContext' not found.");

builder.Services.AddDbContext<AuthServiceAPIContext>(options => options.UseSqlServer(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Email Service
builder.Services.AddScoped<GmailService>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
