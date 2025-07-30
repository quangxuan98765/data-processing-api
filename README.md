# 📊 Excel Data API

> **ASP.NET Core Web API for Excel Data Processing & Bulk Database Operations**

## 🚀 Overview

Excel Data API is a robust .NET 8 Web API designed to handle bulk data import from Excel files through Power Automate integration. The API provides comprehensive data validation, error handling, and efficient database operations.

### ✨ Key Features

- 🔄 **Bulk Data Insert**: Efficiently process and insert large datasets from Excel files
- ✅ **Data Validation**: Comprehensive validation with detailed error reporting
- 🔗 **Power Automate Integration**: Seamless integration with Microsoft Power Automate flows
- 📋 **Detailed Logging**: Complete audit trail with user tracking
- 🛡️ **Error Handling**: Robust error handling with user-friendly responses
- 📊 **SQL Server Integration**: Optimized database operations with connection pooling

## 🏗️ Architecture

```
ExcelDataAPI/
├── Controllers/          # API Controllers
│   └── BulkInsertController.cs
├── Models/              # Data Models & DTOs
│   ├── BulkInsertRequest.cs
│   ├── BulkInsertResponse.cs
│   ├── ExcelInputRow.cs
│   ├── ExcelOutputRow.cs
│   ├── FinancialDataRow.cs
│   └── ValidationResult.cs
├── Services/            # Business Logic
│   ├── DataService.cs
│   └── ValidationService.cs
└── Program.cs          # Application Entry Point
```

## 🛠️ Technology Stack

- **Framework**: .NET 8.0
- **Web Framework**: ASP.NET Core
- **Database**: SQL Server
- **Data Access**: Microsoft.Data.SqlClient
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture with Service Layer

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 / VS Code (optional)
- Power Automate (for integration)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/quangxuan98765/excel-data-processing-api
cd ExcelDataAPI-Project
```

### 2. Configure Database Connection
Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourDatabase;Trusted_Connection=true;"
  }
}
```

### 3. Build and Run
```bash
cd ExcelDataAPI
dotnet restore
dotnet build
dotnet run
```

### 4. Access API Documentation
- **Swagger UI**: `https://localhost:7xxx/swagger`
- **API Base URL**: `https://localhost:7xxx/api`

## 🔗 API Endpoints

### POST `/api/BulkInsert/financial-data`

Bulk insert financial data with validation.

**Request Body:**
```json
{
  "data": [
    {
      "soTaiKhoan": "1234567890",
      "tenTaiKhoan": "Nguyen Van A",
      "soTien": 1000000,
      "ngayGiaoDich": "2024-01-15",
      "moTa": "Thu nhập tháng 1"
    }
  ],
  "idNguoiDung": "USER001",
  "nguoiNhap": "Admin User"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Xử lý thành công 1/1 bản ghi",
  "totalRows": 1,
  "successRows": 1,
  "errorRows": 0,
  "errors": [],
  "processedData": [...]
}
```

## 🔧 Power Automate Integration

The API is designed to work seamlessly with Power Automate flows:

1. **Excel Online Connector**: Read data from Excel files
2. **HTTP Connector**: Send data to API endpoint
3. **Data Transformation**: Format data according to API schema
4. **Error Handling**: Process API responses and handle errors

## 🧪 Testing

### Using Swagger UI
1. Navigate to `/swagger`
2. Expand the `POST /api/BulkInsert/financial-data` endpoint
3. Click "Try it out"
4. Enter sample data and execute

### Using PowerShell (Example)
```powershell
$body = @{
    data = @(
        @{
            soTaiKhoan = "1234567890"
            tenTaiKhoan = "Test Account"
            soTien = 1000000
            ngayGiaoDich = "2024-01-15"
            moTa = "Test transaction"
        }
    )
    idNguoiDung = "TEST_USER"
    nguoiNhap = "Test Admin"
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "https://localhost:7xxx/api/BulkInsert/financial-data" -Method POST -Body $body -ContentType "application/json"
```

## 📊 Database Schema

### FinancialData Table
```sql
CREATE TABLE FinancialData (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    SoTaiKhoan NVARCHAR(50) NOT NULL,
    TenTaiKhoan NVARCHAR(255) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    NgayGiaoDich DATE NOT NULL,
    MoTa NVARCHAR(500),
    IDNguoiDung NVARCHAR(100) NOT NULL,
    NguoiNhap NVARCHAR(255) NOT NULL,
    NgayTao DATETIME2 DEFAULT GETDATE(),
    NgayCapNhat DATETIME2 DEFAULT GETDATE()
);
```

## 🛡️ Security Features

- Input validation and sanitization
- SQL injection prevention
- Data type validation
- Business rule validation
- Comprehensive error logging

## 🔄 Future Enhancements

- [ ] Authentication & Authorization (JWT)
- [ ] File upload endpoint for direct Excel processing
- [ ] Data export functionality
- [ ] Real-time processing status
- [ ] Batch processing queues
- [ ] Advanced reporting features

## 📝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Contact

**Developer**: [Quang](#)  
**Email**: [acongchuongheo@gmail.com](mailto:acongchuongheo@gmail.com)  
**LinkedIn**: [Quang Xuân](https://www.linkedin.com/in/quang-xuan-669491349/)  
**GitHub**: [@quangxuan98765](https://github.com/quangxuan98765)

---

⭐ **Star this repository if you find it helpful!**

> This project demonstrates professional .NET development practices, clean architecture, and real-world API design patterns. Perfect for learning, portfolio showcasing, and practical business applications.
