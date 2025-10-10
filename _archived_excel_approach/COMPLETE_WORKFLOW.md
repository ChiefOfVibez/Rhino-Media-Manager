# Complete Bosch Product Database Workflow

**Goal:** Add products in 2 minutes. Script auto-fills everything else.

---

## File Structure

### Product Folder (One Per Tool)
```
GBL 18V-750\              ← Folder name (can have suffix like "_Pro Pruner")
├── GBL 18V-750.json      ← YOU create this minimal JSON
├── GBL 18V-750_mesh.3dm      ← Tool 3D model
├── GBL 18V-750_mesh.jpg      ← Tool preview image
├── GBL 18V-750_grafica.3dm   ← Graphic overlay (optional)
├── GBL 18V-750_grafica.jpg   ← Graphic preview
├── GBL 18V-750_packaging.3dm ← Packaging 3D (optional)
└── GBL 18V-750_packaging.jpg ← Packaging preview
```

### Holders Folder (Shared By All Tools)
```
Holders\                    ← Root folder for all holders
├── Tego_RAL7043_BO.161.9LL8600.3dm          ← All .3dm files here
├── Traverse_RAL9006_NN.ALL.BO07802.3dm
├── Com hook traverse_66_RAL7043_NN.ALL.BO07802.3dm
│
├── Garden\                 ← Category folders for previews
│   └── previews\
│       ├── Tego_RAL7043_BO.161.9LL8600.jpg
│       ├── Traverse_RAL9006_NN.ALL.BO07802.jpg
│       └── Com hook traverse_66_RAL7043_NN.ALL.BO07802.jpg
│
├── Drills\
│   └── previews\
│       └── Tego_RAL7043_BO.161.9XX1234.jpg
│
└── Saws\
    └── previews\
        └── ...
```
**Key:** Holder .3dm in root, previews organized by tool category (Garden, Drills, Saws).

---

## File Naming Rules

### Tool Files (in product folder)
```
<ProductName>_mesh.3dm           Tool 3D model
<ProductName>_mesh.jpg/png       Tool preview
<ProductName>_grafica.3dm        Graphic overlay (placed on graphic holder)
<ProductName>_grafica.jpg/png    Graphic preview
<ProductName>_packaging.3dm      Packaging 3D
<ProductName>_packaging.jpg/png  Packaging preview
```

### Holder Files
```
3D:      Holders\{Variant}_{Color}_{CodArticol}.3dm
Preview: Holders\{Category}\previews\{Same name as 3D}.jpg

Examples:
Holders\Tego_RAL7043_BO.161.9LL8600.3dm
Holders\Garden\previews\Tego_RAL7043_BO.161.9LL8600.jpg
```

### Graphic Holder Files
```
3D:      Holders\{Variant}_{Width}_{Color}_{CodArticol}.3dm
Preview: Holders\{Category}\previews\{Same name as 3D}.jpg

Example:
Holders\Com hook traverse_66_RAL7043_NN.ALL.BO07802.3dm
Holders\Garden\previews\Com hook traverse_66_RAL7043_NN.ALL.BO07802.jpg
```
**Note:** Only "Com hook traverse" graphic holders exist.

---

## Workflow: Add New Product

### Step 1: Create Minimal JSON (2 min)
```json
{
  "productName": "GBL 18V-750",
  "description": "Leaf blower 18V",
  "sku": "0.601.9H1.100",
  "range": "PRO",
  "category": "Garden",
  "tags": ["garden", "18V"],
  
  "previews": {},
  "packaging": {},
  
  "holders": [
    "Tego_RAL7043_BO.161.9LL8600",
    "Traverse_RAL9006_NN.ALL.BO07802"
  ],
  
  "graphicHolders": [
    "Com hook traverse_66_RAL7043_NN.ALL.BO07802"
  ],
  
  "relatedFiles": [],
  "metadata": {},
  "notes": ""
}
```
**Save as:** `GBL 18V-750\GBL 18V-750.json`

### Step 2: Run Auto-Populator
```
Double-click: autopop_product_json.bat
```

**Script auto-fills:**
```json
{
  "previews": {
    "mesh3d": {
      "fileName": "GBL 18V-750_mesh.3dm",
      "fullPath": "\\\\Server\\..\\GBL 18V-750_mesh.3dm"
    },
    "meshPreview": {
      "fileName": "GBL 18V-750_mesh.jpg",
      "fullPath": "\\\\Server\\..\\GBL 18V-750_mesh.jpg"
    },
    "grafica3d": {...},
    "graficaPreview": {...}
  },
  
  "packaging": {
    "fileName": "GBL 18V-750_packaging.3dm",
    "fullPath": "\\\\Server\\..\\GBL 18V-750_packaging.3dm",
    "preview": "GBL 18V-750_packaging.jpg",
    "previewPath": "\\\\Server\\..\\GBL 18V-750_packaging.jpg"
  },
  
  "holders": [
    {
      "variant": "Tego",
      "color": "RAL7043",
      "codArticol": "BO.161.9LL8600",
      "fileName": "Tego_RAL7043_BO.161.9LL8600.3dm",
      "fullPath": "\\\\Server\\..\\Holders\\Tego_RAL7043_BO.161.9LL8600.3dm",
      "preview": "\\\\Server\\..\\Holders\\Garden\\previews\\Tego_RAL7043_BO.161.9LL8600.jpg"
    }
  ]
}
```

