# 1. Complete clean (xóa bin, obj folders)
Write-Host "🧹 Starting clean build process..." -ForegroundColor Cyan
cd "d:\CSharp API Project"
Write-Host "🗑️  Cleaning bin and obj folders..." -ForegroundColor Yellow
Get-ChildItem -Path . -Recurse -Name "bin" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path . -Recurse -Name "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# 2. Delete publish folder completely (with retry)
if (Test-Path "publish") {
    Write-Host "🗑️  Removing old publish folder..." -ForegroundColor Yellow
    try {
        # Stop any processes that might be using the files
        Get-Process | Where-Object { $_.Path -like "*publish*" } | Stop-Process -Force -ErrorAction SilentlyContinue
        
        # Remove with better error handling
        Remove-Item "publish" -Recurse -Force -ErrorAction Stop
        Write-Host "✅ Old publish folder removed" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️  Some files couldn't be deleted (might be in use). Continuing..." -ForegroundColor Yellow
        # Create new publish folder if removal failed
        if (-not (Test-Path "publish")) {
            New-Item -ItemType Directory -Path "publish" -Force | Out-Null
        }
    }
}

# 3. Restore packages
Write-Host "📦 Restoring packages..." -ForegroundColor Cyan
dotnet restore

# 4. Build solution
Write-Host "🔨 Building solution..." -ForegroundColor Cyan
dotnet build -c Release

# 5. Publish fresh
Write-Host "📤 Publishing application..." -ForegroundColor Cyan
dotnet publish "DataProcessingAPI\DataProcessingAPI.csproj" -c Release -o "publish" --self-contained false

# 6. Check if JWT dll exists (safer way)
if (Test-Path "publish\System.IdentityModel.Tokens.Jwt.dll") {
    Write-Host "✅ JWT Library found in publish folder" -ForegroundColor Green
    try {
        $jwtFile = Get-Item "publish\System.IdentityModel.Tokens.Jwt.dll"
        Write-Host "📄 JWT File Version: $($jwtFile.VersionInfo.FileVersion)" -ForegroundColor Cyan
    }
    catch {
        Write-Host "⚠️  Could not read JWT version (but file exists)" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ JWT Library NOT found in publish folder" -ForegroundColor Red
}

# 7. Fix web.config with better error handling
Write-Host "🔧 Creating web.config..." -ForegroundColor Cyan
$webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\DataProcessingAPI.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" forwardWindowsAuthToken="false">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />
          <environmentVariable name="ASPNETCORE_DETAILEDERRORS" value="true" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@

try {
    $webConfig | Out-File "publish\web.config" -Encoding UTF8
    Write-Host "✅ web.config created successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to create web.config: $($_.Exception.Message)" -ForegroundColor Red
}

# 8. Create logs directory
try {
    $null = New-Item "publish\logs" -ItemType Directory -Force
    Write-Host "✅ Logs directory created" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to create logs directory: $($_.Exception.Message)" -ForegroundColor Red
}

# 9. Final verification
Write-Host "`n🔍 PUBLISH VERIFICATION:" -ForegroundColor Yellow
Write-Host "📁 Publish folder size: $((Get-ChildItem -Path publish -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB) MB" -ForegroundColor Cyan
Write-Host "📄 Main DLL exists: $(Test-Path 'publish\DataProcessingAPI.dll')" -ForegroundColor Cyan
Write-Host "📄 web.config exists: $(Test-Path 'publish\web.config')" -ForegroundColor Cyan
Write-Host "📁 logs folder exists: $(Test-Path 'publish\logs')" -ForegroundColor Cyan

Write-Host "`n✅ Fresh publish completed! Ready for IIS deployment." -ForegroundColor Green