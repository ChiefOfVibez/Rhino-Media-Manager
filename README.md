# Bosch Media Browser - Rhino Plugin

A powerful media browser plugin for Rhino 8, similar to V-Ray Chaos Cosmos, designed for browsing and inserting Bosch product models into Rhino projects.

## 🚀 Quick Start

### Installation

1. **Run the installer:**
   ```batch
   Install-Plugin.bat
   ```
   
2. **Restart Rhino** if it's running

3. **Open the panel:**
   - Run command: `ShowMediaBrowser`

For detailed instructions, see [INSTALLATION.md](INSTALLATION.md)

## 📋 Features

### Browse Products
- **Category Tree** - Navigate products by category hierarchy
- **Advanced Filtering** - Filter by range (DIY/PRO), category, holder variants, tags
- **Search** - Full-text search across product names, SKUs, descriptions
- **Thumbnail Grid** - Visual product browser with pagination
- **Detail View** - Product specs, holders, tags, preview images

### Organize
- **Favourites** - Quick access to frequently used products
- **Collections** - Create custom product groups
- **Tags** - Add custom tags for better organization

### Insert (Coming in Phase 2)
- Single or multiple product insertion
- Linked file workflow
- Grid-based layout
- Holder variant selection

## 📁 Project Structure

```
Bosch Products DB in excel/
├── src/                          # Source code
│   ├── BoschMediaBrowser.Core/   # Core services & models
│   └── BoschMediaBrowser.Rhino/  # Rhino plugin & UI
├── tests/                        # Unit tests
├── docs/                         # Documentation
├── BoschMediaBrowserSpec/        # Specifications
│
├── INSTALLATION.md               # Installation guide
├── UI_IMPLEMENTATION_COMPLETE.md # Current status
├── Database Instructions.md      # Product database guide
│
├── Install-Plugin.bat            # Quick installer (run this)
├── Install-Plugin.ps1            # PowerShell installer script
│
└── _archived_docs/               # Old planning/status docs
```

## 🔧 Development

### Prerequisites
- .NET 7.0 SDK
- Rhino 8 for Windows
- Visual Studio 2022 or JetBrains Rider

### Building

```bash
dotnet build BoschMediaBrowser.sln --configuration Release
```

### Running Tests

```bash
dotnet test
```

## 📊 Product Database Structure

Products are stored in a folder hierarchy:

```
Database Root/
├── Tools and Holders/
│   ├── DIY/
│   │   ├── Garden/
│   │   │   └── ProductName/
│   │   │       ├── product.json          # Product metadata
│   │   │       ├── MeshPreview.png       # Thumbnail
│   │   │       ├── ProductName_*.3dm     # Holder files
│   │   │       └── ...
│   │   └── ...
│   └── PRO/
│       └── ...
└── _public-collections/          # Hidden from categories
```

### Product JSON Format

```json
{
  "productName": "Product Name",
  "sku": "SKU12345",
  "description": "Product description",
  "range": "DIY",
  "category": "Garden",
  "holders": [
    {
      "variant": "Variant Name",
      "color": "Black",
      "fileName": "ProductName_Variant.3dm",
      "fullPath": "path/to/file.3dm"
    }
  ],
  "tags": ["tag1", "tag2"]
}
```

## 🎯 Current Status

**Phase 1: Complete ✅**
- Core services implemented
- Full UI with Eto.Forms
- Browse, Favourites, Collections views
- Zero compilation errors

**Phase 2: Planned**
- Product insertion logic
- Settings panel UI
- Image loading & display
- Advanced features

## 📝 Configuration

User data stored in:
```
%APPDATA%\BoschMediaBrowser\
├── settings.json      # User preferences
├── userdata.json      # Favourites, tags, collections
└── ThumbnailCache\    # Cached preview images
```

## 🐛 Troubleshooting

### Plugin doesn't load
- Check `PlugInManager` in Rhino
- Look for error messages in Rhino's command window
- Verify .NET 7.0 is installed

### No products showing
- Configure Base Server Path in plugin settings
- Ensure JSON files exist in product folders
- Click Refresh button

See [INSTALLATION.md](INSTALLATION.md) for more troubleshooting tips.

## 📚 Documentation

- **[INSTALLATION.md](INSTALLATION.md)** - Complete installation guide
- **[UI_IMPLEMENTATION_COMPLETE.md](UI_IMPLEMENTATION_COMPLETE.md)** - Current implementation status
- **[Database Instructions.md](Database Instructions.md)** - Product database setup
- **[_archived_docs/](_archived_docs/)** - Historical planning documents

## 🏗️ Architecture

### Services Layer
- **DataService** - JSON loading, category tree building
- **SearchService** - Filtering, sorting, searching
- **SettingsService** - User preferences
- **UserDataService** - Favourites, tags, collections
- **ThumbnailService** - Preview caching

### UI Layer (Eto.Forms)
- **MediaBrowserPanel** - Main dockable panel
- **CategoryTree** - TreeGrid navigation
- **FiltersBar** - Multi-criteria filters
- **ThumbnailGrid** - Paginated product view
- **DetailPane** - Product details
- **FavouritesView** - Favourites management
- **CollectionsView** - Collections management

## 📄 License

Internal tool for Bosch product database management.

## 🤝 Support

For issues or questions, contact the development team.

---

**Version:** 1.0.0  
**Target:** Rhino 8 on Windows (.NET 7)  
**Last Updated:** October 2025
