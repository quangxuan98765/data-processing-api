# AuthLibrary - Reusable Authentication Library

## Overview
AuthLibrary là một thư viện authentication tái sử dụng được xây dựng cho .NET 8, sử dụng JWT tokens và BCrypt password hashing. Thư viện này được thiết kế để dễ dàng tích hợp vào bất kỳ ASP.NET Core API nào.

## Features
- ✅ JWT Token Authentication
- ✅ BCrypt Password Hashing 
- ✅ User Registration & Login
- ✅ Password Change
- ✅ Token Validation
- ✅ Automatic Token Cleanup
- ✅ Middleware Integration
- ✅ Dependency Injection Support

## Database Schema Required

### auth_user Table
```sql
CREATE TABLE auth_user (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(150) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    email NVARCHAR(254) NOT NULL UNIQUE,
    first_name NVARCHAR(150),
    last_name NVARCHAR(150),
    is_staff BIT NOT NULL DEFAULT 0,
    is_superuser BIT NOT NULL DEFAULT 0,
    is_active BIT NOT NULL DEFAULT 1,
    date_joined DATETIME2 NOT NULL,
    last_login DATETIME2
);
```

### auth_token Table
```sql
CREATE TABLE auth_token (
    id INT IDENTITY(1,1) PRIMARY KEY,
    token_key NVARCHAR(MAX) NOT NULL,
    idUser INT NOT NULL,
    expire_date DATETIME2 NOT NULL,
    created_date DATETIME2 NOT NULL,
    FOREIGN KEY (idUser) REFERENCES auth_user(id)
);
```

## Installation Steps

### 1. Add Project Reference
```xml
<ProjectReference Include="../AuthLibrary/AuthLibrary.csproj" />
```

### 2. Configure appsettings.json
```json
{
  "JWT": {
    "SecretKey": "YourSuperSecretKeyForJWTTokensMinimum32Characters!",
    "Issuer": "YourAPIName",
    "Audience": "YourAPIName", 
    "ExpiryMinutes": "60"
  }
}
```

### 3. Configure Program.cs
```csharp
using AuthLibrary.Extensions;
using AuthLibrary.Middleware;
using DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DataAccess (required dependency)
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Add AuthLibrary
builder.Services.AddAuthLibrary(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseJwtAuthentication(); // Custom middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### 4. Copy AuthController
Copy `AuthLibrary/Controllers/AuthController.cs` to your API project and update the namespace:

```csharp
namespace YourAPI.Controllers;
```

### 5. Run SQL Scripts
Execute the stored procedures from `AuthLibrary/SQL/AuthStoredProcedures.sql` in your database.

## Usage Examples

### User Registration
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "password": "SecurePassword123!",
  "email": "test@example.com", 
  "firstName": "Test",
  "lastName": "User"
}
```

### User Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-20T15:30:00Z",
  "user": {
    "id": 1,
    "username": "testuser",
    "email": "test@example.com",
    "firstName": "Test", 
    "lastName": "User",
    "isStaff": false,
    "isSuperuser": false
  }
}
```

### Protected Endpoint
```csharp
[HttpGet("protected")]
[Authorize]
public IActionResult GetProtectedData()
{
    // Get user info from JWT claims
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = User.FindFirst(ClaimTypes.Name)?.Value;
    
    return Ok(new { userId, username, message = "Protected data" });
}
```

### Change Password
```http
POST /api/auth/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!"
}
```

## Service Interface Usage

### Inject Services Directly
```csharp
public class MyController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    
    public MyController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }
    
    public async Task<IActionResult> CustomAuth()
    {
        var result = await _authService.LoginAsync(new LoginRequest 
        { 
            Username = "user", 
            Password = "pass" 
        });
        
        return Ok(result);
    }
}
```

## Configuration Options

### Custom JWT Settings
```csharp
// In Program.cs - Alternative configuration
services.AddAuthLibrary(
    secretKey: "YourCustomSecretKey32Characters!",
    issuer: "YourIssuer", 
    audience: "YourAudience",
    expiryMinutes: 120
);
```

### Middleware Configuration
The JWT middleware automatically:
- Skips authentication for public paths (`/api/auth/*`, `/swagger`, `/health`)
- Extracts tokens from `Authorization: Bearer {token}` header
- Validates tokens and adds user info to `HttpContext.Items`
- Returns 401 for invalid/missing tokens

## Security Features

### Password Security
- BCrypt hashing with automatic salt generation
- Configurable work factor (default: 12)
- Secure password verification

### Token Security  
- JWT with HMAC-SHA256 signing
- Configurable expiration
- Token invalidation on logout
- Automatic cleanup of expired tokens

### Database Security
- Parameterized queries prevent SQL injection
- Stored procedures for all database operations
- User input validation

## Error Handling

All services return `AuthResult` objects:
```csharp
public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public object Data { get; set; }
    
    public static AuthResult Success(object data = null);
    public static AuthResult Failed(string message);
}
```

## Testing

### Unit Test Example
```csharp
[Test]
public async Task Login_ValidCredentials_ReturnsSuccess()
{
    // Arrange
    var authService = new AuthService(mockDb, mockPassword, mockToken);
    var request = new LoginRequest { Username = "test", Password = "pass" };
    
    // Act
    var result = await authService.LoginAsync(request);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.IsNotNull(result.Data);
}
```

## Troubleshooting

### Common Issues

1. **"Sequence contains no elements"**
   - Check database connection string
   - Verify stored procedures exist
   - Ensure auth tables are created

2. **"Invalid token"**
   - Verify JWT secret key matches in configuration
   - Check token hasn't expired
   - Ensure token format is correct

3. **"User not found"**
   - Verify user exists in auth_user table
   - Check is_active = 1
   - Confirm username is correct

### Debug Mode
Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "AuthLibrary": "Debug",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

## License
This library is for internal company use. Customize and extend as needed for your projects.

## Support
For issues or questions, contact the development team or check the source code in `AuthLibrary/` folder.
