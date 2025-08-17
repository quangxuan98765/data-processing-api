# 🏢 DataProcessingAPI - Enterprise Business Management System

> **Enterprise .NET 8 Web API with JWT Authentication & Power Platform Integration**

## 🚀 Overview

DataProcessingAPI is an enterprise-grade .NET 8 Web API built with Clean Architecture principles for comprehensive business data management. Features JWT authentication, reusable authentication library, and dual API architecture for enterprise scalability.

### ✨ Key Features

- 🔐 **JWT Authentication** - Secure token-based authentication with BCrypt password hashing
- 📚 **Reusable AuthLibrary** - Standalone authentication library for code reuse
- 🏗️ **Enterprise Architecture** - Separate Auth & Financial API documentation
- 💰 **Financial Management** - Secure revenue and expense tracking with authorization
- 🔄 **CRUD Operations** - Complete data management with proper security
- 📊 **Excel Integration** - Bulk import/export with Power Automate
- ✅ **Data Validation** - Multi-layer validation with business rules
- 🛡️ **Security** - JWT tokens, [Authorize] attributes, SQL injection protection
- 🔗 **Power Platform** - Custom Connectors for Power Apps and Power Automate
- 📈 **Scalable** - Modular design for easy domain extensions

## 🛠️ Technology Stack

- **.NET 8** - Latest LTS framework
- **ASP.NET Core** - Web API with dual Swagger documentation
- **SQL Server** - Database with stored procedures
- **JWT Authentication** - Secure token-based auth with BCrypt
- **AuthLibrary** - Reusable authentication components
- **Clean Architecture** - Domain-driven design pattern
- **Dependency Injection** - Built-in .NET Core DI
- **Power Platform** - Custom Connectors for enterprise integration

## 🏗️ Project Structure

```
DataProcessingAPI/
├── AuthLibrary/              # Reusable JWT + BCrypt authentication library
│   ├── Models/               # User, token models
│   ├── DTOs/                 # Request/response DTOs
│   ├── Services/             # Auth, password, token services
│   ├── Interfaces/           # Service contracts
│   └── SQL/                  # Database stored procedures
├── DataAccess/               # Database service layer
├── DataProcessingAPI/        # Main API project
│   ├── Controllers/
│   │   ├── Auth/             # Authentication endpoints
│   │   └── Financial/        # Financial management (protected)
│   ├── Application/          # Business logic
│   ├── Domain/               # Domain entities
│   └── Shared/               # Common utilities
└── publish/                  # IIS deployment artifacts
```

## 🎯 API Architecture

### 🔐 Authentication API (Auth Group)
- **POST /api/auth/login** - User authentication with JWT token
- **POST /api/auth/register** - New user registration
- **POST /api/auth/logout** - Token revocation
- **POST /api/auth/change-password** - Secure password updates

### 💰 Financial API (Financial Group) - 🔒 Protected
- **Revenue Management** - Income tracking and reporting (JWT required)
- **Expense Management** - Cost management and budgeting (JWT required)

### 🚀 Future Extensions
- **HR Management** - Employee and payroll systems
- **Inventory** - Stock and product management
- **Accounting** - Ledger and financial reporting

## 🚀 Quick Start

```bash
# Clone repository
git clone https://github.com/quangxuan98765/excel-data-processing-api

# Navigate to project
cd DataProcessingAPI

# Setup database (run SQL scripts)
# 1. Run AuthLibrary/SQL/AuthStoredProcedures.sql
# 2. Configure connection string in appsettings.json

# Configure JWT settings in appsettings.json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKey32CharactersMinimum!",
    "Issuer": "DataProcessingAPI",
    "Audience": "DataProcessingAPI",
    "ExpiryMinutes": 60
  }
}

# Run the API
dotnet run
```

**Swagger Documentation**:
- **Auth API**: `https://localhost:7xxx/swagger-auth` 
- **Financial API**: `https://localhost:7xxx/swagger-financial`

## 🔐 Authentication Flow

1. **Register/Login** → Get JWT token
2. **Include token** in Authorization header: `Bearer <token>`
3. **Access protected endpoints** (Financial APIs require authentication)

```http
# Login
POST /api/auth/login
{ "username": "admin", "password": "Password123!" }

# Use token for protected endpoints
GET /api/expense
Authorization: Bearer <your-jwt-token>
```

## 📊 API Endpoints

### Authentication Endpoints
```
POST /api/auth/login                        # User login with JWT token
POST /api/auth/register                     # New user registration  
POST /api/auth/logout                       # Token revocation
POST /api/auth/change-password              # Password updates
```

### Financial Endpoints (🔒 JWT Required)
```
POST /api/revenue/import                    # Bulk import with validation
GET  /api/revenue                           # Get revenue records
POST /api/expense/import                    # Bulk import with validation  
GET  /api/expense                           # Get expense records
```

## 🔗 Power Platform Integration

### Custom Connectors
- **Auth Connector** - Authentication operations for Power Apps login
- **Financial Connector** - Protected financial data operations

### Power Apps Integration
- **Login Flow** - JWT authentication with Custom Connector
- **Data Management** - CRUD operations with proper authorization
- **Excel Import** - Bulk data processing via Power Automate

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 📞 Contact

**Developer**: Quang Xuân  
**Email**: acongchuongheo@gmail.com  
**GitHub**: [@quangxuan98765](https://github.com/quangxuan98765)

---

⭐ **Professional .NET development with Clean Architecture principles**
