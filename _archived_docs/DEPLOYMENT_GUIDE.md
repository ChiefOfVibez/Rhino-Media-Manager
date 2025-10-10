# 🚀 Deployment Guide - Share Webapp with Team

**Last Updated:** 2025-10-07  
**Status:** Ready for team distribution

---

## ✅ What's Complete

### Phase 4A: Core Fixes & Enhancements
- ✅ **Browse file picker FIXED** (PowerShell -STA mode)
- ✅ **Reveal in Explorer** works correctly
- ✅ **Packaging dimensions** added to products
- ✅ **Auto-dependency installer** in BAT file
- ✅ **Virtual environment** auto-created
- ✅ **Browser auto-launch** on server start
- ✅ **Self-contained webapp folder** - everything needed in one place

---

## 📦 Distribution Package

### What to Share

**Share ONLY the `webapp/` folder** - it's completely self-contained:

```
webapp/
├── START_SERVER.bat       # One-click launcher
├── server.py              # Backend (all features)
├── requirements.txt       # Dependencies
├── static/
│   └── index.html        # Frontend UI
└── README.md             # Complete documentation
```

### Requirements for Team Members

1. **Windows 10/11**
2. **Python 3.8+** (download from python.org)
3. **Network access** to M:\ drive (Bosch products)

That's it! No manual dependency installation needed.

---

## 🎯 Quick Start for Team

### Step 1: Copy Folder
```
Copy webapp/ folder to their machine
Location doesn't matter (Desktop, Documents, etc.)
```

### Step 2: Launch
```
Double-click START_SERVER.bat
Wait 3 seconds
Browser opens automatically at http://localhost:8000
```

### What Happens Automatically

The BAT file will:
1. ✅ Check if Python is installed
2. ✅ Create virtual environment (first run only)
3. ✅ Install all dependencies (pip install -r requirements.txt)
4. ✅ Kill any existing server on port 8000
5. ✅ Start FastAPI server
6. ✅ Open browser to http://localhost:8000

**First run:** Takes ~1-2 minutes (installing deps)  
**Subsequent runs:** Takes ~5 seconds (deps already installed)

---

## 🔧 Configuration

### Change Network Path

Edit `server.py` line ~30:
```python
BASE_PATH = r"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__"
```

Change to your network path, then restart server.

### Change Cache Duration

Edit `server.py` line ~55:
```python
CACHE_DURATION = 120  # seconds (2 minutes)
```

For 500+ products, 120s is optimal. Adjust as needed.

### Change Server Port

Edit `server.py` line ~609:
```python
uvicorn.run("server:app", host="0.0.0.0", port=8000, reload=True)
```

Change `port=8000` to another port if 8000 is in use.

---

## 🧹 Project Cleanup (Optional)

The main project folder has been cleaned up. Run `CLEANUP_PROJECT.bat` to remove:

### Files Removed
- ❌ `__pycache__/` - Python cache
- ❌ `GBL 18V-750.json` - Example file
- ❌ `products.json` - Old cache
- ❌ `audit_log.jsonl` - Moved to webapp
- ❌ `autopop_product_json.bat/py` - Now in webapp
- ❌ `extract_previews.bat/py` - Now in webapp
- ❌ `scan_database.bat` - Now in webapp
- ❌ `TEMPLATE_minimal.json` - Not needed

### Files Kept
- ✅ `webapp/` - **Main application**
- ✅ `BoschMediaBrowserSpec/` - **Plugin specs (for Phase 5)**
- ✅ `Bosch_3D_ProductDatabase_clean.xlsm` - **Excel source**
- ✅ `_archived_excel_approach/` - **Archived work**
- ✅ `bosch_scanner.py` - **Utility script**
- ✅ `*.md` files - **All documentation**

---

## 🎨 Features Available

### ✅ All Implemented (Phases 1-3)

**Editing:**
- Edit all product fields
- Add/remove/edit holders
- Dirty state tracking
- Revert changes

**File Management:**
- Auto-populate from file structure
- Automatic file renaming on parameter changes
- Browse for preview images
- Reveal files in Explorer
- Extract 3D previews

