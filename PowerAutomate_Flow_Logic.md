# Power Automate Flow Complete Guide

## 🔄 FLOW HOÀN CHỈNH - CHI TIẾT TỪNG BƯỚC:

### 1. **When PowerApp calls a flow V2**
- Trigger từ Power App
- Nhận parameters: File Excel, IDNguoiDung, NguoiNhap

### 2. **Create file (SharePoint)**
- **File Name:** `Input_@{utcNow()}.xlsx`
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

### 5. **HTTP - Call C# API**
- **Method:** POST
- **URI:** `https://eb65f130856c.ngrok-free.app/api/bulkinsert/financial-data`
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

### 6. **Condition - Check API Success**
- **Condition:** `@{body('HTTP')?['success']}` equals `true`

---

## ✅ **YES BRANCH (API Success):**

### 7a. **Create CSV table - Result Data**
- **From:** `@{body('HTTP')?['excelOutputData']}`
- **Columns:** (Tự động detect từ JSON structure)
  - ThangTaiChinh
  - NamTaiChinh  
  - TenNguon
  - LoaiThu
  - SoTien
  - MoTa
  - GhiChu
  - KetQuaXuLy

### 8a. **Create file (SharePoint) - Result File**
- **File Name:** `KetQua_@{utcNow()}.csv`
- **File Content:** `@{body('Create_CSV_table')}`
- **Folder Path:** `/Shared Documents/TaiChinhOutput/`

### 9a. **Compose - Download Link**
- **Inputs:** `@{body('Create_file_2')?['WebUrl']}`

### 10a. **Respond to PowerApp - Success**
```json
{
  "DownloadUrl": "@{outputs('Compose')}",
  "ErrorMessage": "",
  "Summary": {
    "TotalRows": @{body('HTTP')?['totalRows']},
    "ValidRows": @{body('HTTP')?['validRows']},
    "InvalidRows": @{body('HTTP')?['invalidRows']},
    "Message": "@{body('HTTP')?['message']}"
  },
  "Success": true
}
```

---

## ❌ **NO BRANCH (API Error):**

### 7b. **Respond to PowerApp - Error**
```json
{
  "DownloadUrl": "",
  "ErrorMessage": "@{body('HTTP')?['message']}",
  "Summary": {
    "TotalRows": @{body('HTTP')?['totalRows']},
    "ValidRows": @{body('HTTP')?['validRows']},
    "InvalidRows": @{body('HTTP')?['invalidRows']},
    "Message": "@{body('HTTP')?['message']}"
  },
  "Success": false
}
```

---

## 🎯 **QUAN TRỌNG - Create CSV Table Setup:**

**"From" field configuration:**
1. Click vào **"From"** field
2. Chọn **Dynamic content**
3. Tìm **"HTTP"** action  
4. Chọn **"body"**
5. Thêm đường dẫn: `['excelOutputData']`
6. Final value: `@{body('HTTP')?['excelOutputData']}`

**Columns sẽ tự động được detect từ JSON structure của ExcelOutputData**

---

## 📊 **Power App Response Handling:**

Power App sẽ nhận được response với cấu trúc:
- **Success = true:** Hiển thị download link và summary
- **Success = false:** Hiển thị error message  
- **Summary:** Luôn có thông tin thống kê (total, valid, invalid rows)
