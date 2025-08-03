# 🏢 ExcelDataAPI - Enterprise Data Management System

> **Clean Architecture .NET 8 Web API for Business Data Management**

## 🚀 Overview

ExcelDataAPI is a scalable .NET 8 Web API built with Clean Architecture principles for comprehensive business data management. Currently supports Financial Management (Revenue/Expense) with extensibility for HR, Inventory, and other business domains.

### ✨ Key Features

- 🏗️ **Clean Architecture** - Domain-driven design with separation of concerns
- 💰 **Financial Management** - Revenue and expense tracking with bulk operations
- 🔄 **CRUD Operations** - Complete data management functionality
- 📊 **Excel Integration** - Bulk import/export with Power Automate
- ✅ **Data Validation** - Multi-layer validation with business rules
- 🛡️ **Security** - Parameterized queries, SQL injection protection
- 🔗 **Power Platform** - Seamless Power Apps and Power Automate integration
- 📈 **Scalable** - Easy to extend with new business domains

## 🛠️ Technology Stack

- **.NET 8** - Latest LTS framework
- **ASP.NET Core** - Web API with Swagger documentation
- **SQL Server** - Database with bulk operations support
- **Clean Architecture** - Domain-driven design pattern
- **Dependency Injection** - Built-in .NET Core DI

## 🎯 Current Domains

### 💰 Financial Management
- **Revenue API** - Income tracking and reporting
- **Expense API** - Cost management and budgeting

### 🚀 Future Extensions
- **HR Management** - Employee and payroll systems
- **Inventory** - Stock and product management
- **Accounting** - Ledger and financial reporting

## 🚀 Quick Start

```bash
# Clone repository
git clone https://github.com/quangxuan98765/excel-data-processing-api

# Navigate to project
cd ExcelDataAPI

# Configure database connection in appsettings.json
# Run the API
dotnet run
```

**Swagger UI**: `https://localhost:7xxx/swagger`

## 📊 API Endpoints

### Revenue Management
```
POST /api/revenue/import                    # Bulk import with built-in validation
```

### Expense Management
```
POST /api/expense/import                    # Bulk import with built-in validation
```

## 🔗 Integration

- **Power Automate** - Excel file processing workflows
- **Power Apps** - Form-based data entry
- **SharePoint** - Document storage and management

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 📞 Contact

**Developer**: Quang Xuân  
**Email**: acongchuongheo@gmail.com  
**GitHub**: [@quangxuan98765](https://github.com/quangxuan98765)

---

⭐ **Professional .NET development with Clean Architecture principles**
