# Rhino Media Manager

**A comprehensive media management system for Bosch product libraries in Rhino 8.**

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Rhino](https://img.shields.io/badge/Rhino-8-green)
![.NET](https://img.shields.io/badge/.NET-7.0-purple)
![Status](https://img.shields.io/badge/status-production%20ready-brightgreen)

## 📦 What's Included

This repository contains two main components:

### 1. **Rhino Plugin** - Media Browser for Rhino 8
Complete Rhino plugin with linked block support, proxy meshes, and intelligent holder transforms.

**[📖 Plugin Documentation →](rhino-plugin/README.md)**

### 2. **Web Application** - Product Database Manager
Flask-based web interface for managing product metadata, generating JSONs, and previewing assets.

**[📖 Webapp Documentation →](webapp/README.md)**

---

## 🚀 Quick Start

### Rhino Plugin

```powershell
cd rhino-plugin
.\scripts\Verify-And-Install.bat
```

**[Full Installation Guide →](rhino-plugin/QUICK_START.md)**

### Web Application

```bash
cd webapp
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
python app.py
```

Open http://localhost:5000

## 🎯 Key Features

### Rhino Plugin

- ✅ **Linked Blocks** - True linked blocks that update when source files change
- ✅ **Proxy Mesh Support** - Lightweight meshes for viewport performance (94% polygon reduction)
- ✅ **Intelligent Transforms** - Automatic holder-based tool positioning
- ✅ **Multi-Insert** - Tool + Holder + Packaging in one operation
- ✅ **Material Handling** - Materials imported and preserved
- ✅ **Clean Block Naming** - Simple, filename-based naming convention

### Web Application

- ✅ **Product Browser** - Visual interface for product database
- ✅ **JSON Generation** - Automatic metadata generation
- ✅ **Holder Management** - Configure holder variants and transforms
- ✅ **Preview System** - 3D preview rendering
- ✅ **Batch Operations** - Process multiple products
- ✅ **Data Validation** - Ensures JSON integrity

## 📁 Repository Structure

```
rhino-media-manager/
├── rhino-plugin/              # Rhino 8 Plugin
│   ├── src/                   # C# source code
│   │   ├── BoschMediaBrowser.Core/      # Business logic
│   │   └── BoschMediaBrowser.Rhino/     # UI & Rhino integration
│   ├── scripts/               # Build & install scripts
│   ├── docs/                  # Plugin documentation
│   ├── tests/                 # Unit tests
│   ├── README.md              # Plugin docs
│   ├── CHANGELOG.md           # Version history
│   ├── QUICK_START.md         # Quick guide
│   └── MILESTONE_v1.0.0.md    # Milestone summary
│
├── webapp/                    # Flask Web Application
│   ├── app.py                 # Main application
│   ├── templates/             # HTML templates
│   ├── static/                # CSS, JS, images
│   ├── requirements.txt       # Python dependencies
│   └── README.md              # Webapp docs
│
├── docs/                      # Project Documentation
│   ├── guides/                # User guides
│   ├── fixes/                 # Bug fix documentation
│   ├── sessions/              # Dev session summaries
│   └── implementation/        # Technical specs
│
├── _archived_docs/            # Historical documents
├── _archived_excel_approach/  # Old Excel-based approach
│
├── README.md                  # This file
├── .gitignore                 # Git ignore patterns
├── Bosch_3D_ProductDatabase_clean.xlsm  # Excel database
└── bosch_scanner.py           # Database scanner utility
```

## 🔧 Development

### Prerequisites

**For Rhino Plugin:**
- .NET 7.0 SDK
- Rhino 8 for Windows
- Visual Studio 2022 (recommended for debugging)

**For Web Application:**
- Python 3.9+
- Flask and dependencies

### Building the Plugin

```powershell
cd rhino-plugin
dotnet clean
dotnet build src\BoschMediaBrowser.Rhino\BoschMediaBrowser.Rhino.csproj --configuration Release
```

Or use the automated script:
```powershell
cd rhino-plugin
.\scripts\Verify-And-Install.bat
```

### Running the Webapp

```bash
cd webapp
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
python app.py
```

## 📊 Product Database Structure

Products are organized in a hierarchical folder structure:

```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders/
├── DIY/
│   ├── Garden/
│   │   └── ProductName/
│   │       ├── ProductName.json              # Metadata
│   │       ├── ProductName_Mesh_Tego.3dm     # Full detail mesh
│   │       ├── ProductName_proxy_mesh.3dm    # Lightweight proxy
│   │       ├── ProductName_packaging.3dm     # Packaging
│   │       └── preview.png                   # Thumbnail
│   ├── Drills/
│   └── ...
├── PRO/
│   └── Garden/
│       ├── GBL 18V-750/
│       ├── GGP 12V-25/
│       └── ...
├── Holders/
│   ├── Garden/
│   │   ├── Tego_RAL7043_BO.161.9LL8600.3dm
│   │   ├── Traverse_RAL9006_NN.ALL.BO07803.3dm
│   │   └── ...
│   └── ...
└── _public-collections/              # Hidden from categories
```

**Key Files:**
- `{Product}.json` - Product metadata with holder transforms
- `{Product}_Mesh_{Holder}.3dm` - Full detail mesh for specific holder
- `{Product}_proxy_mesh.3dm` - Lightweight viewport mesh (optional)
- `{Product}_packaging.3dm` - Packaging 3D model

## 🎯 Status

### ✅ Version 1.0.0 - Production Ready (Oct 14, 2025)

**Rhino Plugin:**
- ✅ Linked blocks working perfectly
- ✅ Proxy mesh support implemented
- ✅ Holder transforms system complete
- ✅ Multi-insert (tool + holder + packaging)
- ✅ Settings persistence
- ✅ Material handling

**Web Application:**
- ✅ Product browser interface
- ✅ JSON generation
- ✅ Holder configuration
- ✅ Preview rendering
- ✅ Batch operations

**[📖 View Full Milestone →](rhino-plugin/MILESTONE_v1.0.0.md)**

---

## 📚 Documentation

### Quick Links

- **[Rhino Plugin Docs](rhino-plugin/README.md)** - Complete plugin documentation
- **[Quick Start Guide](rhino-plugin/QUICK_START.md)** - 3-step installation
- **[Proxy Mesh Guide](rhino-plugin/docs/PROXY_MESH_GUIDE.md)** - Performance optimization
- **[Changelog](rhino-plugin/CHANGELOG.md)** - Version history

### Additional Resources

- **[User Guides](docs/guides/)** - Installation, testing, database setup
- **[Bug Fixes](docs/fixes/)** - Documentation of resolved issues
- **[Dev Sessions](docs/sessions/)** - Development session summaries
- **[Implementation Details](docs/implementation/)** - Technical specifications

---

## 🐛 Troubleshooting

### Rhino Plugin

**Plugin doesn't load:**
- Verify installation at `%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser\`
- Check Rhino command window for error messages
- Restart Rhino completely

**No products showing:**
1. Open Settings (⚙️ icon)
2. Set database path to: `M:\...\Tools and Holders`
3. Click "Test Connection" - should show green ✅
4. Click Save and restart plugin

**See [rhino-plugin/README.md](rhino-plugin/README.md) for more troubleshooting**

---

## 🔗 Related Projects

- **Excel Database** - `Bosch_3D_ProductDatabase_clean.xlsm`
- **Scanner Utility** - `bosch_scanner.py` - Scans folders and generates JSONs

---

## 📄 License

Internal tool for Bosch product database management.

---

## 🤝 Contributing

For issues, feature requests, or questions:
1. Check existing documentation
2. Review [CHANGELOG.md](rhino-plugin/CHANGELOG.md)
3. Contact development team

---

**Version:** 1.0.0 - Production Ready  
**Platform:** Rhino 8 (Windows) + Flask (Python)  
**Last Updated:** October 14, 2025  
**Repository:** https://github.com/ChiefOfVibez/Rhino-Media-Manager
