# RunASP.NET x64 Deployment Script
# This script publishes the Task Management API as x64 for RunASP.NET deployment

param(
    [string]$Configuration = "Release",
    [string]$PublishPath = "./publish/runasp",
    [switch]$SelfContained = $false,
    [switch]$Clean = $true
)

Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Task Management API - RunASP.NET x64 Publishing Script       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Navigate to project directory
$projectPath = ".\TaskManagementAPI"
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Error: TaskManagementAPI directory not found" -ForegroundColor Red
    exit 1
}

# Clean if requested
if ($Clean) {
    Write-Host "`n🧹 Cleaning previous builds..." -ForegroundColor Yellow
    Push-Location $projectPath
    dotnet clean -c $Configuration
    Pop-Location
}

# Build
Write-Host "`n🔨 Building application (x64, Release)..." -ForegroundColor Yellow
Push-Location $projectPath
dotnet build -c $Configuration -r win-x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
Pop-Location

# Publish
Write-Host "`n📦 Publishing for x64 Windows Runtime..." -ForegroundColor Yellow
$publishArgs = @(
    "publish",
    "-c", $Configuration,
    "-r", "win-x64",
    "-o", $PublishPath,
    "--no-build",
    "--no-restore"
)

if ($SelfContained) {
    $publishArgs += "--self-contained"
    Write-Host "   (Self-contained: includes .NET 9 runtime)" -ForegroundColor Cyan
} else {
    Write-Host "   (Framework-dependent: requires .NET 9 on server)" -ForegroundColor Cyan
}

Push-Location $projectPath
dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Publish failed" -ForegroundColor Red
    exit 1
}
Pop-Location

# Verify published files
Write-Host "`n📋 Verifying published files..." -ForegroundColor Yellow
$publishDir = Resolve-Path $PublishPath

# Check for executable
if (Test-Path "$publishDir\TaskManagementAPI.exe") {
    Write-Host "   ✅ TaskManagementAPI.exe found" -ForegroundColor Green
} else {
    Write-Host "   ❌ TaskManagementAPI.exe not found" -ForegroundColor Red
}

# Check for DLL
if (Test-Path "$publishDir\TaskManagementAPI.dll") {
    Write-Host "   ✅ TaskManagementAPI.dll found" -ForegroundColor Green
} else {
    Write-Host "   ❌ TaskManagementAPI.dll not found" -ForegroundColor Red
}

# Check for configuration files
if (Test-Path "$publishDir\appsettings.Production.json") {
    Write-Host "   ✅ appsettings.Production.json found" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  appsettings.Production.json not found" -ForegroundColor Yellow
}

# List published files
Write-Host "`n📂 Published files structure:" -ForegroundColor Yellow
Get-ChildItem -Path $publishDir -File | Select-Object -First 15 | ForEach-Object {
    Write-Host "   - $($_.Name)" -ForegroundColor Gray
}
$fileCount = (Get-ChildItem -Path $publishDir -Recurse -File | Measure-Object).Count
Write-Host "   ... and $(($fileCount - 15)) more files" -ForegroundColor Gray

# Verify architecture
Write-Host "`n🔍 Verifying x64 Architecture..." -ForegroundColor Yellow

# Check file type
$exePath = "$publishDir\TaskManagementAPI.exe"
if (Test-Path $exePath) {
    # Try to read PE header
    $bytes = [System.IO.File]::ReadAllBytes($exePath)
    $magic = [System.BitConverter]::ToUInt16($bytes, 0x3c)
    $peSignature = [System.Text.Encoding]::ASCII.GetString($bytes, $magic, 4)
    
    if ($peSignature -eq "PE`0`0") {
        $machine = [System.BitConverter]::ToUInt16($bytes, $magic + 4)
        if ($machine -eq 0x8664) {
            Write-Host "   ✅ Architecture: x64 (PE32+) - Ready for RunASP.NET!" -ForegroundColor Green
        } elseif ($machine -eq 0x014c) {
            Write-Host "   ❌ Architecture: x86 (PE32) - NOT suitable for RunASP.NET" -ForegroundColor Red
            Write-Host "   Please rebuild with x64 configuration" -ForegroundColor Red
        }
    }
}

# Show deployment size
Write-Host "`n📊 Deployment Size:" -ForegroundColor Yellow
$size = (Get-ChildItem -Path $publishDir -Recurse | Measure-Object -Property Length -Sum).Sum
$sizeMB = [Math]::Round($size / 1MB, 2)
Write-Host "   Total: $sizeMB MB" -ForegroundColor Gray

# Final instructions
Write-Host "`n✅ Publishing Complete!" -ForegroundColor Green
Write-Host "`n📝 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Verify all files in: $publishDir" -ForegroundColor White
Write-Host "   2. Create web.config in the root directory" -ForegroundColor White
Write-Host "   3. Upload all files to RunASP.NET via FTP" -ForegroundColor White
Write-Host "   4. Update appsettings.Production.json with production values" -ForegroundColor White
Write-Host "   5. Test endpoints:" -ForegroundColor White
Write-Host "      - http://task-management-api-73.runasp.net/health" -ForegroundColor Cyan
Write-Host "      - http://task-management-api-73.runasp.net/swagger" -ForegroundColor Cyan
Write-Host "`n📖 For detailed instructions, see: RUNASP_DEPLOYMENT.md" -ForegroundColor White
Write-Host ""