### Step 3: Done!
Your JSON now has all paths. Plugin can locate and insert files in Rhino.

---

## JSON Structure Explained

### Previews (Auto-Filled with Full Paths)
```json
"previews": {
  "mesh3d": {
    "fileName": "GBL 18V-750_mesh.3dm",      ← File name
    "fullPath": "\\\\Server\\path\\to\\file"  ← Full UNC path for plugin
  },
  "meshPreview": {...},      ← Tool preview image
  "grafica3d": {...},        ← Graphic overlay 3D (placed on holder)
  "graficaPreview": {...}    ← Graphic preview
}
```

**Why full paths?** Plugin needs exact location to insert files in Rhino.

### Holders (Auto-Populated)
```json
"holders": [
  {
    "variant": "Tego",                 ← Extracted from name
    "color": "RAL7043",                ← Extracted from name
    "codArticol": "BO.161.9LL8600",    ← Extracted from name
    "fileName": "Tego_RAL7043_BO.161.9LL8600.3dm",
    "fullPath": "\\\\..\\Holders\\Tego_RAL7043_BO.161.9LL8600.3dm",
    "preview": "\\\\..\\Holders\\Garden\\previews\\Tego_RAL7043_BO.161.9LL8600.jpg"
  }
]
```

**You type:** Just the name `"Tego_RAL7043_BO.161.9LL8600"`  
**Script fills:** All details + paths

### Packaging (Auto-Discovered)
```json
"packaging": {
  "fileName": "GBL 18V-750_packaging.3dm",
  "fullPath": "\\\\..\\GBL 18V-750_packaging.3dm",
  "preview": "GBL 18V-750_packaging.jpg",
  "previewPath": "\\\\..\\GBL 18V-750_packaging.jpg"
}
```

---

## Important Notes

### Folder Names vs Product Names
Folder can have suffix: `GGP 12V-25_Pro Pruner\`  
JSON must match: `GGP 12V-25.json` (not `GGP 12V-25_Pro Pruner.json`)  
Script finds JSON regardless of folder suffix.

### Category Must Match
```json
{
  "category": "Garden"  ← Must exactly match folder name
}
```
Preview lookup: `Holders\Garden\previews\`  
If JSON says "garden" but folder is "Garden" → preview not found!

### Grafica vs Graphic Holder
**Don't confuse!**
- **Grafica:** 2D overlay in product folder (`_grafica.3dm`) → placed ON graphic holder
- **Graphic Holder:** 3D holder from Holders folder → holds the graphic

**Workflow:**
1. User picks graphic holder (from Holders)
2. Plugin inserts graphic holder in Rhino
3. Plugin overlays grafica on top (auto-positioned)

---

## Troubleshooting

### Script Doesn't Find JSON
**Problem:** Folder is `GGP 12V-25_Pro Pruner\` but JSON not found  
**Fix:** Ensure JSON file exists: `GGP 12V-25.json` (any JSON works now)

### Holder Files Not Found
```
⚠️  NOT FOUND: Tego_RAL7043_BO.161.9LL8600.3dm
   Expected at: \\Server\...\Holders\Tego_RAL7043_BO.161.9LL8600.3dm
```
**Fix:** Add the .3dm file to `Holders\` folder (exact name), run script again.

### Preview Not Found
**Problem:** Holder preview empty  
**Fix:** 
1. Ensure preview in `Holders\{Category}\previews\` (e.g., Garden)
2. Check category in JSON matches folder name exactly
3. Preview name must match .3dm name but .jpg/.png

### Syntax Warnings (Now Fixed!)
Old script had escape sequence warnings → fixed in v2.

---

## Daily Workflow

```
1. Create 2-3 minimal JSONs (5 min)
   ↓
2. autopop_product_json.bat (auto)
   ↓
3. Done! All paths filled
```

**Time:** 2 minutes per product (just JSON creation)

---

## What Script Auto-Fills

✅ Tool mesh 3D + preview (with full paths)  
✅ Grafica 3D + preview (with full paths)  
✅ Packaging 3D + preview (with full paths)  
✅ Holder details (variant, color, cod, paths, preview)  
✅ Graphic holder details (variant, width, color, paths, preview)  
✅ Metadata (created date, last modified)  

## What You Provide

📝 Product name  
📝 Description  
📝 SKU  
📝 Range (PRO/DIY)  
📝 Category (Garden, Drills, Saws, etc.)  
📝 Tags  
📝 Holder names (just simple strings)  
📝 Graphic holder names (just simple strings)  

**That's it!**

---

## Ready to Start!

1. Copy `TEMPLATE_minimal.json`
2. Rename to `ProductName.json`
3. Fill in 7 fields (name, description, SKU, range, category, tags, holders)
4. Run `autopop_product_json.bat`
5. Check your JSON → everything auto-filled! 🎯
