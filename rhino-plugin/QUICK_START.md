# Quick Start Guide

## 🚀 Installation (3 Steps)

### 1. Run Installer

```powershell
.\scripts\Verify-And-Install.bat
```

**This rebuilds and installs automatically!**

### 2. Start Rhino

Close any open Rhino instances, then start Rhino 8.

### 3. Open Plugin

```
ShowMediaBrowser
```

---

## ⚙️ First-Time Setup

### Configure Database Path

1. Click **Settings** (⚙️ gear icon)
2. Set path to: `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders`
3. Click **Test Connection** → Should show ✅ green checkmark
4. Click **Save**

### Optional: Enable Proxy Meshes

For better performance:

1. In Settings dialog
2. Check ✅ **"Use proxy mesh for viewport (lighter display)"**
3. Click **Save**

---

## 📝 Basic Usage

### Insert a Product

1. **Browse** - Click category (e.g., "Garden")
2. **Preview** - Click product card (e.g., "GBL 18V-750")
3. **Configure**:
   - Select holder: "Traverse - RAL9006"
   - Check packaging if needed
4. **Insert** - Click insert button
5. **Pick point** in Rhino viewport

**Done!** Product inserted with correct holder transform.

---

## 🎯 Block Types

When inserted, you get **3 linked blocks**:

```
✓ Tool:      GBL 18V-750_Mesh_Tego_Traverse_RAL9006
✓ Holder:    Traverse_RAL9006_NN.ALL.BO07803
✓ Packaging: GBL 18V-750_packaging (if selected)
```

All blocks are **linked** to source files!

---

## 🔍 Troubleshooting

### Plugin won't load

```
1. Check: %APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser\
2. Verify files exist: BoschMediaBrowser.rhp, BoschMediaBrowser.Core.dll
3. Restart Rhino
```

### No products showing

```
1. Open Settings
2. Verify path includes "Tools and Holders"
3. Click Test Connection
4. Check Rhino command window for errors
```

### Wrong geometry

```
1. Open BlockManager
2. Delete all existing blocks
3. Re-insert (forces fresh block creation)
```

---

## 📖 More Help

- **Full documentation:** README.md
- **Proxy mesh guide:** docs/PROXY_MESH_GUIDE.md
- **Version history:** CHANGELOG.md
- **Milestone info:** MILESTONE_v1.0.0.md

---

**Version:** 1.0.0  
**Last Updated:** Oct 14, 2025
