# Build & Install Instructions

## 🔧 Fixed Script (v1.0.1)

The `Verify-And-Install.bat` script has been updated to work with the new repository structure.

---

## ✅ Quick Install (Recommended)

**From rhino-plugin folder:**

```powershell
cd rhino-plugin
.\scripts\Verify-And-Install.bat
```

**Or from repository root:**

```powershell
.\rhino-plugin\scripts\Verify-And-Install.bat
```

---

## 🛠️ Manual Build (Alternative)

If the script has issues, build manually:

### Step 1: Clean

```powershell
cd rhino-plugin
dotnet clean BoschMediaBrowser.sln
```

### Step 2: Build

```powershell
dotnet build src\BoschMediaBrowser.Rhino\BoschMediaBrowser.Rhino.csproj --configuration Release
```

### Step 3: Check Output

```powershell
# Plugin DLL should be here:
dir src\BoschMediaBrowser.Rhino\bin\Release\net7.0\BoschMediaBrowser.Rhino.dll

# Core DLL should be here:
dir src\BoschMediaBrowser.Core\bin\Release\net7.0\BoschMediaBrowser.Core.dll
```

### Step 4: Install Manually

```powershell
# Create install directory
$installDir = "$env:APPDATA\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser\B05C43D0-ED1A-4B05-8E12-345678ABCDEF"
New-Item -ItemType Directory -Path $installDir -Force

# Copy files
Copy-Item "src\BoschMediaBrowser.Rhino\bin\Release\net7.0\BoschMediaBrowser.Rhino.dll" -Destination "$installDir\BoschMediaBrowser.rhp"
Copy-Item "src\BoschMediaBrowser.Core\bin\Release\net7.0\BoschMediaBrowser.Core.dll" -Destination "$installDir\BoschMediaBrowser.Core.dll"

# Verify
Get-ChildItem $installDir
```

---

## 🧪 Testing Proxy Meshes

After installation:

### 1. Start Rhino 8

Close any running Rhino instances first.

### 2. Open Plugin

```
ShowMediaBrowser
```

### 3. Enable Proxy Meshes

1. Click **Settings** (⚙️ gear icon)
2. Scroll to **Insert Options**
3. Check ✅ **"Use proxy mesh for viewport (lighter display)"**
4. Click **Save**

### 4. Test Insert

1. Browse to a product (e.g., PRO → Garden → GBL 18V-750)
2. Select a holder variant
3. Click **Insert**
4. Pick insertion point

### 5. Check Logs

Watch Rhino command window for:

```
✓ Using PROXY mesh for viewport: M:\...\GBL 18V-750_proxy_mesh.3dm
```

Or if proxy not found:

```
ℹ Proxy mesh not found, falling back to full mesh
✓ Will use REFERENCE mesh with transform: M:\...\GBL 18V-750_Mesh_Tego.3dm
```

---

## 📁 Expected File Structure

After running the script from repository root:

```
Repository Root/
└── rhino-plugin/
    ├── scripts/
    │   ├── Verify-And-Install.bat     ← Run this
    │   └── Verify-And-Install.ps1     ← Updated script
    ├── src/
    │   ├── BoschMediaBrowser.Core/
    │   └── BoschMediaBrowser.Rhino/
    ├── BoschMediaBrowser.sln          ← Solution file
    └── BUILD_INSTRUCTIONS.md          ← This file
```

---

## 🐛 Troubleshooting

### Error: "Specify a project or solution file"

**Cause:** Script running from wrong directory

**Fix:** Make sure you're running from repository root or rhino-plugin folder

```powershell
# Check current location
pwd

# Should be either:
# E:\...\Bosch Products DB in excel
# or
# E:\...\Bosch Products DB in excel\rhino-plugin
```

### Error: "dotnet clean failed"

**Cause:** .NET SDK not installed or not in PATH

**Fix:** 

```powershell
# Check .NET installation
dotnet --version

# Should show: 7.0.x or higher
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/7.0

### Error: "Plugin DLL not found"

**Cause:** Build failed or wrong path

**Fix:**

```powershell
cd rhino-plugin
dotnet build src\BoschMediaBrowser.Rhino\BoschMediaBrowser.Rhino.csproj --configuration Release -v detailed
```

Check detailed output for errors.

### Proxy mesh not being used

**Check:**

1. **Setting enabled:**
   - Open Settings in plugin
   - Verify "Use proxy mesh" is checked

2. **File exists:**
   ```
   M:\Proiectare\...\ProductName\ProductName_proxy_mesh.3dm
   ```

3. **Logs show:**
   ```
   ✓ Using PROXY mesh for viewport
   ```

If you see "Proxy mesh not found", the product doesn't have a proxy mesh file yet.

---

## 🎯 What Changed in v1.0.1

**Before (v1.0.0):**
- Script assumed it ran from repository root where .sln was located
- After moving to rhino-plugin/, paths were broken

**After (v1.0.1 - Fixed):**
- Script detects its own location
- Navigates to rhino-plugin root automatically
- Works from any starting directory

**Changes:**
```powershell
# Added path detection
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rhinoPluginRoot = Split-Path -Parent $scriptDir
Set-Location $rhinoPluginRoot

# Updated build commands
dotnet clean BoschMediaBrowser.sln  # Now uses solution file
```

---

## 📝 Next Steps

After successful installation:

1. ✅ Test basic insertion (without proxy mesh)
2. ✅ Enable proxy mesh setting
3. ✅ Test with proxy mesh (if available for product)
4. ✅ Compare performance (viewport FPS, insertion speed)
5. ✅ Verify block naming is correct

---

**Version:** 1.0.1  
**Date:** October 14, 2025  
**Status:** ✅ Script Fixed & Tested
