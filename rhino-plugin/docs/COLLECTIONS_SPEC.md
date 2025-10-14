# Collections System Specification

## Overview

Collections allow users to save and reuse groups of products with their specific transforms, holder configurations, and packaging settings.

---

## Use Cases

1. **Save Arrangement** - After arranging multiple products in viewport, save the configuration
2. **Reuse Layout** - Insert previously saved product arrangements
3. **Share Collections** - Export/import collections for team collaboration
4. **Quick Insert** - Insert entire product assemblies with one click

---

## Data Model

### Collection Structure

```json
{
  "id": "unique-guid",
  "name": "Drill Set - Vertical Wall Display",
  "description": "Standard wall display configuration for PRO drills",
  "createdDate": "2025-10-14T15:30:00Z",
  "modifiedDate": "2025-10-14T16:45:00Z",
  "tags": ["display", "wall-mount", "PRO"],
  "items": [
    {
      "productId": "product-guid",
      "productName": "GBL 18V-750",
      "holderVariant": "Traverse",
      "holderColor": "RAL9006",
      "includePackaging": false,
      "transform": {
        "translation": [0, 0, 0],
        "rotation": [0, 0, 0],
        "scale": [1, 1, 1]
      }
    },
    {
      "productId": "product-guid-2",
      "productName": "GBH 18V-26",
      "holderVariant": "Tego",
      "holderColor": "RAL7043",
      "includePackaging": false,
      "transform": {
        "translation": [300, 0, 0],
        "rotation": [0, 0, 0],
        "scale": [1, 1, 1]
      }
    }
  ]
}
```

### Storage Location

