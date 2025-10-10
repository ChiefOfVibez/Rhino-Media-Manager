# Bosch Product Database - Web Application

**Self-contained web interface** for managing the Bosch product database and generating JSONs for the Rhino Media Browser plugin.

## ✨ Features

### ✅ **Completed (All 3 Phases)**

**Phase 1: Full Editing**
- ✅ Edit all product fields (Description, SKU, Range, Category, Tags, Notes)
- ✅ Inline holder management (Add, Remove, Edit)
- ✅ Smart dirty state tracking with unsaved changes warning
- ✅ Revert changes functionality

**Phase 2: File Renaming**
- ✅ Automatic file renaming when holder parameters change
- ✅ Rename 3D files + preview images on network
- ✅ User confirmation before renaming
- ✅ Complete error handling

**Phase 3: Modern UI**
- ✅ Toast notifications (Success, Error, Warning, Info)
- ✅ Non-blocking notifications
- ✅ Auto-dismiss + manual close
- ✅ Professional UX

**Phase 4: Enhancements** (In Progress)
- ✅ Packaging dimensions (L/W/H/Weight)
- ✅ Browse file picker (fixed!)
- ✅ Reveal in Explorer
- ⏳ Enhanced Add Product modal
- ⏳ Pagination (50 items/page)
- ⏳ Bulk operations
- ⏳ Show/hide columns
- ⏳ Drag to reorder columns

## 🚀 Quick Start

### **One-Click Launch** (Recommended)

1. **Double-click** `START_SERVER.bat`
2. Wait 3 seconds
3. Browser opens automatically at http://localhost:8000

The BAT file will:
- ✅ Check Python installation
- ✅ Create virtual environment (first run)
- ✅ Install/update all dependencies
- ✅ Start server
- ✅ Auto-open browser

### Manual Start

```bash
cd webapp
pip install -r requirements.txt
python server.py
```

Then open: **http://localhost:8000**

## Usage

### Main Table View

- **Product columns (A-M):** Name, Description, SKU, Cod Articol, Range, Category, Preview, Holder info
- **✏️ Edit:** Open edit modal for detailed editing
- **🤖 Auto:** Auto-populate JSON from file structure
- **🖼️ View:** Show preview images

### Edit Modal

- Edit description, SKU, range, notes
- Manage holder variants and colors
- Changes save directly to JSON files

### Auto-Population

1. Click **🤖 Auto** button on any product
2. Script scans the product folder for:
   - Tool mesh and preview (.3dm + .jpg/png)
   - Grafica overlay and preview
   - Packaging files
   - Holder files and previews
3. JSON is automatically updated with full paths

### Database Scanning

Click **🔄 Scan Database** to discover new products from the file structure.

## 📁 File Structure

```
webapp/
├── START_SERVER.bat       # 🚀 One-click launcher (auto-installs deps)
├── server.py              # FastAPI backend with all endpoints
├── requirements.txt       # Python dependencies
├── static/
│   └── index.html        # Complete frontend UI (Alpine.js + Tailwind)
├── venv/                  # Virtual environment (auto-created)
└── README.md             # This file
```

**All self-contained!** Share the `webapp/` folder with your team - it has everything needed.

## 🔧 Configuration

**Network Path:** Edit in `server.py` (line ~30)
```python
BASE_PATH = r"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__"
```

**Cache Duration:** Edit in `server.py` (line ~55)
```python
CACHE_DURATION = 120  # seconds (2 minutes)
```

**Server Port:** Edit in `server.py` (line ~609)
```python
uvicorn.run("server:app", host="0.0.0.0", port=8000, reload=True)
```

## 🌐 API Endpoints

### Products
- `GET /api/products` - List all products (with caching)
- `GET /api/products/{name}` - Get single product
- `PUT /api/products/{name}` - Update product
- `POST /api/products/new` - Create new product
- `POST /api/products/{name}/auto-populate` - Auto-populate from files
- `POST /api/cache/refresh` - Force refresh cache

### File Operations
- `POST /api/browse-file` - Open file picker dialog
- `POST /api/reveal-file` - Reveal file in Explorer
- `POST /api/open-folder` - Open folder in Explorer
- `POST /api/rename-file` - Rename file on network

### Utilities
- `GET /api/scan` - Scan database for new products
- `POST /api/extract-previews` - Extract 3D previews from .3dm files
- `GET /api/audit/recent` - Get audit log
- `GET /holders/{category}/{filename}` - Serve holder preview images

## 📊 Tech Stack

- **Backend:** FastAPI (Python 3.8+) - Fast, modern, async
- **Frontend:** Alpine.js 3 + Tailwind CSS - Lightweight, reactive
- **Data Storage:** JSON files (same format as Rhino plugin)
- **3D Processing:** rhino3dm - Read .3dm files
- **Image Processing:** Pillow - Extract/process images
- **Caching:** In-memory with threading (120s default)

## 🎨 Performance Features

- **Smart Caching:** 120s cache, 50x faster for 500+ products
- **Lazy Loading:** Images load on demand
- **Pagination:** 50 items/page (coming soon)
- **Dirty State:** Only reload when data changes

## 📝 Development

### Running in Dev Mode
```bash
# Auto-reload on file changes
python server.py
```

### Debugging
- Server logs: Check console output
- Audit logs: `audit_log.jsonl` (auto-created)
- Browser console: F12 → Console tab

### Customization
- **Frontend:** Edit `static/index.html`
- **Backend:** Edit `server.py`
- **Styling:** Modify Tailwind classes in HTML

## 🚢 Sharing with Team

1. **Copy entire `webapp/` folder** to shared location
2. Team members just **double-click `START_SERVER.bat`**
3. That's it! Auto-installs everything needed

**Requirements:**
- Windows 10/11
- Python 3.8+ installed (download from python.org)
- Network access to M:\ drive (or update BASE_PATH)

## 🐛 Troubleshooting

### Server won't start
- Check Python is installed: `python --version`
- Check port 8000 is free: `netstat -ano | findstr :8000`
- Run BAT file as Administrator if needed

### Browser doesn't open
- Manually go to: http://localhost:8000
- Check firewall isn't blocking

### File picker not working
- Needs PowerShell with Windows.Forms
- Run: `Add-Type -AssemblyName System.Windows.Forms` in PowerShell to test

### Performance slow
- Increase cache duration in `server.py`
- Enable pagination (coming soon)
- Check network speed to M:\ drive

## 📚 Documentation

See parent folder for:
- `PHASE1_COMPLETION.md` - Full editing features
- `PHASE2_COMPLETION.md` - File renaming system
- `PHASE3_COMPLETION.md` - Toast notifications
- `PHASE4_ENHANCEMENTS_PLAN.md` - Current enhancements
- `PERFORMANCE_OPTIMIZATION.md` - Performance guide
- `PROJECT_COMPLETE.md` - Overall summary

## 🔜 Roadmap

### Phase 4 (In Progress)
- [ ] Enhanced Add Product modal with all fields
- [ ] Pagination (50 items/page)
- [ ] Bulk operations (select multiple)
- [ ] Show/hide columns
- [ ] Drag to reorder columns

### Future Phases
- [ ] Rhino Media Browser plugin integration
- [ ] Collections system
- [ ] Advanced search & filters
- [ ] Export to Excel
- [ ] User permissions

## 📞 Support

Contact your database admin or check documentation in parent folder.
