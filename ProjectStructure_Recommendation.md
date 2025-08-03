# ExcelDataAPI - Clean Architecture Documentation

> **Mục đích**: Hệ thống API quản lý dữ liệu doanh nghiệp với Clean Architecture  
> **Hiện tại**: Chỉ có 2 API import Excel cho Thu và Chi  
> **Tương lai**: Mở rộng thêm nhiều domain khác (HR, Kế toán, Kho...)

---

## 🏗️ **CẤU TRÚC PROJECT HIỆN TẠI**

```
ExcelDataAPI/
├── 📁 Controllers/
│   └── Financial/                          # API Endpoints
│       ├── RevenueController.cs            # POST /api/revenue/import
│       └── ExpenseController.cs            # POST /api/expense/import
│
├── 📁 Application/                         # Business Logic Layer
│   ├── Interfaces/Financial/               # Contracts (Interface)
│   │   ├── IRevenueService.cs             # Revenue service contract
│   │   └── IExpenseService.cs             # Expense service contract
│   ├── Services/Financial/                 # Business Logic Implementation
│   │   ├── RevenueService.cs              # Revenue business logic (270+ lines)
│   │   └── ExpenseService.cs              # Expense business logic (274+ lines)
│   └── DTOs/                              # Data Transfer Objects
│       ├── ImportDTOs.cs                  # Request objects
│       └── ResultDTOs.cs                  # Response objects
│
├── 📁 Infrastructure/                      # External Dependencies
│   ├── Data/                              # Database Access
│   │   ├── IDataProvider.cs               # Data access contract (6 methods)
│   │   └── SqlDataProvider.cs             # SQL Server implementation
│   └── Configuration/
│       └── DatabaseSettings.cs            # Database config từ appsettings.json
│
├── 📁 Domain/                             # Core Business
│   └── Entities/
│       └── FinancialEntities.cs           # Revenue/Expense entities
│
├── 📁 Shared/                             # Common Utilities
│   ├── Constants/
│   │   └── AppConstants.cs                # App constants
│   └── Exceptions/
│       └── CustomExceptions.cs            # Custom exceptions
│
└── 📁 Configuration Files/
    ├── appsettings.json                   # Main config
    ├── appsettings.Development.json       # Dev config
    └── Program.cs                         # DI container setup
```

---

## 🔄 **LUỒNG DỮ LIỆU CHI TIẾT**

### **Từ Request đến Database:**

```
📱 Client (Power Apps/Postman)
    ↓ HTTP POST Request
🎮 Controller (RevenueController/ExpenseController)
    ↓ Call service interface
🏢 Service Interface (IRevenueService/IExpenseService)  
    ↓ Business logic implementation
⚙️ Service Implementation (RevenueService/ExpenseService)
    ↓ Data validation + processing
🔌 Data Provider Interface (IDataProvider)
    ↓ SQL operations
💽 SQL Data Provider (SqlDataProvider)
    ↓ Execute SQL commands
🗄️ SQL Server Database
```

### **Chi tiết từng bước:**

#### **1. Controller Layer (API Endpoints)**
```csharp
// RevenueController.cs
[HttpPost("import")]
public async Task<ActionResult<BulkOperationResultDto>> Import([FromBody] List<RevenueImportDto> data)
{
    // Chỉ làm:
    // - Nhận HTTP request 
    // - Gọi service
    // - Trả về HTTP response
    var result = await _revenueService.BulkInsertAsync(data);
    return Ok(result);
}
```

#### **2. Service Layer (Business Logic)**
```csharp
// RevenueService.cs - Đây là nơi làm việc chính
public async Task<BulkOperationResultDto> BulkInsertAsync(List<RevenueImportDto> data)
{
    // Step 1: Validate dữ liệu từ Excel
    ValidateRequiredFields(data);     // Check required fields
    ValidateDataTypes(data);          // Check data types (int, decimal...)
    
    // Step 2: Convert sang DataTable cho SQL Server
    var dataTable = CreateDataTable(data);
    
    // Step 3: Gọi database provider để insert
    var result = await _dataProvider.BulkInsertAsync(dataTable, "TaiChinh_ThuHoatDong");
    
    return result;
}
```

