# Chuyển vào thư mục project (chứa ngrok.exe)
cd "D:\CSharp API Project\ExcelDataAPI"

# Hỏi port API (mặc định 5000)
$port = Read-Host "Nhập port API (mặc định 5000)"
if ([string]::IsNullOrWhiteSpace($port)) { $port = 5000 }

# Mở cửa sổ PowerShell mới để chạy API
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run"


# Delay 5 giây cho API khởi động
Start-Sleep -Seconds 5
# Mở cửa sổ PowerShell mới để chạy ngrok
Start-Process powershell -ArgumentList "-NoExit", "-Command", "ngrok http $port"
