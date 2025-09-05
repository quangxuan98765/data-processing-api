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
- **GET /api/auth/profile** - Get user profile

### 💰 Financial API (Financial Group) - 🔒 JWT Protected

#### Revenue Management
- **GET /api/revenue** - Get all revenue records
- **GET /api/revenue/{id}** - Get specific revenue by ID
- **POST /api/revenue** - Create new revenue record
- **PUT /api/revenue/{id}** - Update existing revenue
- **DELETE /api/revenue/{id}** - Delete revenue record
- **POST /api/revenue/bulk-import** - Excel bulk import with validation

#### Expense Management  
- **GET /api/expense** - Get all expense records
- **GET /api/expense/{id}** - Get specific expense by ID
- **POST /api/expense** - Create new expense record
- **PUT /api/expense/{id}** - Update existing expense
- **DELETE /api/expense/{id}** - Delete expense record
- **POST /api/expense/bulk-import** - Excel bulk import with validation

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

## 📊 Enterprise DTOs & Clean Architecture

### Request/Response Pattern
```csharp
// API Request DTOs (Client → Server)
CreateRevenueRequest    // POST /api/revenue
UpdateRevenueRequest    // PUT /api/revenue/{id}
CreateExpenseRequest    // POST /api/expense  
UpdateExpenseRequest    // PUT /api/expense/{id}

// API Response DTOs (Server → Client)
RevenueResponse        // GET operations
ExpenseResponse        // GET operations

// Import DTOs (Excel → API)
RevenueImportDto       // Bulk import from Excel
ExpenseImportDto       // Bulk import from Excel
```

### Benefits
- ✅ **Security**: Request DTOs prevent ID/timestamp manipulation
- ✅ **Validation**: Separate validation rules for create vs update
- ✅ **Documentation**: Clear API contracts in Swagger
- ✅ **Maintainability**: Easy to extend without breaking changes

## 📊 API Endpoints

### Authentication Endpoints
```
POST /api/auth/login                        # User login with JWT token
POST /api/auth/register                     # New user registration  
POST /api/auth/logout                       # Token revocation
POST /api/auth/change-password              # Password updates
GET  /api/auth/profile                      # Get user profile
```

### Financial Endpoints (🔒 JWT Required)
```
# Revenue Management
GET    /api/revenue                         # Get all revenue records
GET    /api/revenue/{id}                    # Get revenue by ID
POST   /api/revenue                         # Create new revenue
PUT    /api/revenue/{id}                    # Update revenue
DELETE /api/revenue/{id}                    # Delete revenue
POST   /api/revenue/bulk-import             # Excel bulk import

# Expense Management  
GET    /api/expense                         # Get all expense records
GET    /api/expense/{id}                    # Get expense by ID
POST   /api/expense                         # Create new expense
PUT    /api/expense/{id}                    # Update expense
DELETE /api/expense/{id}                    # Delete expense
POST   /api/expense/bulk-import             # Excel bulk import
```

## 🔗 Power Platform Integration

### Custom Connectors
- **Financial Data API Connector** - Complete CRUD operations with JWT authentication
- **Swagger-based Definition** - Auto-generated from API documentation
- **Request/Response DTOs** - Clean API contracts for Power Platform

### Power Apps Integration
- **JWT Authentication Flow** - Secure login with token management
- **Financial Data Management** - Full CRUD operations with proper authorization
- **Form Validation** - Client-side + server-side validation
- **Data Binding** - Clean Response DTOs for easy Power Apps binding

### Power Automate Flows
- **Excel Data Import** - Bulk processing with validation and error handling
- **Automated Workflows** - Scheduled data processing and reporting
- **Error Handling** - Comprehensive error logging and retry mechanisms

### Integration Benefits
- ✅ **Type Safety** - Strongly typed Request/Response DTOs
- ✅ **Error Handling** - Structured error responses
- ✅ **Security** - JWT authentication for all protected endpoints  
- ✅ **Scalability** - Enterprise-grade API design patterns

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 📞 Contact

**Developer**: Quang Xuân  
**Email**: acongchuongheo@gmail.com  
**GitHub**: [@quangxuan98765](https://github.com/quangxuan98765)

---

⭐ **Professional .NET development with Clean Architecture principles**
