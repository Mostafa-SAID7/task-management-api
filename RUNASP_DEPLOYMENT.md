# RunASP.NET Deployment Guide - Task Management API

## ⚠️ Current Issue
```
HTTP Error 500.31 - Failed to load ASP.NET Core runtime
Error: Could not find 'aspnetcorev2_inprocess.dll'
Architecture: x86 (32-bit)
Framework: Microsoft.NETCore.App 9.0.0 (x86)
```

**Root Cause**: RunASP.NET x86 hosting doesn't support .NET 9

---

## ✅ Solution: Deploy as x64 (64-bit)

### Step 1: Publish for x64 Architecture

```bash
cd task-management-api/TaskManagementAPI

# Option A: Using dotnet publish with x64 runtime identifier
dotnet publish -c Release -r win-x64 -o ./publish/runasp

# Option B: Using Visual Studio
# Project → Publish → Edit → Configuration → Target Runtime: win-x64

# Option C: Update project file for x64 default
# Edit TaskManagementAPI.csproj and add:
# <RuntimeIdentifier>win-x64</RuntimeIdentifier>
# <PlatformTarget>x64</PlatformTarget>
```

### Step 2: Verify Published Files

After publishing, check the output folder:
```
./publish/runasp/
├── TaskManagementAPI.exe        ← Should be x64 executable
├── TaskManagementAPI.dll        ← Core assembly
├── appsettings.json
├── appsettings.Production.json
├── web.config                   ← IIS configuration
└── [other dependencies]
```

**Verify architecture** (on Windows):
```powershell
# If you see "PE32+" → x64 ✅
# If you see "PE32" → x86 ❌
```

### Step 3: Update web.config for Out-of-Process Hosting

RunASP.NET recommends **out-of-process** hosting (more stable). Create/update `web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\TaskManagementAPI.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="outofprocess" />
    </system.webServer>
  </location>
</configuration>
```

### Step 4: Update Project File (.csproj)

Edit `TaskManagementAPI.csproj` to ensure x64 deployment:

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PlatformTarget>x64</PlatformTarget>
  <SelfContained>false</SelfContained>
  <PublishReadyToRun>true</PublishReadyToRun>
  <PublishTrimmed>false</PublishTrimmed>
</PropertyGroup>
```

### Step 5: Deploy to RunASP.NET

#### Method A: FTP Upload

1. Connect via FTP: `ftp://task-management-api-73.runasp.net`
2. Navigate to `/wwwroot/` directory
3. Delete existing files (except web.config if you just created it)
4. Upload all files from `./publish/runasp/`
5. Ensure `web.config` is present

#### Method B: Using Visual Studio Publish

1. Right-click Project → **Publish**
2. Select **Folder** profile
3. Configure:
   - **Location**: `./publish/runasp`
   - **Configuration**: Release
   - **Target Runtime**: win-x64
4. Click **Publish**
5. Upload published folder to RunASP.NET via FTP

#### Method C: PowerShell Upload Script

```powershell
# Set credentials
$FtpServer = "ftp://task-management-api-73.runasp.net"
$FtpUser = "your_ftp_user"
$FtpPass = "your_ftp_password"
$LocalPath = "C:\path\to\publish\runasp\*"
$FtpPath = "/wwwroot/"

# Create FTP credentials
$Credential = New-Object System.Net.NetworkCredential($FtpUser, $FtpPass)

# Upload files
$WebClient = New-Object System.Net.WebClient
$WebClient.Credentials = $Credential

# For each file in publish folder
Get-ChildItem -Path $LocalPath -Recurse | ForEach-Object {
    $RemoteFile = "$FtpServer$FtpPath$($_.Name)"
    Write-Host "Uploading: $($_.FullName) to $RemoteFile"
    # $WebClient.UploadFile($RemoteFile, $_.FullName)
}
```

### Step 6: Configure Application Pool

1. Connect to RunASP.NET control panel
2. Go to **Application Pool settings**
3. Ensure:
   - ✅ **Runtime version**: .NET 4.0 (even for .NET 9, IIS uses this setting)
   - ✅ **Enable 32-bit Applications**: **FALSE** (x64 mode)
   - ✅ **Pipeline Mode**: Integrated
   - ✅ **Recycling**: Set to 30+ minutes
   - ✅ **Load User Profile**: True

### Step 7: Set Environment Variables

In RunASP.NET control panel or via `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ASPNETCORE_ENVIRONMENT": "Production",
  "ConnectionStrings": {
    "DefaultConnection": "Server=db.bqtbimefbeipvaabfper.supabase.co;Port=5432;Database=postgres;User Id=postgres;Password=Memo@356000;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "7Mg7wV45m8eF8N/n8+chfR6TwmEpXnGCTIydLHme4mKu1I2vtgOSCUoiDu76hRQfYLhVeawJs/PheU+bcjztuA==",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementAPI",
    "ExpirationMinutes": 60
  }
}
```

