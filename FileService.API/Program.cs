using FileService.API.Data;
using FileService.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. MongoDB Atlas Database Configuration
// Reads the connection string from appsettings.json > MongoDB:ConnectionString
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"]
    ?? throw new InvalidOperationException("MongoDB ConnectionString is not configured.");

// Register MongoDB client as a singleton (one shared connection pool for the app)
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

// Register FileDbContext as scoped (created once per HTTP request)
builder.Services.AddScoped<FileDbContext>();

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Configure CORS to allow requests from:
//    - React Frontend (http://localhost:5173 original, http://localhost:7070 new port)
//    - API Gateway (https://localhost:7000) which proxies all frontend requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",   // Original direct frontend port
                "http://localhost:7070",   // New Vite frontend port
                "https://localhost:7000"   // Ocelot API Gateway
              )
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

// NOTE: No database setup needed here.
// MongoDB Atlas creates the "files" collection automatically on the first InsertOneAsync call.

// 7. HTTP Request Pipeline Configuration

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// Enable CORS policy before Authentication & Authorization
app.UseCors("AllowReact");

// Enable Authentication middleware (validates JWT)
app.UseAuthentication();

// Enable Authorization middleware (checks [Authorize] attributes)
app.UseAuthorization();

app.MapControllers();

app.Run();
