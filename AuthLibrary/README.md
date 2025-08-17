# 🔐 AuthLibr## 📋 Requirements

### Database Setup
- **Tables**: `auth_user`, `auth_token` (auto-created by stored procedures)
- **Stored Procedures**: Run `SQL/AuthStoredProcedures.sql` once (idempotent; safe to re-run)

### Dependencies  
- **.NET 8** - Target framework
- **DataAccess** - Your existing database service layer
- **Microsoft.IdentityModel.Tokens** - JWT operations
- **BCrypt.Net-Next** - Password hashing

💡 **Tip**: Keep the SQL file in source control. You don't need to delete it after running - it's designed for version control and team collaboration. Enterprise JWT Authentication

A production-ready, reusable authentication library for .NET 8 APIs. Provides complete JWT token management with BCrypt password hashing, designed for enterprise scalability and code reuse across multiple projects.

## ✨ What You Get
- 🔑 **JWT Token Management** - Generation, validation, and revocation
- 🔒 **BCrypt Password Hashing** - Secure password storage and verification  
- 👤 **User Management** - Registration, login, password changes
- 🛡️ **Security Features** - Token expiration, logout, session management
- 🏗️ **Clean Architecture** - Service interfaces for easy testing and DI
- 📊 **Database Integration** - Works with existing DataAccess layer (no duplication)
- 🔄 **Reusable Design** - Drop-in authentication for any .NET API

## Requirements
- Tables: auth_user, auth_token
- Stored procedures: run once from SQL/AuthStoredProcedures.sql (idempotent; safe to re-run)

Tip: Keep the SQL file in source control. You don’t need to delete it after running.

## 🚀 Quick Start

### 1️⃣ Add Project Reference
```xml
<ProjectReference Include="..\AuthLibrary\AuthLibrary.csproj" />
```

### 2️⃣ Configure JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKey32CharactersMinimum!",
    "Issuer": "YourApiName",
    "Audience": "YourApiName", 
    "ExpiryMinutes": 60
  }
}
```

### 3️⃣ Register Services (Program.cs)
```csharp
using AuthLibrary.Interfaces;
using AuthLibrary.Services;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Register DataAccess and AuthLibrary services
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Configure JWT services
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddScoped<ITokenService>(_ => new TokenService(
    jwtSettings["SecretKey"]!, 
    jwtSettings["Issuer"]!, 
    jwtSettings["Audience"]!, 
    int.Parse(jwtSettings["ExpiryMinutes"] ?? "60")
));
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();
```

### 4️⃣ Run Database Setup
Execute `AuthLibrary/SQL/AuthStoredProcedures.sql` in your SQL Server database once.

## 💻 Implementation Examples

### Authentication Controller
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthLibrary.Interfaces;
using AuthLibrary.DTOs;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
[ApiExplorerSettings(GroupName = "auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService) 
        => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result.ErrorMessage);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(result.ErrorMessage);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"]
            .ToString().Replace("Bearer ", "");
        var result = await _authService.LogoutAsync(token);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }
}
```

### Protecting Endpoints
```csharp
[ApiController]
[Route("api/financial")]
[Authorize] // Requires JWT token
[ApiExplorerSettings(GroupName = "financial")]
public class FinancialController : ControllerBase
{
    [HttpGet("expenses")]
    public IActionResult GetExpenses()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Your protected business logic here
        return Ok(new { message = "Protected data", userId });
    }
}
```

## 📡 API Usage Examples

### User Registration
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@company.com", 
  "password": "SecurePassword123!",
  "fullName": "John Doe"
}
```

### User Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "newuser",
  "password": "SecurePassword123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiry": "2025-08-17T10:30:00Z",
  "user": {
    "userId": 1,
    "username": "newuser",
    "email": "user@company.com"
  }
}
```

### Accessing Protected Endpoints
```http
GET /api/financial/expenses
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

## 🏗️ Architecture & Services

### Core Services
- **`IAuthService`** - High-level authentication operations (login, register, logout)
- **`ITokenService`** - JWT token generation, validation, and management
- **`IPasswordService`** - BCrypt password hashing and verification

### Data Models
- **`AuthUser`** - User entity with secure password storage
- **`AuthToken`** - JWT token tracking for revocation support
- **`LoginRequest/RegisterRequest`** - Request DTOs with validation
- **`LoginResponse`** - Response DTO with token and user info

## 🔧 Maintenance & Best Practices

### Token Cleanup (Optional)
```sql
-- Run periodically to clean expired tokens
EXEC sp_CleanExpiredTokens;
```

### Security Considerations
- ✅ **Secret Key**: Use strong 32+ character secret keys in production
- ✅ **Token Expiry**: Configure appropriate expiry times (60 minutes default)
- ✅ **HTTPS Only**: Always use HTTPS in production environments
- ✅ **Password Policy**: Enforce strong passwords in your validation layer
- ✅ **Token Revocation**: Logout properly revokes tokens from database

### Integration Notes
- 🏗️ **Modular Design**: AuthLibrary focuses only on authentication concerns
- 🔌 **Drop-in Ready**: Easy integration with existing .NET APIs
- 📊 **Database Agnostic**: Uses your existing DataAccess layer
- 🔄 **Reusable**: One library, multiple projects/APIs
- 🧪 **Testable**: Interface-based design for easy unit testing

### Production Deployment
1. **Environment Variables**: Store JWT secrets securely
2. **Database Migration**: Include SQL scripts in deployment pipeline  
3. **Health Checks**: Monitor authentication service availability
4. **Logging**: AuthService includes comprehensive error logging

---

💡 **Enterprise Ready**: Built for production use with proper error handling, security best practices, and scalable architecture.