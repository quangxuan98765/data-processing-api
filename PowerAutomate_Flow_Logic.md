# Power Automate Flow Complete Guide - CLEAN ARCHITECTURE

## 🏗️ **NEW CLEAN ARCHITECTURE ENDPOINTS:**

### 📊 **Revenue Endpoints:**
- `POST /api/revenue/import` - Import revenue data
- `POST /api/revenue/validate` - Validate revenue before import
- `GET /api/revenue/period/{month}/{year}` - Get revenue by period
- `GET /api/revenue/exists/{month}/{year}/{sourceId}` - Check revenue exists

### 💰 **Expense Endpoints:**
- `POST /api/expense/import` - Import expense data
- `POST /api/expense/validate` - Validate expense before import
- `GET /api/expense/period/{month}/{year}` - Get expense by period
- `GET /api/expense/exists/{month}/{year}/{sourceId}` - Check expense exists

---

## 🔄 **POWER AUTOMATE FLOW - CẬP NHẬT:**

### 1. **When PowerApp calls a flow V2**
- Trigger từ Power App
- Nhận parameters: File Excel, IDNguoiDung, NguoiNhap, **DataType (revenue/expense)**

### 2. **Create file (SharePoint)**
- **File Name:** `Input_@{triggerBody()?['dataType']}_@{utcNow()}.xlsx`
- **File Content:** `@{triggerBody()?['file']['contentBytes']}`
- **Folder Path:** `/Shared Documents/TaiChinhInput/`

### 3. **Get tables (Excel Online)**
- **Location:** SharePoint Site
- **Document Library:** Shared Documents
- **File:** Output từ step 2
- **Table:** Table1 (hoặc tên table trong Excel)

### 4. **List rows present in a table**
- **Location:** SharePoint Site  
- **Document Library:** Shared Documents
- **File:** Output từ step 2
- **Table:** Output từ step 3

### 5. **Condition - Check Data Type**
- **Condition:** `@{triggerBody()?['dataType']}` equals `revenue`

---

## ✅ **REVENUE BRANCH:**

### 5.1. **HTTP - Call Revenue API**
- **Method:** POST
- **URI:** `https://eb65f130856c.ngrok-free.app/api/revenue/import`
- **Headers:**
  ```json
  {
    "Content-Type": "application/json",
    "ngrok-skip-browser-warning": "true"
  }
  ```
- **Body:**
  ```json
  {
    "data": @{body('List_rows_present_in_a_table')?['value']},
    "idNguoiDung": "@{triggerBody()?['IDNguoiDung']}",
    "nguoiNhap": "@{triggerBody()?['NguoiNhap']}"
  }
  ```

### 5.2. **Condition - Check Revenue API Success**
- **Condition:** `@{body('HTTP_Revenue')?['success']}` equals `true`

---

## ✅ **EXPENSE BRANCH:**

### 5.1. **HTTP - Call Expense API**
- **Method:** POST
- **URI:** `https://eb65f130856c.ngrok-free.app/api/expense/import`
- **Headers:**
  ```json
  {
    "Content-Type": "application/json",
    "ngrok-skip-browser-warning": "true"
  }
  ```
- **Body:**
  ```json
  {
    "data": @{body('List_rows_present_in_a_table')?['value']},
    "idNguoiDung": "@{triggerBody()?['IDNguoiDung']}",
    "nguoiNhap": "@{triggerBody()?['NguoiNhap']}"
  }
  ```

### 5.2. **Condition - Check Expense API Success**
- **Condition:** `@{body('HTTP_Expense')?['success']}` equals `true`

---

## ✅ **SUCCESS BRANCHES (Revenue/Expense):**

---

## ✅ **SUCCESS BRANCHES:**

### 6.1. **Revenue Success Actions**
- **Create Success File:** `Success_Revenue_@{utcNow()}.xlsx`
- **Email Subject:** `✅ Import dữ liệu doanh thu thành công`
- **Response Data:** Revenue-specific results

### 6.2. **Expense Success Actions**  
- **Create Success File:** `Success_Expense_@{utcNow()}.xlsx`
- **Email Subject:** `✅ Import dữ liệu chi phí thành công`
- **Response Data:** Expense-specific results

---

## ❌ **ERROR BRANCHES:**

### 7.1. **Revenue Error Actions**
- **Create Error File:** `Error_Revenue_@{utcNow()}.xlsx`
- **Email Subject:** `❌ Lỗi import dữ liệu doanh thu`
- **Error Details:** Revenue-specific error handling

### 7.2. **Expense Error Actions**
- **Create Error File:** `Error_Expense_@{utcNow()}.xlsx`
- **Email Subject:** `❌ Lỗi import dữ liệu chi phí`
- **Error Details:** Expense-specific error handling

---

## 🔧 **MIGRATION NOTES:**

### ⚠️ **BREAKING CHANGES:**
1. **Old Endpoint:** `/api/bulkinsert/financial-data` ❌
2. **New Endpoints:** 
   - `/api/revenue/import` ✅
   - `/api/expense/import` ✅

### 📝 **Power App Updates Required:**
1. Add **DataType** parameter (`revenue` or `expense`)
2. Update flow to call correct endpoint based on data type
3. Handle separate success/error responses for each type

### 🎯 **Benefits of New Architecture:**
- ✅ **Separation of Concerns:** Revenue và Expense logic tách biệt
- ✅ **Maintainability:** Dễ debug và phát triển
- ✅ **Scalability:** Có thể mở rộng từng module độc lập
- ✅ **Clear API:** Endpoints rõ ràng theo business domain

---

## 🏗️ **CLEAN ARCHITECTURE STRUCTURE:**

```
Application/
├── Interfaces/
│   └── Financial/
│       ├── IRevenueService.cs
│       └── IExpenseService.cs
└── Services/
    └── Financial/
        ├── RevenueService.cs
        └── ExpenseService.cs

Controllers/
└── Financial/
    ├── RevenueController.cs
    └── ExpenseController.cs
```

### 🔄 **Service Methods:**
- `BulkInsertAsync()` - Import data with validation
- `ValidateAsync()` - Validate data before import  
- `CheckDataExistsAsync()` - Check for existing data
- `GetByPeriodAsync()` - Retrieve data by month/year
