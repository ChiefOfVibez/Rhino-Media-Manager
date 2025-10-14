# Bosch Media Browser - Rhino Plugin

**A modern media browser plugin for Rhino 8** that provides seamless access to Bosch product libraries with intelligent holder transforms, linked block support, and packaging insertion.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Rhino](https://img.shields.io/badge/Rhino-8-green)
![.NET](https://img.shields.io/badge/.NET-7.0-purple)

## 🎯 Features

### Core Functionality
- ✅ **Smart Product Browser** - Browse DIY and PRO ranges with category filtering
- ✅ **Linked Block Support** - True linked blocks that update when source files change
- ✅ **Intelligent Transforms** - Automatic holder-based tool positioning
- ✅ **Multi-Insert** - Insert tool + holder + packaging in one action
- ✅ **Material Preservation** - Materials imported and mapped correctly
- ✅ **Preview System** - Visual product cards with thumbnails

### Insertion Options
- **Block Type**: Linked, Embedded, or LinkedAndEmbedded
- **Insert Mode**: Block Instance or Group (exploded)
- **Layer Style**: Active or Reference
- **Transform**: Automatic holder-based positioning

### Product Structure
Products support multiple holders with unique transforms:
```json
{
  "ProductName": "GBL 18V-750",
  "ReferenceHolder": "Tego",
  "HolderTransforms": {
    "Tego": { "Translation": [0, 0, 0], "Rotation": [0, 0, 0] },
    "Traverse": { "Translation": [0, -44, -89], "Rotation": [-33, 0, 0] }
  }
}
```

## 📦 Installation

### Quick Install (Recommended)

1. **Run the installer:**
   ```powershell
   .\scripts\Verify-And-Install.bat
   ```

This script will:
- ✅ Clean and rebuild the plugin
- ✅ Check Rhino status
- ✅ Reset panel state
- ✅ Install to Rhino 8 plugins directory
- ✅ Verify installation

### Manual Build

```powershell
dotnet clean
dotnet build src\BoschMediaBrowser.Rhino\BoschMediaBrowser.Rhino.csproj --configuration Release
```

Then copy files from `src\BoschMediaBrowser.Rhino\bin\Release\net7.0\` to:
```
%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser\B05C43D0-ED1A-4B05-8E12-345678ABCDEF\
```

## 🚀 Usage

### First Launch

1. Start Rhino 8
2. Run command: `ShowMediaBrowser`
3. Configure database path in Settings (⚙️ icon)
4. Set path to: `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders`

### Inserting Products

1. **Browse** - Select category (Garden, Drills, etc.)
2. **Preview** - Click product card to see details
3. **Configure**:
   - Select holder variant
   - Toggle packaging checkbox
4. **Insert** - Click insert button and pick point in Rhino

### Block Naming Convention

Blocks are named using the source filename for clarity:

```
Tool:      GBL 18V-750_Mesh_Tego_Traverse_RAL9006
Holder:    Traverse_RAL9006_NN.ALL.BO07803
Packaging: GBL 18V-750_packaging
```

## 📁 Project Structure

```
rhino-plugin/
├── src/
│   ├── BoschMediaBrowser.Core/      # Core business logic
│   │   ├── Models/                  # Data models (Product, Holder, Settings)
│   │   ├── Services/                # Services (Data, Settings)
│   │   └── BoschMediaBrowser.Core.csproj
│   │
│   └── BoschMediaBrowser.Rhino/     # Rhino plugin
│       ├── UI/                      # Eto.Forms UI components
│       │   ├── Controls/            # Reusable controls
│       │   ├── Dialogs/             # Settings, About dialogs
│       │   └── MediaBrowserPanel.cs # Main panel
│       ├── BoschMediaBrowserPlugin.cs
│       └── BoschMediaBrowser.Rhino.csproj
│
├── scripts/
│   ├── Verify-And-Install.bat       # Main installer (with rebuild)
│   ├── Verify-And-Install.ps1       # PowerShell install script
│   ├── Install-Plugin.bat           # Install without rebuild
│   └── Clean-Install.bat            # Clean install
│
├── docs/
│   └── (future documentation)
│
├── BoschMediaBrowser.sln            # Visual Studio solution
├── README.md                        # This file
└── .gitignore                       # Git ignore patterns
```

## 🔧 Configuration

### Database Path Structure

Expected folder structure:
```
[BasePath]/Tools and Holders/
├── DIY/
│   ├── Garden/
│   ├── Drills/
│   └── Measuring/
├── PRO/
│   ├── Garden/
│   │   └── GBL 18V-750/
│   │       ├── GBL 18V-750.json
│   │       ├── GBL 18V-750_Mesh_Tego.3dm
│   │       ├── GBL 18V-750_packaging.3dm
│   │       └── preview.png
│   └── Drills/
└── Holders/
    ├── Garden/
    │   ├── Tego_RAL7043_BO.161.9LL8600.3dm
    │   └── Traverse_RAL9006_NN.ALL.BO07803.3dm
    └── Drills/
```

### Settings File Location

Settings are stored at:
```
%APPDATA%\BoschMediaBrowser\settings.json
```

Example:
```json
{
  "BaseServerPath": "M:\\...\\Tools and Holders",
  "InsertBlockType": "Linked",
  "InsertAs": "BlockInstance",
  "InsertLayerStyle": "Active",
  "InsertReadLinkedBlocks": true
}
```

## 🎨 Features in Detail

### Linked Blocks

The plugin creates **true linked blocks** using Rhino's `InstanceDefinitionUpdateType.LinkedAndEmbedded`:

- Blocks reference source .3dm files
- Updates automatically when source files change
- Embedded geometry as fallback if source unavailable
- Visible in BlockManager with source file path

### Holder Transforms

Products define transforms for each compatible holder:

```csharp
// Example: Traverse holder requires rotation and translation
Translation: [0, -44, -89]   // mm
Rotation: [-33, 0, 0]        // degrees (X, Y, Z)
Scale: [1, 1, 1]             // uniform
```

The plugin:
1. Reads tool mesh (reference holder position)
2. Applies transform to move tool to selected holder
3. Inserts holder at origin (stationary)
4. Inserts packaging offset +500mm on X-axis

### Material Handling

Materials are:
- Imported from source .3dm files
- Checked for duplicates (by name)
- Mapped to document materials
- Preserved on all geometry

## 🐛 Troubleshooting

### Plugin Not Loading

Check Rhino command window for:
```
=== Bosch Media Browser Plugin Loading ===
Plugin GUID: B05C43D0-ED1A-4B05-8E12-345678ABCDEF
```

If not appearing, verify installation at:
```
%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser\
```

### Products Not Showing

1. Check database path in Settings
2. Click "Test Connection" - should show green checkmark
3. Verify .json files exist in product folders
4. Check Rhino command output for loading errors

### Linked Blocks Not Working

Ensure settings has:
- ✅ "Read linked blocks from the file" checked
- ✅ "Block Definition Type" = Linked

Verify in BlockManager:
- Right-click block → Properties
- Should show "Type: Linked and Embedded"
- Should show source file path

### Wrong Geometry Inserted

Delete all blocks in BlockManager and re-insert. The plugin caches block definitions, so old blocks may be reused.

## 🔄 Development Workflow

### Making Changes

1. Edit source code in `src/`
2. Run installer (automatically rebuilds):
   ```powershell
   .\scripts\Verify-And-Install.bat
   ```
3. **Close Rhino completely**
4. Restart Rhino
5. Test changes

### Debugging

Open solution in Visual Studio:
```powershell
.\BoschMediaBrowser.sln
```

Set breakpoints and press **F5** to debug in Rhino.

## 📊 Technical Details

### Dependencies

- **Rhino 8** (RhinoCommon)
- **.NET 7.0**
- **Eto.Forms** (cross-platform UI)
- **System.Text.Json** (JSON parsing)

### Performance

- Lazy loading of products (only visible items)
- Cached thumbnails
- Minimal file I/O during browsing
- Block definition reuse

## 🎯 Roadmap

- [ ] Proxy mesh support
- [ ] Multiple product selection
- [ ] Batch insert with array
- [ ] Custom collections
- [ ] Favorites system
- [ ] Tag-based filtering
- [ ] Search by SKU/name
- [ ] Recent products history

## 📝 License

Internal tool for Bosch product management.

## 🤝 Support

For issues or questions, check:
- Rhino command output for error messages
- Settings file at `%APPDATA%\BoschMediaBrowser\settings.json`
- Installation directory for file integrity

---

**Last Updated:** October 14, 2025  
**Version:** 1.0.0 (Milestone Release - Linked Blocks Working!)