#### **3. Data Provider Layer (Database Access)**
```csharp
// SqlDataProvider.cs - Xử lý SQL Server
public async Task<int> BulkInsertAsync(DataTable dataTable, string tableName)
{
    // Step 1: Mở connection
    using var connection = new SqlConnection(_connectionString);
    
    // Step 2: Sử dụng SqlBulkCopy cho performance
    using var bulkCopy = new SqlBulkCopy(connection);
    bulkCopy.DestinationTableName = tableName;
    
    // Step 3: Map columns Excel -> SQL table
    MapColumns(bulkCopy, dataTable);
    
    // Step 4: Execute bulk insert
    await bulkCopy.WriteToServerAsync(dataTable);
    
    return dataTable.Rows.Count;
}
```

---

## 🎯 **TẠI SAO THIẾT KẾ NHƯ VẬY?**

### **1. Separation of Concerns (Tách biệt nhiệm vụ)**
- **Controller**: Chỉ lo HTTP request/response
- **Service**: Chỉ lo business logic
- **DataProvider**: Chỉ lo database operations
- **DTOs**: Chỉ lo transfer data giữa layers

### **2. Dependency Injection (DI)**
```csharp
// Program.cs
builder.Services.AddScoped<IDataProvider, SqlDataProvider>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Controller nhận service qua constructor
public RevenueController(IRevenueService revenueService)
{
    _revenueService = revenueService; // Tự động inject
}
```

### **3. Interface-Based Design**
```csharp
// Interface định nghĩa contract
public interface IRevenueService
{
    Task<BulkOperationResultDto> BulkInsertAsync(List<RevenueImportDto> data);
}

// Implementation thựa hiện contract
public class RevenueService : IRevenueService
{
    // Actual implementation here
}
```

---

## 🔍 **API HIỆN TẠI**

### **Revenue API:**
```http
POST /api/revenue/import
Content-Type: application/json

[
    {
        "thangTaiChinh": "12",
        "namTaiChinh": "2024", 
        "idNguon": "1",
        "loaiThu": "Lương",
        "soTien": "15000000",
        "moTa": "Lương tháng 12",
        "ghiChu": "",
        "idNguoiDung": "USER001",
        "nguoiNhap": "Nguyen Van A"
    }
]
```

### **Expense API:**
```http
POST /api/expense/import  
Content-Type: application/json

[
    {
        "thangTaiChinh": "12",
        "namTaiChinh": "2024",
        "idNguon": "1", 
        "loaiChi": "Văn phòng phẩm",
        "soTien": "500000",
        "moTa": "Mua bút, giấy",
        "ghiChu": "",
        "idNguoiDung": "USER001",
        "nguoiNhap": "Nguyen Van A"
    }
]
```

### **Response Format:**
```json
{
  "success": true,
  "message": "Revenue import successful",
  "totalRows": 1,
  "processedRows": 1,
  "insertedRows": 1,
  "errors": [],
  "data": null
}
```

---

## 🛡️ **BẢO MẬT VÀ PERFORMANCE**

### **SQL Injection Prevention:**
```csharp
// ❌ NGUY HIỂM - Dễ bị SQL injection
var sql = $"SELECT * FROM Table WHERE Id = {userId}";

// ✅ AN TOÀN - Sử dụng parameters
var sql = "SELECT * FROM Table WHERE Id = @userId";
var parameters = new Dictionary<string, object> { ["@userId"] = userId };
await _dataProvider.GetDataTableAsync(sql, parameters);
```

### **Performance Optimization:**
- **SqlBulkCopy**: Insert hàng nghìn records trong vài giây
- **Async/Await**: Non-blocking operations
- **Connection Pooling**: Tự động quản lý connections
- **DataTable**: Memory-efficient cho bulk operations

---

## 🚀 **CÁCH THÊM DOMAIN MỚI**

