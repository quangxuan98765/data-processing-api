using DataProcessingAPI.Application.Interfaces.Financial;
using DataProcessingAPI.Application.Services.Financial;
using DataProcessingAPI.Application.Interfaces;
using DataProcessingAPI.Application.Services;
using DataAccess;
using AuthLibrary.Interfaces;
using AuthLibrary.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("financial", new OpenApiInfo 
    { 
        Title = "Financial Data API", 
        Version = "v1",
        Description = "APIs for managing financial data (expenses and revenue)"
    });
    c.SwaggerDoc("auth", new OpenApiInfo 
    { 
        Title = "Authentication API", 
        Version = "v1",
        Description = "APIs for user authentication and authorization"
    });
    c.SwaggerDoc("speedtest", new OpenApiInfo 
    { 
        Title = "Speed Test API", 
        Version = "v1",
        Description = "APIs for managing ICT speed test results"
    });
});

// 🔧 DATABASE SERVICE
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<IDatabaseService>(provider => 
    new DatabaseService(connectionString));

// 🏢 BUSINESS SERVICES - CẢ THU VÀ CHI
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// 🚀 SPEED TEST SERVICE
builder.Services.AddScoped<ISpeedTestService, SpeedTestService>();

// 🔐 AUTHENTICATION SERVICES
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["SecretKey"] ?? "DataProcessingAPI_SecretKey_32Characters!";
var issuer = jwtSettings["Issuer"] ?? "DataProcessingAPI";
var audience = jwtSettings["Audience"] ?? "DataProcessingAPI";
var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService>(provider => 
    new AuthLibrary.Services.TokenService(secretKey, issuer, audience, expiryMinutes));
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/financial/swagger.json", "Financial API v1");
        options.SwaggerEndpoint("/swagger/auth/swagger.json", "Auth API v1");
        options.SwaggerEndpoint("/swagger/speedtest/swagger.json", "Speed Test API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
