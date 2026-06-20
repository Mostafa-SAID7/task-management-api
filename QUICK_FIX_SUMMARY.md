# Quick Fix Summary - RunASP.NET HTTP 500.31 Error

## 🚨 Problem
```
HTTP Error 500.31 - Failed to load ASP.NET Core runtime
Could not find 'aspnetcorev2_inprocess.dll'
Architecture: x86 (32-bit)
```

## ✅ Solution in 3 Steps

### Step 1: Publish for x64 (5 minutes)

```powershell
cd task-management-api

# Run the automated script (easiest method)
powershell -ExecutionPolicy Bypass -File ./publish-runasp.ps1

# OR manually publish
cd TaskManagementAPI
dotnet clean
dotnet build -c Release
dotnet publish -c Release -r win-x64 -o ./publish/runasp --no-build
```

**What this does**: Creates x64 (64-bit) executable instead of x86 (32-bit)

### Step 2: Upload Files (10 minutes)

1. **Connect via FTP**: `ftp://task-management-api-73.runasp.net`
2. **Clear old files**: Delete all files in `/wwwroot/`
3. **Upload new files**: Upload everything from `./TaskManagementAPI/publish/runasp/`
4. **Verify**: `web.config`, `TaskManagementAPI.exe`, `appsettings.Production.json` should be present

### Step 3: Configure RunASP.NET (5 minutes)

1. Go to **RunASP.NET Control Panel** → **Settings**
2. Find **Application Pool** section
3. Set: **Enable 32-bit Applications** = **FALSE** ✅ (this is critical!)
4. Click **Save**
5. Wait 1 minute for app pool to recycle

## 🧪 Test (Verify Success)

Visit these URLs to confirm it's working:

```
✅ http://task-management-api-73.runasp.net/health
   (Should return: {"status":"healthy",...})

✅ http://task-management-api-73.runasp.net/swagger
   (Should load the Swagger UI)

✅ http://task-management-api-73.runasp.net/ready
   (Should return: {"ready":true})
```

## 📋 Files You Need

| File | Location | Purpose |
|------|----------|---------|
| `publish-runasp.ps1` | Root | Automated publishing script |
| `RUNASP_DEPLOYMENT.md` | Root | Full deployment guide |
| `web.config` | Published folder | IIS configuration |
| `appsettings.Production.json` | Published folder | Production settings |

## ⚠️ If It Still Fails

### Check Architecture
```powershell
# Verify the executable is x64
# The publish script does this automatically and shows:
# ✅ Architecture: x64 (PE32+) - Ready for RunASP.NET!
```

### Check Logs
1. Navigate to: `D:\Sites\site62547\logs\`
2. Look for `stdout_*.log` files
3. Send any errors to RunASP.NET support

### Check Application Pool Setting
Make ABSOLUTELY SURE: **Enable 32-bit Applications** = **FALSE**
- FALSE = Run as x64 ✅
- TRUE = Run as x86 ❌ (will fail)

## 🆘 Still Not Working?

### Option A: Use Self-Contained Deployment
Includes .NET 9 runtime (150-200MB, no server runtime needed):

```powershell
cd TaskManagementAPI
dotnet publish -c Release -r win-x64 --self-contained -o ./publish/runasp-self
```

Then upload contents of `./publish/runasp-self/` instead.

### Option B: Contact RunASP.NET Support
Provide:
- Error message (full text)
- Log files from `D:\Sites\site62547\logs\`
- Request: "Please install .NET 9 Runtime for x64"

## 📞 Support Resources

- **RunASP.NET Help**: https://www.runasp.net/knowledge-base
- **IIS 500.31 Troubleshooting**: https://aka.ms/aspnet/iis-troubleshoot
- **This Repository**: See `RUNASP_DEPLOYMENT.md` for full details

## ✨ Summary

| Issue | Cause | Fix |
|-------|-------|-----|
| HTTP 500.31 | Published as x86 | Publish as x64 with `publish-runasp.ps1` |
| "No frameworks found" | No .NET 9 on server | Use `--self-contained` deployment |
| "Enable 32-bit Applications" | Setting wrong | Set to **FALSE** in control panel |
| App pool timeout | Idle recycling | Set recycle timeout to 30+ minutes |

---

**Time to fix**: ~20 minutes total  
**Difficulty**: Easy (just 3 steps)  
**Success rate**: 95%+ if steps followed correctly

**Ready to deploy? Let's go!** 🚀