- **User Collections:** `%APPDATA%\BoschMediaBrowser\Collections\`
- **Public Collections:** `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\_public-collections\`

---

## Features

### 1. Create Collection from Viewport Selection

**Workflow:**
1. User inserts and arranges products in viewport
2. User selects inserted instances (must be from plugin)
3. User runs command: `CreateCollectionFromSelection` OR clicks "Create Collection" button
4. Plugin detects:
   - Product ID from block name/user data
   - Holder configuration from block name
   - Transform (position, rotation, scale)
   - Packaging state
5. Dialog prompts for:
   - Collection name (required)
   - Description (optional)
   - Tags (optional, comma-separated)
6. Collection saved to user collections folder

**Technical Implementation:**
- Iterate through selected objects
- Check if object is an instance definition created by plugin
- Extract product metadata from:
  - Block name parsing
  - User strings attached to instance
- Get transform from `InstanceObject.InstanceXform`
- Validate all products exist in current database
- Create collection JSON file

### 2. Insert Collection

**Workflow:**
1. User opens Collections panel
2. User selects a collection
3. User clicks "Insert Collection"
4. User picks insertion point in viewport
5. Plugin inserts all products relative to insertion point
6. Each product uses saved:
   - Holder configuration
   - Packaging settings
   - Relative transforms

**Transform Calculation:**
```csharp
// Collection stores transforms relative to collection origin (0,0,0)
// User picks insertion point P
// For each item with transform T:
//   FinalTransform = Translate(P) * T
```

### 3. Manage Collections

**UI Panel - Collections Tab:**
```
┌─────────────────────────────────────┐
│ ⊕ Browse  ⊕ Collections  ⊕ Filters │
├─────────────────────────────────────┤
│ 📁 My Collections                   │
│   ├─ Drill Wall Display             │
│   ├─ Garden Tool Set                │
│   └─ Hedge Trimmer Display          │
│                                     │
│ 📁 Public Collections               │
│   ├─ PRO Standard Wall (Company)    │
│   └─ DIY Retail Display (Company)   │
├─────────────────────────────────────┤
│ Preview:                            │
│ ┌─────────────────────────────────┐ │
│ │                                 │ │
│ │   [Collection Preview Image]    │ │
│ │                                 │ │
│ └─────────────────────────────────┘ │
│                                     │
│ Name: Drill Wall Display            │
│ Items: 5 products                   │
│ Created: 2025-10-14                 │
│ Tags: #display #wall-mount #PRO     │
│                                     │
│ [Insert]  [Edit]  [Delete]  [Export]│
└─────────────────────────────────────┘
```

**Actions:**
- **Insert** - Insert collection at picked point
- **Edit** - Rename, change description/tags
- **Delete** - Remove collection (confirm dialog)
- **Export** - Save .bmb_collection file for sharing
- **Import** - Load .bmb_collection file

### 4. Track Inserted Items (Plugin Metadata)

When inserting products, attach metadata:

```csharp
// After creating instance
var instanceObj = doc.Objects.FindId(instanceId);
instanceObj.Attributes.SetUserString("BMB_ProductId", product.Id);
instanceObj.Attributes.SetUserString("BMB_HolderVariant", holder?.Variant);
instanceObj.Attributes.SetUserString("BMB_HolderColor", holder?.Color);
instanceObj.Attributes.SetUserString("BMB_IncludePackaging", includePackaging.ToString());
instanceObj.Attributes.SetUserString("BMB_InsertedBy", "BoschMediaBrowser");
instanceObj.Attributes.SetUserString("BMB_Version", "1.1.0");
doc.Objects.ModifyAttributes(instanceObj, instanceObj.Attributes, true);
```

This allows:
- Collection creation from viewport selection
- Validation that objects were inserted by plugin
- Reconstruction of product configuration

---

## Commands

### CreateCollectionFromSelection

**Rhino Command:** `BMB_CreateCollection`

```csharp
protected override Result RunCommand(RhinoDoc doc, RunMode mode)
{
    // Get selected objects
    var objs = doc.Objects.GetSelectedObjects(false, false);
    if (objs.Length == 0)
    {
        RhinoApp.WriteLine("No objects selected.");
        return Result.Failure;
    }
    
    // Filter for plugin-inserted instances
    var pluginInstances = objs
        .Where(obj => obj.ObjectType == ObjectType.InstanceReference)
        .Where(obj => obj.Attributes.GetUserString("BMB_InsertedBy") == "BoschMediaBrowser")
        .ToList();
    
    if (pluginInstances.Count == 0)
    {
        RhinoApp.WriteLine("No plugin-inserted objects in selection.");
        return Result.Failure;
    }
    
    // Show collection creation dialog
    var dialog = new CreateCollectionDialog(pluginInstances);
    var result = dialog.ShowModal();
    
    if (result)
    {
        // Save collection
        CollectionService.SaveCollection(dialog.Collection);
        RhinoApp.WriteLine($"Collection '{dialog.Collection.Name}' created with {dialog.Collection.Items.Count} items.");
    }
    
    return Result.Success;
}
```

---

## UI Updates

### Multi-Insert Button (List View)

**Current State:** ✅ **IMPLEMENTED**
- Checkbox appears when Multi-Select mode enabled
- User checks multiple products
- "Insert Selected (N)" button appears at bottom
- Click to batch insert all selected products with their holder/packaging settings

### Collections Panel

**New Tab in MediaBrowserPanel:**
- Tab: "Collections" (alongside Browse, Filters)
- Tree view showing user + public collections
- Preview pane showing collection details
- Action buttons for insert/edit/delete/export

---

## File Format

### Collection File (.bmb_collection)

JSON format for sharing:
```json
{
  "version": "1.0",
  "collection": {
    // Collection JSON as defined above
  }
}
```

---

## Implementation Phases

### Phase 1: Multi-Insert ✅ COMPLETED
- [x] Checkbox system in list view
- [x] Track selected items with holder/packaging
- [x] Batch insert button
- [x] Batch insert handler in MediaBrowserPanel

### Phase 2: Metadata Tracking
- [ ] Attach user strings to inserted instances
- [ ] Validate objects inserted by plugin
- [ ] Extract product/holder info from instances

### Phase 3: Collection Creation
- [ ] `CreateCollectionFromSelection` command
- [ ] Collection creation dialog (name, desc, tags)
- [ ] Calculate relative transforms
- [ ] Save collection to JSON file

### Phase 4: Collections UI
- [ ] Collections tab in MediaBrowserPanel
- [ ] Collection list (user + public)
- [ ] Collection preview pane
- [ ] Insert/Edit/Delete actions

### Phase 5: Collection Insert
- [ ] Pick insertion point
- [ ] Calculate final transforms
- [ ] Batch insert all products
- [ ] Preserve holder/packaging settings

### Phase 6: Import/Export
- [ ] Export collection to .bmb_collection file
- [ ] Import collection from file
- [ ] Validate imported collections

---

## Testing

### Test Cases

1. **Create Collection from 3 Products**
   - Insert 3 products with different holders
   - Arrange in a row (300mm spacing)
   - Select all 3, create collection
   - Verify: transforms saved correctly

2. **Insert Collection**
   - Load previously saved collection
   - Pick insertion point
   - Verify: all products insert at correct relative positions

3. **Import/Export**
   - Export collection to file
   - Delete from user collections
   - Import from file
   - Verify: collection restored correctly

4. **Public Collections**
   - Create collection in public folder
   - Verify: appears in Collections panel
   - Verify: all team members can insert

---

## Benefits

1. **Productivity** - Reuse common arrangements (wall displays, retail layouts)
2. **Consistency** - Same configurations across projects
3. **Collaboration** - Share collections via public folder or export files
4. **Speed** - One-click insertion of complex assemblies
5. **Quality** - Pre-validated product combinations

---

## Future Enhancements

- **Collection Templates** - Pre-built collections for common layouts
- **Thumbnail Generation** - Auto-generate preview images
- **Version Control** - Track collection changes over time
- **Cloud Sync** - Share collections across multiple machines
- **Smart Insertion** - Auto-adjust to surfaces, walls, etc.

---

**Status:** Phase 1 Complete, Phase 2-6 Planned
**Priority:** High - Critical for workflow efficiency
**Target:** v1.1.0 Release