### **Ví dụ: Thêm Employee Management**

#### **Bước 1: Tạo Entity**
```csharp
// Domain/Entities/HREntities.cs
public class EmployeeEntity
{
    public int Id { get; set; }
    public string EmployeeName { get; set; }
    public string Department { get; set; }
    public decimal Salary { get; set; }
    // ...
}
```

#### **Bước 2: Tạo DTOs**
```csharp
// Application/DTOs/HRDTOs.cs
public class EmployeeImportDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Salary { get; set; } = string.Empty;
    // ...
}
```

#### **Bước 3: Tạo Interface**
```csharp
// Application/Interfaces/HR/IEmployeeService.cs
public interface IEmployeeService
{
    Task<BulkOperationResultDto> BulkInsertAsync(List<EmployeeImportDto> data);
}
```

#### **Bước 4: Implement Service**
```csharp
// Application/Services/HR/EmployeeService.cs
public class EmployeeService : IEmployeeService
{
    // Copy pattern từ RevenueService.cs
    // Modify validation rules cho Employee
}
```

#### **Bước 5: Tạo Controller**
```csharp
// Controllers/HR/EmployeeController.cs
[Route("api/employee")]
public class EmployeeController : ControllerBase
{
    // Copy pattern từ RevenueController.cs
}
```

#### **Bước 6: Đăng ký DI**
```csharp
// Program.cs
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
```

---

## 📚 **CODING STANDARDS**

### **Naming Convention:**
- **Controllers**: `{Domain}Controller.cs` (e.g., `EmployeeController.cs`)
- **Services**: `{Domain}Service.cs` (e.g., `EmployeeService.cs`)  
- **Interfaces**: `I{Domain}Service.cs` (e.g., `IEmployeeService.cs`)
- **DTOs**: `{Domain}ImportDto.cs` (e.g., `EmployeeImportDto.cs`)

### **Method Naming:**
- **CRUD**: `CreateAsync`, `GetAsync`, `UpdateAsync`, `DeleteAsync`
- **Bulk**: `BulkInsertAsync`, `BulkUpdateAsync`
- **Validation**: `ValidateRequiredFields`, `ValidateDataTypes`

### **Error Handling:**
```csharp
// Consistent error response format
return new BulkOperationResultDto
{
    Success = false,
    Message = "Validation failed",
    Errors = { "Dòng 1: Tên không được trống" }
};
```

---

## 🔧 **DEBUGGING GUIDE**

### **Common Issues:**

1. **Build Errors:**
   ```bash
   # Check for missing interfaces
   dotnet build
   ```

2. **API Not Working:**
   ```bash
   # Check DI registration in Program.cs
   # Check connection string in appsettings.json
   ```

3. **Database Errors:**
   ```bash
   # Check SQL Server connection
   # Check table structure matches Entity
   ```

### **Debugging Steps:**
1. **Controller**: Debug request/response
2. **Service**: Debug business logic + validation
3. **DataProvider**: Debug SQL execution
4. **Database**: Check actual data inserted

---

## 📖 **SUMMARY**

### **Hiện tại có:**
- ✅ 2 API endpoints: Revenue Import + Expense Import
- ✅ Clean Architecture với proper separation
- ✅ Security: Parameterized queries
- ✅ Performance: SqlBulkCopy cho bulk operations
- ✅ Maintainability: Interface-based design

### **Tương lai sẽ thêm:**
- 🔄 More domains: HR, Inventory, Accounting
- 🔄 CRUD operations: Create, Read, Update, Delete
- 🔄 Advanced features: Reports, Analytics, Real-time

### **Key Points để nhớ:**
1. **Controllers**: Chỉ lo HTTP, không có business logic
2. **Services**: Chứa ALL business logic + validation  
3. **DataProvider**: Chỉ lo database operations
4. **DTOs**: Transfer data giữa layers
5. **Interfaces**: Contracts cho dependency injection

**Mọi thứ đều theo pattern này - copy và modify cho domain mới!** 🚀