---

## 🔍 Troubleshooting Deployment Issues

### Issue 1: Still Getting x86 Error

**Solution**:
```bash
# Clean before rebuilding
dotnet clean
dotnet build -c Release

# Publish with explicit x64
dotnet publish -c Release -r win-x64 --no-build -o ./publish/runasp
```

### Issue 2: "No frameworks were found"

**Solution**: The server needs .NET 9 Runtime. RunASP.NET may not have it pre-installed.

**Options**:
1. Use **self-contained** deployment (includes .NET runtime)
   ```bash
   dotnet publish -c Release -r win-x64 -o ./publish/runasp --self-contained
   ```
   
2. Request RunASP.NET to install .NET 9 Runtime

3. Switch to **Docker** deployment instead

### Issue 3: Application Pool Timeout

**Solution**: Increase timeout in RunASP.NET control panel:
- Set to **60+ minutes** instead of 30 minutes
- Enable **Always Running** mode

### Issue 4: Database Connection Fails

**Solution**: Verify connection string in `appsettings.Production.json`:

```bash
# Test connection locally first
dotnet run --environment Production

# Check logs for connection errors
# Navigate to RunASP.NET logs folder
```

### Issue 5: Port Already in Use

**Solution**: RunASP.NET uses port **80/443** by default. Verify in web.config:

```xml
<bindings>
  <binding protocol="http" bindingInformation="*:80:" />
  <binding protocol="https" bindingInformation="*:443:" />
</bindings>
```

---

## ✅ Verification Checklist

After deployment, verify:

- [ ] Application loads at `http://task-management-api-73.runasp.net`
- [ ] `/health` endpoint returns `200 OK` with JSON
- [ ] `/ready` endpoint returns `200 OK`
- [ ] `/swagger` page loads successfully
- [ ] Database connection working (check logs)
- [ ] No 500.31 or 500.19 errors
- [ ] Application pool not recycling excessively

### Quick Health Check

```bash
# Test from your local machine
curl -s http://task-management-api-73.runasp.net/health | jq .

# Expected response:
# {
#   "status": "healthy",
#   "timestamp": "2026-06-21T14:30:00.000Z",
#   "environment": "Production",
#   "version": "1.0.0",
#   "uptime": 3600.5
# }
```

---

## 📋 Deployment Checklist

### Before Publishing
- [ ] Update `appsettings.Production.json` with Supabase connection
- [ ] Verify JWT key is correct (64+ characters)
- [ ] Run `dotnet build -c Release` locally - **must succeed**
- [ ] Test health endpoints locally: `dotnet run --environment Production`

### Publishing
- [ ] Publish as x64: `dotnet publish -c Release -r win-x64`
- [ ] Verify executable is x64 (not x86)
- [ ] Create `web.config` with correct settings
- [ ] Include all required files (appsettings, DLLs, etc.)

### Uploading
- [ ] Clear old wwwroot files
- [ ] Upload entire publish folder via FTP
- [ ] Verify `web.config` is present
- [ ] Set correct file permissions (755 on Linux, inherited on Windows IIS)

### Post-Deployment
- [ ] Test `/health` endpoint
- [ ] Check application logs
- [ ] Monitor application pool
- [ ] Verify database connection
- [ ] Test Swagger UI access

---

## 🚀 Self-Contained Alternative (Recommended for RunASP.NET)

If RunASP.NET doesn't support .NET 9 Runtime, use self-contained deployment:

```bash
# Publish self-contained (includes .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish/runasp-selfcontained

# Size will be ~150-200MB (includes full .NET runtime)
# But no framework installation needed on server
```

Upload the `runasp-selfcontained` folder to RunASP.NET.

---

## 📞 Support Resources

- **RunASP.NET Documentation**: https://www.runasp.net/knowledge-base
- **IIS Error 500.31**: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/troubleshoot
- **.NET 9 Deployment**: https://learn.microsoft.com/en-us/dotnet/core/deploying/
- **Troubleshooting**: Check `D:\Sites\site62547\logs\*` for detailed error logs

---

## Contact RunASP.NET Support

If issue persists after trying above solutions:

1. **Enable verbose logging** in `web.config`:
   ```xml
   <aspNetCore processPath="dotnet" arguments=".\TaskManagementAPI.dll" 
               stdoutLogEnabled="true" 
               stdoutLogFile=".\logs\stdout" 
               hostingModel="outofprocess" />
   ```

2. **Check stdout logs**: `D:\Sites\site62547\logs\stdout_*.log`

3. **Contact RunASP.NET**: Submit support ticket with:
   - Error logs (full content)
   - Published files structure
   - Architecture confirmation (x64)
   - Request to install .NET 9 Runtime if needed

---

**Last Updated**: June 21, 2026  
**Status**: Ready for x64 Deployment