**UI/UX:**
- Toast notifications (success/error/warning/info)
- Smart caching (2 minute cache)
- Search and filter products
- Sort by any column
- Preview images
- Packaging dimensions (L/W/H/Weight)

### ⏳ Coming Soon (Phase 4B)

- Pagination (50 items/page) - **Performance boost**
- Bulk operations (select multiple products)
- Enhanced Add Product modal (all fields)
- Show/hide columns
- Drag to reorder columns

---

## 🐛 Troubleshooting

### Issue: "Python not found"
**Solution:** Install Python from python.org, then try again

### Issue: "Dependencies failed to install"
**Solution:** 
```bash
cd webapp
pip install -r requirements.txt
```
Check error message, may need admin rights

### Issue: "Port 8000 already in use"
**Solution:**
```bash
# Find process using port 8000
netstat -ano | findstr :8000

# Kill process (replace PID)
taskkill /F /PID <PID>
```

### Issue: "Browser doesn't open"
**Solution:** Manually go to http://localhost:8000

### Issue: "File picker doesn't open"
**Solution:** 
- Check PowerShell is available
- Run as Administrator
- Test: `powershell -Command "Add-Type -AssemblyName System.Windows.Forms"`

### Issue: "Can't access M:\ drive"
**Solution:** 
- Map network drive to M:\
- Or edit BASE_PATH in server.py to your path

---

## 📊 Performance Tips

### For 500+ Products
- ✅ Cache duration: 120s (optimal)
- ✅ Use 🔃 Refresh button instead of page reload
- ✅ Enable pagination when available (coming soon)

### For 1000+ Products
- ⚙️ Increase cache to 180s (3 min)
- ⚙️ Enable pagination
- ⚙️ Consider SQLite backend (future)

---

## 📚 Documentation

All documentation is in the main project folder:

| File | Purpose |
|------|---------|
| `webapp/README.md` | **Complete webapp guide** |
| `PHASE1_COMPLETION.md` | Full editing features |
| `PHASE2_COMPLETION.md` | File renaming system |
| `PHASE3_COMPLETION.md` | Toast notifications |
| `PHASE4_ENHANCEMENTS_PLAN.md` | Current/future features |
| `PERFORMANCE_OPTIMIZATION.md` | Performance tuning |
| `PROJECT_COMPLETE.md` | Overall summary |
| `DEPLOYMENT_GUIDE.md` | **This file** |

---

## 🔜 Next Phase: Rhino Plugin

After Phase 4B is complete (pagination, bulk ops, etc.), we'll plan and develop:

**Rhino Media Browser Plugin** (C# / RhinoCommon)
- Based on specs in `BoschMediaBrowserSpec/`
- Eto.Forms dockable panel
- Same JSON format as webapp
- Insert linked files into Rhino
- Search, filter, collections
- Similar to V-Ray Chaos Cosmos

**Planning methodology:** Spec Kit approach
- Detailed specifications first
- Architecture design
- Systematic implementation
- Full documentation

---

## ✅ Checklist Before Sharing

- [ ] Test START_SERVER.bat on clean machine
- [ ] Verify all dependencies install correctly
- [ ] Test on team member's computer
- [ ] Ensure M:\ drive is accessible
- [ ] Share webapp/ folder via network/USB/cloud
- [ ] Provide this deployment guide
- [ ] Quick demo session (5 minutes)

---

## 🎉 Success Metrics

**Deployment is successful when:**
- ✅ Team member runs BAT file
- ✅ Browser opens automatically
- ✅ Can browse products
- ✅ Can edit and save products
- ✅ Auto-populate works
- ✅ File operations work (browse, reveal)

**Total setup time:** < 5 minutes for users  
**No manual configuration needed!**

---

## 📞 Support

**For technical issues:**
1. Check `webapp/README.md` troubleshooting section
2. Check server console for error messages
3. Check `audit_log.jsonl` for operation history
4. Contact database admin

**For feature requests:**
- Document in GitHub/internal tracker
- Will be prioritized for Phase 4B/5

---

**Ready to deploy! Share the `webapp/` folder and let your team enjoy the modern interface! 🚀**
