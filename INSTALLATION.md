# Bosch Media Browser - Installation Guide

## Prerequisites

✅ **Rhino 8** for Windows  
✅ **.NET 7.0 SDK** installed  
✅ **Visual Studio 2022** or **Rider** (for development only)

## Installation

### Option 1: Automated Installation (Recommended)

1. **Build the plugin:**
   ```powershell
   dotnet build BoschMediaBrowser.sln --configuration Release
   ```

2. **Run the installer:**
   ```powershell
   .\Install-Plugin.ps1
   ```

3. **Restart Rhino** if it's running

### Option 2: Manual Installation

1. **Build the solution:**
   ```powershell
   dotnet build BoschMediaBrowser.sln --configuration Release
   ```

2. **Locate the built files:**
   - Plugin: `src\BoschMediaBrowser.Rhino\bin\Release\net7.0\BoschMediaBrowser.Rhino.dll`
   - Core: `src\BoschMediaBrowser.Core\bin\Release\net7.0\BoschMediaBrowser.Core.dll`

3. **Copy to Rhino plugins folder:**
   ```
   %APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser\{GUID}\
   ```
   
   Rename `BoschMediaBrowser.Rhino.dll` to `BoschMediaBrowser.rhp` and copy both files.

4. **Restart Rhino**

## First Time Setup

1. Open Rhino 8
2. Run command: `ShowMediaBrowser`
3. The Media Browser panel will open (initially empty)
4. Click **Settings** (⚙️) button
5. Configure:
   - **Base Server Path**: Point to your products folder  
     Example: `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__`
   - **Thumbnail Cache**: Leave default or customize
   - **Grid Spacing**: Default 1200mm (for product insertion)
   - **Thumbnail Size**: Default 192px

6. Click **Refresh** (🔄) to load products

## Usage

### Commands

- **`ShowMediaBrowser`** - Toggle the media browser panel

### Panel Features

**Browse Tab:**
- **Category Tree** - Navigate by product categories
- **Filters Bar** - Filter by range (DIY/PRO), category, holder variant, tags
- **Thumbnail Grid** - View products with previews (pagination: 20 per page)
- **Detail Pane** - View product details, holders, tags, preview images

**Favourites Tab:**
- View all favourited products
- Quick access to frequently used items
- Clear all favourites option

**Collections Tab:**
- Create custom product collections
- Add/remove products from collections
- Rename or delete collections

### Keyboard Shortcuts

- **Ctrl+F** - Focus search box
- **F5** - Refresh products
- **Esc** - Clear search/filters

## Configuration Files

User data stored in:
```
%APPDATA%\BoschMediaBrowser\
├── settings.json      # User preferences
├── userdata.json      # Favourites, tags, collections
└── ThumbnailCache\    # Cached preview images
```

## Troubleshooting

### Plugin doesn't load

1. Check Rhino's PluginManager (`PlugInManager` command)
2. Look for "Bosch Media Browser" in the list
3. If not listed, check Rhino's command window for error messages

### No products showing

1. Verify the **Base Server Path** in settings points to the correct folder
2. Ensure JSON files exist in the product folders
3. Click **Refresh** to reload
4. Check that folders don't start with underscore (those are hidden)

### Panel is blank

- The UI might still be initializing
- Check the Rhino command window for any errors
- Try closing and reopening the panel with `ShowMediaBrowser`

## Support Files

- **Product Data**: JSON files in each product folder
- **Previews**: PNG/JPG files named `MeshPreview.png` or `GraficaPreview.jpg`
- **Holders**: 3DM files in product folders
- **Public Collections**: Located in `_public-collections/` folder (hidden from categories)

## Updating

To update the plugin:

1. Close Rhino
2. Rebuild the solution
3. Run `Install-Plugin.ps1` again
4. Restart Rhino

## Uninstalling

Remove the plugin folder:
```powershell
Remove-Item "$env:APPDATA\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser" -Recurse -Force
```

User data remains in `%APPDATA%\BoschMediaBrowser\` unless manually deleted.
