# 1. Complete clean (xóa bin, obj folders)
cd "d:\CSharp API Project"
Get-ChildItem -Path . -Recurse -Name "bin" | Remove-Item -Recurse -Force
Get-ChildItem -Path . -Recurse -Name "obj" | Remove-Item -Recurse -Force

# 2. Delete publish folder completely
Remove-Item "publish" -Recurse -Force

# 3. Restore packages
dotnet restore

# 4. Build solution
dotnet build -c Release

# 5. Publish fresh
dotnet publish "DataProcessingAPI\DataProcessingAPI.csproj" -c Release -o "publish" --self-contained false

# 6. Verify JWT version in publish folder
Add-Type -Path "publish\System.IdentityModel.Tokens.Jwt.dll"
[System.Reflection.Assembly]::LoadFrom((Resolve-Path "publish\System.IdentityModel.Tokens.Jwt.dll").Path).GetName().Version

# 7. Fix web.config
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

$webConfig | Out-File "publish\web.config" -Encoding UTF8
mkdir "publish\logs" -Force

Write-Host "✅ Fresh publish completed!"