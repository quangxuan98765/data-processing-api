# 🔐 AuthLibr## 📋 Quick Setup

### Dependencies
- **.NET 8** + your existing DataAccess layer
- **SQL Server** with `auth_user`, `auth_token` tables
- Run `SQL/AuthStoredProcedures.sql` once (includes `sp_GetTokenByUserId` for Power Platform)

### Configuration
1. Add project reference to AuthLibrary
2. Configure JWT settings in `appsettings.json` 
3. Register services in `Program.cs` (see examples in project)
4. Add `[Authorize]` to protected endpoints

⚠️ **Important**: Ensure `auth_token.token_key` is `NVARCHAR(1000)` - JWT tokens are 400-600 characters! Enterprise JWT Authentication

# 🔐 AuthLibrary - Enterprise JWT Authentication

Production-ready JWT authentication for .NET 8 APIs with Power Platform integration. Drop-in solution for secure multi-user applications.

**✨ Power Apps Ready** - Dynamic JWT authentication without exposing tokens to end users.

## ✨ Features
- 🔑 **JWT Management** - Generate, validate, revoke tokens
- 🔒 **BCrypt Security** - Industry-standard password hashing  
- 👤 **User System** - Registration, login, session management
- ⚡ **Power Platform Ready** - Custom Connector integration
- 🔐 **Dynamic Tokens** - SQL-based token retrieval for multi-user apps
- 🏗️ **Clean Architecture** - Service interfaces, dependency injection ready

## Requirements
- Tables: auth_user, auth_token
- Stored procedures: run once from SQL/AuthStoredProcedures.sql (idempotent; safe to re-run)

Tip: Keep the SQL file in source control. You don’t need to delete it after running.

## 🚀 Standard API Usage

### Endpoints Available
- `POST /api/auth/login` - Returns JWT + user info
- `POST /api/auth/register` - Create new user
- `POST /api/auth/logout` - Revoke token

### Protected Routes
Add `[Authorize]` to any controller/action. JWT validation automatic.

### Client Integration
Include `Authorization: Bearer {token}` header in requests.

## 🔄 Power Platform Integration

**The Problem**: Power Apps users need secure API access, but JWT tokens shouldn't be exposed to end users.

**The Solution**: Dynamic token retrieval via Power Automate flows.

### Architecture Flow
```
Power Apps (userId only) → Power Automate Flow → Custom Connector → C# API
                            ↓                        ↓           ↓
                    sp_GetTokenByUserId        X-Authorization  [Authorize]
                            ↓                        ↓      validates JWT
                      Fresh Token            Policy Transform
```

### Setup Process

#### 1. **Login Flow** (One-time per user)
- Power Apps → Flow → API login → Store token in SQL → Return user info only

#### 2. **CRUD Flow** (Dynamic token per request)  
- Power Apps → Flow → SQL gets token → Custom Connector → API with JWT

#### 3. **Custom Connector Configuration**
- Use `X-Authorization` header parameter (Power Automate restriction)
- Policy transforms `X-Authorization` → `Authorization`
- No `securityDefinitions` in OpenAPI spec

#### 4. **Policy Setup**
- **Header name**: `Authorization`
- **Header value**: `@headers('X-Authorization')`
- **Action**: `override`

### Key Benefits
- ✅ **Zero Token Exposure** - Users never see JWTs
- ✅ **Enterprise Security** - Individual tokens per user
- ✅ **Fresh Tokens** - Retrieved dynamically per request
- ✅ **Standard Validation** - C# API `[Authorize]` works normally

## 🏗️ Technical Overview

### Core Components
- **IAuthService** - Login, register, logout operations
- **ITokenService** - JWT generation and validation
- **IPasswordService** - BCrypt hashing

### Database Layer
- **auth_user** - User accounts with secure password storage
- **auth_token** - JWT tokens with expiry tracking
- **Stored Procedures** - All operations use SPs for performance and security

### Integration Pattern
- Works with existing DataAccess layer (no duplication)
- Interface-based design for testing and DI
- Reusable across multiple projects

## 🔧 Maintenance

### Routine Tasks
```sql
-- Clean expired tokens periodically
EXEC sp_CleanExpiredTokens;
```

### Troubleshooting
- **401 Unauthorized**: Check token expiry, JWT secret key, or column size
- **Token Truncated**: Ensure `token_key` column is NVARCHAR(1000)+
- **Power Automate Policy**: Verify `X-Authorization` → `Authorization` transform

### Security Best Practices
- ✅ **Strong JWT Secrets** - 32+ character keys in production
- ✅ **HTTPS Only** - Never transmit tokens over HTTP
- ✅ **Token Expiry** - Configure appropriate timeouts (60 min default)
- ✅ **Token Revocation** - Logout properly invalidates tokens
- ✅ **Password Policy** - Enforce strong passwords in validation

### Deployment
- Store JWT secrets in environment variables/secure config
- Include SQL scripts in deployment pipeline
- Test Custom Connector policies in dev environment first

---

💡 **Enterprise Authentication**: Complete JWT solution with Power Platform integration for secure multi-user applications.