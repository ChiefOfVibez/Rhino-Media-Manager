# Bosch Product Database - Operating Instructions

**Version:** 1.1  
**Last Updated:** 2025-10-06 15:20  
**Platform:** Web Application + Network Storage

---

## Table of Contents

1. [Overview](#overview)
2. [Folder Structure & Naming Conventions](#folder-structure--naming-conventions)
3. [Adding Products via Web App](#adding-products-via-web-app)
4. [Adding Products Manually](#adding-products-manually)
5. [Preview Image Management](#preview-image-management)
6. [Holder Management](#holder-management)
7. [File Naming Conventions](#file-naming-conventions)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The Bosch Product Database consists of:
- **Web Application**: http://localhost:8000 (for data entry and management)
- **Network Storage**: `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\`
- **JSON Files**: One per product, containing metadata and file references
- **Rhino Plugin**: Reads JSON files to display products in Rhino (coming soon)

### Core Principle
**The folder structure IS the database structure.** Categories and products are determined by folder names.

---

## Folder Structure & Naming Conventions

### Base Path
```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\
```

### Structure
```
Tools and Holders/
├── PRO/                          # Professional range
│   ├── Garden/                   # Category folder
│   │   ├── GBL 18V-750/         # Product folder (product name)
│   │   │   ├── GBL 18V-750.json # Product metadata (required)
│   │   │   ├── GBL 18V-750_mesh.3dm
│   │   │   ├── GBL 18V-750_mesh.jpg
│   │   │   ├── GBL 18V-750_grafica.3dm
│   │   │   ├── GBL 18V-750_grafica.jpg
│   │   │   ├── GBL_18V-750_packaging.3dm
│   │   │   └── GBL_18V-750_packaging.jpg
│   │   └── [Other Products]/
│   ├── Drills/
│   └── [Other Categories]/
├── DIY/                          # DIY range
│   └── [Same structure as PRO]/
└── Holders/                      # Shared holders
    ├── Tego/
    │   ├── Previews/
    │   │   ├── Tego_RAL7043_preview.jpg
    │   │   └── Tego_RAL9006_preview.jpg
    │   ├── Tego_RAL7043_BO.161.9LL8600.3dm
    │   └── Tego_RAL9006_BO.161.9LL8601.3dm
    └── Traverse/
        └── [Same structure]/
```

### Hidden Folders
Folders starting with underscore `_` are hidden from category lists:
- `_public-collections/` - Rhino plugin collections
- `_templates/` - Template files
- `_archived/` - Old versions

---

## Adding Products via Web App

### Method 1: Using the Web Interface (Recommended)

#### Step 1: Access the Web App
1. Open browser: http://localhost:8000
2. You should see the product table

#### Step 2: Create Product Folder (if needed)
```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders\PRO\[Category]\[ProductName]\
```

**Example:**
```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders\PRO\Garden\GBL 18V-750\
```

#### Step 3: Add Files to Folder
Place these files in the product folder:

**Required Files:**
- `[ProductName]_mesh.3dm` - Tool mesh model
- `[ProductName]_mesh.jpg` or `.png` - Mesh preview image

**Optional Files:**
- `[ProductName]_grafica.3dm` - Graphic overlay
- `[ProductName]_grafica.jpg` - Graphic preview
- `[ProductName]_packaging.3dm` - Packaging model
- `[ProductName]_packaging.jpg` - Packaging preview

#### Step 4: Click "➕ New Product"

Fill in the form:

**Product Folder Path** (required):
```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders\PRO\Garden\GBL 18V-750
```

**Range:**
- `PRO` - Professional range
- `DIY` - DIY range

**Category:** (e.g., `Garden`, `Drills`, `Measuring`)

**Description:** Brief product description

**Holder Names** (optional, one per line):
```
Tego_RAL7043_BO.161.9LL8600
Tego_RAL9006_BO.161.9LL8601
Traverse_RAL7043_NN.ALL.BO07802
Traverse_RAL9006_NN.ALL.BO07803
```

**Format:** `Variant_Color_CodArticol`

#### Step 5: Click "✨ Create & Auto-Populate"

The system will:
1. Create `[ProductName].json` in the product folder
2. Scan folder for mesh, grafica, packaging files
3. Look up holders in `Holders/` folder
4. Find holder preview images
5. Populate all metadata automatically
6. Refresh the product list

---

## Adding Products Manually

### Step 1: Create Product Folder
```
M:\...\Tools and Holders\[RANGE]\[CATEGORY]\[PRODUCT_NAME]\
```

### Step 2: Add Files
Copy all product files (mesh, grafica, packaging) to the folder.

### Step 3: Create JSON File

**Filename:** `[PRODUCT_NAME].json`

**Minimal Template:**
```json
{
  "productName": "GBL 18V-750",
  "description": "Leaf blower with 18V battery",
  "sku": "0.601.9H1.100",
  "range": "PRO",
  "category": "Garden",
  "subcategory": "",
  "tags": ["garden", "18V", "leaf blower"],
  "notes": "",
  
  "previews": {},
  "packaging": {},
  
  "holders": [
    "Tego_RAL7043_BO.161.9LL8600",
    "Tego_RAL9006_BO.161.9LL8601",
    "Traverse_RAL7043_NN.ALL.BO07802"
  ],
  
  "metadata": {}
}
```

### Step 4: Auto-Populate via Web App
1. Open http://localhost:8000
2. Find your product in the list
3. Click "🤖 Auto" button
4. System will populate all file paths automatically

---

## Preview Image Management

### Extract Previews from 3D Files

The web app can automatically extract embedded preview images from `.3dm` files:

1. Click **🖼️ Extract Previews** button in the header
2. System will:
   - Scan all `.3dm` files in `Holders/` folder recursively
   - Extract embedded preview images
   - Save as `.jpg` files in `Holders/<category>/Previews/` folder
   - Name files to match the 3D file (e.g., `Tego_RAL7043_BO.161.9LL8600.jpg`)
   - Skip files that already have previews
3. Wait for completion message
4. Refresh products to see new previews

**Requirements:**
- Rhino files must have embedded preview images
- Python libraries: `rhino3dm` and `Pillow`

### Manual Preview Selection

You can manually browse and select preview images for:
- **Mesh previews** - Click 📁 Browse in Mesh section
- **Grafica previews** - Click 📁 Browse in Grafica section  
- **Packaging previews** - Click 📁 Browse in Packaging section
- **Holder previews** - Click 📁 Browse Preview in Holder section

Manually-set previews are **preserved** when clicking Auto-Populate!

---

## Holder Management

### Holder Folder Structure
```
Holders/
├── [Variant]/                    # e.g., Tego, Traverse
│   ├── Previews/                 # Preview images folder
│   │   └── [Variant]_[Color]_preview.jpg
│   └── [Variant]_[Color]_[CodArticol].3dm
```

### Holder Naming Convention
```
[Variant]_[Color]_[CodArticol].3dm
```

**Examples:**
- `Tego_RAL7043_BO.161.9LL8600.3dm`
- `Traverse_RAL9006_NN.ALL.BO07803.3dm`

### Adding New Holder
1. Create variant folder if it doesn't exist: `Holders/[Variant]/`
2. Add 3DM file: `[Variant]_[Color]_[CodArticol].3dm`
3. Add preview image: `Holders/[Variant]/Previews/[Variant]_[Color]_preview.jpg`
4. Holder will be automatically available to all products

### Linking Holders to Products
In the web app:
1. Edit product
2. Add holder name to the list (format: `Variant_Color_CodArticol`)
3. Click "🤖 Auto-Populate" to find the file

OR in JSON:
```json
"holders": [
  "Tego_RAL7043_BO.161.9LL8600",
  "Tego_RAL9006_BO.161.9LL8601"
]
```

---

## File Naming Conventions

### Product Files

#### Mesh Files
- **File:** `[ProductName]_mesh.3dm`
- **Preview:** `[ProductName]_mesh.jpg` or `.png`
- **Example:** `GBL 18V-750_mesh.3dm`, `GBL 18V-750_mesh.jpg`

#### Grafica (Overlay) Files
- **File:** `[ProductName]_grafica.3dm`
- **Preview:** `[ProductName]_grafica.jpg` or `.png`
- **Example:** `GBL 18V-750_grafica.3dm`

#### Packaging Files
- **File:** `[ProductName]_packaging.3dm`
- **Preview:** `[ProductName]_packaging.jpg` or `.png`
- **Example:** `GBL_18V-750_packaging.3dm`

**Note:** Underscores `_` in filename are flexible for packaging.

### Preview Images
- **Format:** JPG or PNG
- **Recommended size:** 512x512 pixels or larger
- **Transparent background:** PNG recommended for mesh previews

### Holder Files
- **3DM File:** `[Variant]_[Color]_[CodArticol].3dm`
- **Preview:** `[Variant]_[Color]_preview.jpg` (in `Previews/` subfolder)

---

## Troubleshooting

### Issue: Product not showing in web app

**Solution:**
1. Check folder is in correct location: `Tools and Holders\PRO\` or `DIY\`
2. Ensure JSON file exists in product folder
3. JSON filename must match folder name
4. Click "🔄 Scan Database" in web app
5. Click "♻️ Refresh" to reload

### Issue: Preview images not displaying

**Solution:**
1. Check file exists in product folder
2. Filename must match convention: `[ProductName]_mesh.jpg`
3. File must be JPG or PNG format
4. Try clicking preview thumbnail again
5. Check file permissions on network drive

### Issue: Holder not found

**Solution:**
1. Check holder exists in `Holders/[Variant]/` folder
2. Filename must match format: `Variant_Color_CodArticol.3dm`
3. Check spelling matches exactly (case-sensitive)
4. Click "🤖 Auto-Populate" to re-scan

### Issue: "Reveal in Explorer" not working

**Solution:**
1. Ensure web app server is running
2. Check file path in JSON is correct
3. Verify network drive is mapped on your PC
4. Try clicking folder path in product header instead

### Issue: Auto-populate doesn't find files

**Solution:**
1. Check files are in the product folder
2. Verify filenames follow naming convention exactly
3. Check file extensions (.3dm, .jpg, .png)
4. Ensure files aren't in subfolders
5. Run auto-populate again after fixing names

### Issue: Dropdown not updating Cod Articol

**Solution:**
1. Check holders array in JSON has full data
2. Click "🤖 Auto" to refresh holder information
3. Ensure holder files exist in Holders folder
4. Refresh browser page

---

## Web App Features Quick Reference

### Main Table
- **Preview Column:** Click to view full-size image
- **Variant Dropdown:** Select holder variant → Live updates Cod Articol column
- **Color Dropdown:** Select color → Live updates Cod Articol & Holder Preview columns
- **Cod Articol Column:** Automatically displays the code for selected holder variant/color
- **Holder Preview Column:** Automatically displays thumbnail for selected holder (click to enlarge)
- **✏️ Edit:** Open full editor
- **🤖 Auto:** Auto-populate product data

### Edit Modal
- **📁 Folder Path:** Click to open product folder in Explorer
- **Tags:** Add/remove tags with ➕ button
- **Preview Sections:**
  - **📁 Browse** buttons for Mesh, Grafica, Packaging - Manually select preview images
  - Click preview image to view full size
- **Holder Section:**
  - **Variant/Color Dropdowns:** Select to view specific holder
  - **Cod Articol:** Displays automatically for selected holder
  - **📂 Reveal in Explorer:** Opens file location in Windows Explorer (works with UNC paths)
  - **📁 Browse Preview:** Manually select holder preview image from Previews folder
  - **Preview Image:** Click to enlarge
- **🖼️ Extract Previews** (Header button): Batch extract preview images from all .3dm files
- **🤖 Auto-Populate:** Re-scan and update all file paths and data
- **💾 Save Changes:** Save to JSON file (preserves all manually-set previews)

### Add Product (➕)
1. Enter folder path
2. Set range, category, description
3. Optionally add holder names
4. Click "✨ Create & Auto-Populate"

---

## Best Practices

### ✅ DO
- Use consistent naming: `[ProductName]_[type].3dm`
- Keep folder names short and clear
- Add preview images for all 3D files
- Use auto-populate to avoid manual JSON editing
- Test in web app after adding products
- Add descriptive tags for searchability

### ❌ DON'T
- Use spaces in filenames (use `_` or `-`)
- Create nested subfolders in product folders
- Manually edit JSON unless necessary
- Start folder names with underscore (will be hidden)
- Mix PRO and DIY products in same category folder
- Forget to add preview images

---

## Quick Start Checklist

### Adding a New Product

- [ ] Create product folder in correct location (`PRO/` or `DIY/[Category]/`)
- [ ] Add `[ProductName]_mesh.3dm` file
- [ ] Add `[ProductName]_mesh.jpg` preview
- [ ] (Optional) Add grafica and packaging files
- [ ] Open http://localhost:8000
- [ ] Click "➕ New Product"
- [ ] Fill in folder path, range, category
- [ ] Add holder names (if applicable)
- [ ] Click "✨ Create & Auto-Populate"
- [ ] Verify product appears in table
- [ ] Click "✏️ Edit" to check all data is correct
- [ ] Test dropdowns for holder variants

---

## Support & Updates

**Web App URL:** http://localhost:8000

**Network Path:** `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\`

**Document Updates:** This file will be updated as new features are added or procedures change.

**Version History:**
- v1.1 (2025-10-06 15:20): Added live table updates, browse buttons for all preview types, UNC path support for reveal in explorer
- v1.0 (2025-10-06): Initial comprehensive guide

---

## Appendix: JSON Structure Reference

### Complete JSON Example
```json
{
  "productName": "GBL 18V-750",
  "description": "Suflanta pentru frunze cu acumulator 18V",
  "sku": "0.601.9H1.100",
  "range": "PRO",
  "category": "Garden",
  "subcategory": "",
  "tags": ["garden", "18V", "leaf blower", "PRO"],
  "notes": "Internal notes here",
  
  "previews": {
    "mesh3d": "GBL 18V-750_mesh.3dm",
    "meshPreview": "GBL 18V-750_mesh.jpg",
    "grafica3d": "GBL 18V-750_grafica.3dm",
    "graficaPreview": "GBL 18V-750_grafica.jpg"
  },
  
  "packaging": {
    "fileName": "GBL_18V-750_packaging.3dm",
    "preview": "GBL_18V-750_packaging.jpg"
  },
  
  "holders": [
    {
      "variant": "Tego",
      "color": "RAL7043",
      "codArticol": "BO.161.9LL8600",
      "fileName": "Tego_RAL7043_BO.161.9LL8600.3dm",
      "preview": "Tego_RAL7043_preview.jpg",
      "fullPath": "M:\\...\\Holders\\Tego\\Tego_RAL7043_BO.161.9LL8600.3dm"
    }
  ],
  
  "metadata": {
    "createdDate": "2025-10-06",
    "lastModified": "2025-10-06"
  }
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `productName` | string | ✅ | Product name (matches folder name) |
| `description` | string | ✅ | Brief product description |
| `sku` | string | ❌ | Product SKU code |
| `range` | string | ✅ | "PRO" or "DIY" |
| `category` | string | ✅ | Category (e.g., "Garden") |
| `subcategory` | string | ❌ | Optional subcategory |
| `tags` | array | ❌ | Search tags |
| `notes` | string | ❌ | Internal notes |
| `previews` | object | ❌ | Mesh and grafica file references |
| `packaging` | object | ❌ | Packaging file references |
| `holders` | array | ❌ | Holder configurations |
| `metadata` | object | ❌ | Timestamps and other metadata |

---

**End of Document**
