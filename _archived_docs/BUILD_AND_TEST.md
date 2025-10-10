# Build & Test Guide - Rhino Plugin

## Quick Start: Test with Real Data

### Prerequisites
✅ Visual Studio 2022 (or VS Code with C# extension)  
✅ .NET 7 SDK installed  
✅ Rhino 8 installed  
✅ Access to M:\ drive with product data

---

## Step 1: Build the Solution

### Option A: Visual Studio
1. Open `BoschMediaBrowser.sln` in Visual Studio
2. Set build configuration to **Debug**
3. Build the solution: **Build → Build Solution** (Ctrl+Shift+B)
4. Check Output window for success

### Option B: Command Line
```powershell
# Navigate to solution directory
cd "e:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel"

# Restore packages
dotnet restore

# Build solution
dotnet build BoschMediaBrowser.sln --configuration Debug
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Step 2: Locate the Built Plugin

After building, the plugin will be at:
```
src\BoschMediaBrowser.Rhino\bin\Debug\net7.0\BoschMediaBrowser.Rhino.rhp
```

---

## Step 3: Load Plugin in Rhino

### Method 1: Drag & Drop (Easiest)
1. Open **Rhino 8**
2. Drag the `.rhp` file into Rhino viewport
3. Click **Load** when prompted
4. Plugin loads and registers automatically

### Method 2: PlugInManager Command
1. Open **Rhino 8**
2. Type command: `PlugInManager`
3. Click **Install**
4. Browse to: `src\BoschMediaBrowser.Rhino\bin\Debug\net7.0\BoschMediaBrowser.Rhino.rhp`
5. Click **Open**

### Method 3: Development Mode (Best for Testing)
1. In Rhino, type: `TestToggleDevPlugIn`
2. Browse to the `.rhp` file
3. Click **Open**
4. Plugin loads WITHOUT registration (perfect for dev)

---

## Step 4: Open the Media Browser Panel

Once loaded, run the command:
```
ShowMediaBrowser
```

**What should happen:**
- ✅ Panel opens on the right side of Rhino
- ✅ "Loading products..." message appears
- ✅ Status bar shows product count after loading
- ✅ Search box is active

---

## Step 5: Verify Data Loading

### Check Console Output
Look in Rhino's **Command History** for messages like:
```
BoschMediaBrowser plugin loaded successfully
Loading products from: M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__
Loaded X products
```

### Test Search
1. Type in the search box
2. Check console for search results

### Check Settings File
Settings are saved to:
```
%AppData%\BoschMediaBrowser\settings.json
```

Open this file to verify:
```json
{
  "BaseServerPath": "M:\\Proiectare\\__SCAN 3D Produse\\__BOSCH\\__NEW DB__",
  "LinkedInsertDefault": true,
  "GridSpacing": 1200.0,
  "ThumbnailSize": 192
}
```

---

## Step 6: Test with Real Data

### Verify Products Load
1. Check if JSON files exist at: `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\**\*.json`
2. Panel should show: "Loaded X products" in status bar
3. Try searching for a known product name

### Verify Category Derivation
Products should be categorized by folder structure:
- Products in `DIY\` → range = "DIY"
- Products in `PRO\` → range = "PRO"  
- Products in `Tools and Holders\DIY\` → topCategory = "Tools and Holders", range = "DIY"

### Test Auto-Reload
1. Edit a JSON file in the data folder
2. Save it
3. Panel should auto-reload (FileSystemWatcher)
4. Check console for "Reloading products..." message

---

## Troubleshooting

### Plugin Won't Load
- ✅ Check .NET 7 SDK is installed: `dotnet --version`
- ✅ Rebuild in Visual Studio (clean + build)
- ✅ Check Rhino version is 8.x
- ✅ Look for errors in Rhino Command History

### No Products Found
- ✅ Verify M:\ drive is accessible
- ✅ Check path: `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__`
- ✅ Verify JSON files exist in subdirectories
- ✅ Check file permissions

### Panel Shows Error
- ✅ Check Rhino Command History for stack traces
- ✅ Verify all dependencies installed (RhinoCommon, Eto.Forms)
- ✅ Check settings.json for valid paths

### Build Errors
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build --configuration Debug
```

---

## Development Workflow

### Hot Reload (Quick Testing)
1. Make code changes
2. **Build** (Ctrl+Shift+B)
3. In Rhino, type: `TestToggleDevPlugIn` (unload)
4. Type: `TestToggleDevPlugIn` (reload with new build)
5. Type: `ShowMediaBrowser`

### Debug with Visual Studio
1. Set `BoschMediaBrowser.Rhino` as startup project
2. Right-click project → **Properties**
3. Debug tab:
   - Launch: **Executable**
   - Executable path: `C:\Program Files\Rhino 8\System\Rhino.exe`
4. Press **F5** to debug
5. Rhino launches with debugger attached

---

## Next Steps After Validation

Once data loads correctly:
1. ✅ Verify category derivation works
2. ✅ Test search/filter functions
3. ✅ Check thumbnail caching
4. ✅ Proceed to build UI controls (T017-T020)

---

## Quick Command Reference

```bash
# Build
dotnet build

# Run tests
dotnet test

# Clean build
dotnet clean && dotnet build
```

**Rhino Commands:**
- `ShowMediaBrowser` - Open/close panel
- `PlugInManager` - Manage plugins
- `TestToggleDevPlugIn` - Dev mode load/unload

---

**Ready to test! Follow steps 1-6 above.** 🚀
