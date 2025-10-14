# Changelog

All notable changes to the Bosch Media Browser Rhino Plugin will be documented in this file.

## [1.0.0] - 2025-10-14 - MILESTONE RELEASE 🎉

### ✅ Major Achievement: Linked Blocks Working!

This is a **major milestone** - the plugin now creates true linked blocks that update when source files change.

### Added
- ✅ **Linked block support** using `InstanceDefinitionUpdateType.LinkedAndEmbedded`
- ✅ **Simplified block naming** based on source filename
- ✅ **Holder transform system** with automatic tool positioning
- ✅ **Packaging insertion** at offset position
- ✅ **Material import** and mapping from source files
- ✅ **Settings persistence** with async save
- ✅ **Path validation** for "Tools and Holders" structure
- ✅ **Rebuild script** (`Verify-And-Install.bat`) with automatic build
- ✅ **Debug logging** for troubleshooting

### Fixed
- ✅ **Block naming bug** - Blocks now use simple filename (e.g., `Traverse_RAL9006_NN.ALL.BO07803`)
- ✅ **Settings not saving** - Made save method async with await
- ✅ **Linked blocks not working** - Added `ModifySourceArchive()` call
- ✅ **Database path** - Now correctly supports "Tools and Holders" folder structure
- ✅ **Product loading filter** - Whitelist specific hidden folders instead of blocking all `_*`

### Block Naming
Tool mesh with holder:
```
GBL 18V-750_Mesh_Tego_Traverse_RAL9006
```

Holder (stationary):
```
Traverse_RAL9006_NN.ALL.BO07803
```

Packaging:
```
GBL 18V-750_packaging
```

### Insertion Flow
1. **Tool mesh** - Inserted with holder transform applied
2. **Holder** - Inserted at origin (no transform)
3. **Packaging** - Inserted at origin + 500mm X offset

### Settings
```json
{
  "InsertBlockType": "Linked",           // ← Creates linked blocks
  "InsertAs": "BlockInstance",           // or "Group"
  "InsertLayerStyle": "Active",          // or "Reference"
  "InsertReadLinkedBlocks": true
}
```

### Database Structure
Expected path: `M:\...\__NEW DB__\Tools and Holders`

```
Tools and Holders/
├── DIY/
│   └── [Categories]/
│       └── [Products]/
├── PRO/
│   └── [Categories]/
│       └── [Products]/
│           ├── ProductName.json
│           ├── ProductName_Mesh_Tego.3dm
│           └── ProductName_packaging.3dm
└── Holders/
    └── [Categories]/
        └── HolderVariant_Color_SKU.3dm
```

### Installation
Run `scripts\Verify-And-Install.bat` which now:
1. Cleans old builds
2. Rebuilds plugin
3. Checks Rhino status
4. Resets panel state
5. Installs to Rhino
6. Verifies installation

### Known Issues
- None! Everything working as expected.

### Technical Details
- **Platform:** Rhino 8, .NET 7.0
- **UI Framework:** Eto.Forms
- **Block Type:** LinkedAndEmbedded (updates when source changes)
- **Transform System:** JSON-based holder transforms with rotation/translation/scale

---

## [0.9.0] - 2025-10-13 - Pre-Milestone

### Added
- Initial plugin structure
- Product browsing UI
- Category filtering
- Holder selection
- Basic insertion

### Issues (Resolved in 1.0.0)
- ❌ Blocks were embedded only
- ❌ Block names were complex
- ❌ Settings didn't persist
- ❌ Wrong geometry inserted
- ❌ Path validation issues

---

## Future Releases

### [1.1.0] - Planned
- [ ] Proxy mesh support
- [ ] Multiple product selection
- [ ] Batch insert
- [ ] Custom collections

### [1.2.0] - Planned
- [ ] Favorites system
- [ ] Tag-based filtering
- [ ] Search improvements
- [ ] Recent products

---

**Format:** [MAJOR.MINOR.PATCH]
- **MAJOR** - Breaking changes
- **MINOR** - New features (backwards compatible)
- **PATCH** - Bug fixes
