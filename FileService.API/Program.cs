using FileService.API.Data;
using FileService.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection Configuration (EF Core SQL Server)
var connectionString = builder.Configuration.GetConnectionString("FileDbContext")
    ?? "Server=(localdb)\\mssqllocaldb;Database=FileServiceAPIContext;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<FileDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Configure CORS to explicitly allow React frontend (http://localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 4. Register Custom Services for Dependency Injection (DI)
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<ThumbnailService>();
builder.Services.AddScoped<UploadLimitService>();
builder.Services.AddScoped<FileService.API.Services.FileService>();

// 5. JWT Authentication (Matching AuthService.API key, issuer, and audience)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthService.API",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FileHub",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "A8k3oLix328Z7vN4pR6tY1wE5sD8jH0cB3gU9iK2")
            )
        };
    });

// 6. OpenAPI / Swagger Setup with Bearer Token Authorization Button
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Authorization token (e.g., Bearer eyJhbGci...)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Enable automatic EF Core database creation on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FileDbContext>();
    dbContext.Database.EnsureCreated();
}

// 7. HTTP Request Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS policy before Authentication & Authorization
app.UseCors("AllowReact");

// Enable Authentication middleware (validates JWT)
app.UseAuthentication();

// Enable Authorization middleware (checks [Authorize] attributes)
app.UseAuthorization();

app.MapControllers();

app.Run();
