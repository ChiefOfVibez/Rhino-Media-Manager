# 🦏 Rhino Media Browser Plugin - Planning Document

**Phase:** 5 (Post-Webapp Completion)  
**Methodology:** Spec Kit Approach  
**Target:** Rhino 8 on Windows (.NET 7)  
**Status:** Planning Phase

---

## 📋 Overview

**Goal:** Build a Rhino Media Browser plugin similar to V-Ray Chaos Cosmos for managing and inserting Bosch product holders into Rhino.

**Data Source:** Excel workbook (via JSON export)  
**Integration:** Uses same JSON format as webapp  
**Platform:** RhinoCommon C# + Eto.Forms dockable panel

---

## 🎯 Spec Kit Methodology

### Phase 5A: Specification & Architecture (Week 1)

#### 1. Requirements Specification
- [ ] **Functional Requirements Document**
  - User stories and use cases
  - Feature prioritization
  - Success criteria
  
- [ ] **Technical Requirements**
  - Rhino 8 compatibility
  - .NET 7 framework
  - File system requirements
  - Performance targets

#### 2. Architecture Design
- [ ] **System Architecture**
  - Plugin structure (RhinoCommon)
  - Data layer (JSON parsing)
  - UI layer (Eto.Forms)
  - Integration points

- [ ] **Data Model**
  - JSON schema (same as webapp)
  - In-memory caching strategy
  - Sync mechanism with Excel

- [ ] **UI/UX Design**
  - Dockable panel mockups
  - User workflows
  - Interaction patterns

#### 3. Component Specifications
- [ ] **Core Components**
  - Media browser panel
  - Search & filter system
  - Category tree navigation
  - Preview system
  - Collections manager
  - Insert mechanism

### Phase 5B: Implementation (Weeks 2-3)

#### Sprint 1: Core Infrastructure
- [ ] Project setup (Visual Studio, NuGet)
- [ ] RhinoCommon plugin template
- [ ] JSON parser
- [ ] Data models (C# classes)
- [ ] Unit tests

#### Sprint 2: UI Foundation
- [ ] Eto.Forms dockable panel
- [ ] Basic layout (search, tree, preview)
- [ ] Tab system (Browse, Favorites, Collections)
- [ ] Theme integration (Rhino Dark/Light)

#### Sprint 3: Browser Functionality
- [ ] Category tree from folder structure
- [ ] Product list view
- [ ] Search implementation
- [ ] Filter by range/category
- [ ] Sort options

#### Sprint 4: Preview & Insert
- [ ] Image preview panel
- [ ] 3D preview (if feasible)
- [ ] Insert linked file command
- [ ] Multi-select insert
- [ ] Insert options (location, rotation)

#### Sprint 5: Collections & Favorites
- [ ] Favorites system
- [ ] Collections CRUD
- [ ] Persist to `_public-collections/`
- [ ] Share collections

#### Sprint 6: Polish & Testing
- [ ] Error handling
- [ ] Performance optimization
- [ ] User testing
- [ ] Documentation
- [ ] Deployment package

### Phase 5C: Integration & Deployment (Week 4)

#### Integration
- [ ] Sync with webapp JSON format
- [ ] Network path configuration
- [ ] Auto-update check (future)
- [ ] Excel export integration

#### Deployment
- [ ] RHI installer package
- [ ] Installation guide
- [ ] User manual
- [ ] Training materials

---

## 🏗️ Technical Architecture (Preview)

### Component Diagram

```
┌─────────────────────────────────────────────────────┐
│  Rhino Media Browser Plugin (.rhp)                  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  Eto.Forms Dockable Panel                   │   │
│  ├─────────────────────────────────────────────┤   │
│  │  ┌─ Search Bar ──────────────────────────┐  │   │
│  │  └─────────────────────────────────────────  │   │
│  │  ┌─ Tabs ────────────────────────────────┐  │   │
│  │  │ [Browse] [Favorites] [Collections]    │  │   │
│  │  └─────────────────────────────────────────  │   │
│  │  ┌─ Left Panel (Categories) ─┬─ Right ───┐  │   │
│  │  │  📂 Tools and Holders     │           │  │   │
│  │  │    ├── PRO Range          │  Product  │  │   │
│  │  │    │   ├── Garden         │  List     │  │   │
│  │  │    │   ├── Demolition     │           │  │   │
│  │  │    └── DIY Range          │  [Items]  │  │   │
│  │  │        ├── Drilling       │           │  │   │
│  │  │        └── Cutting        │           │  │   │
│  │  └─────────────────────────────────────────  │   │
│  │  ┌─ Bottom Panel (Preview) ──────────────┐  │   │
│  │  │  🖼️ Image Preview                     │  │   │
│  │  │  📋 Product Info                       │  │   │
│  │  │  [Insert] [Add to Collection]         │  │   │
│  │  └─────────────────────────────────────────  │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  Data Layer                                 │   │
│  ├─────────────────────────────────────────────┤   │
│  │  • JSON Parser (M:\...\*.json)             │   │
│  │  • In-memory Cache (Products, Holders)     │   │
│  │  • Collections Manager                      │   │
│  │  • Favorites Manager                        │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │  Rhino Commands                             │   │
│  ├─────────────────────────────────────────────┤   │
│  │  • InsertBoschHolder                        │   │
│  │  • BoschMediaBrowser (open panel)          │   │
│  │  • RefreshBoschDatabase                     │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
                          ↕
┌─────────────────────────────────────────────────────┐
│  Data Source                                        │
├─────────────────────────────────────────────────────┤
│  M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__│
│  ├── Tools and Holders/                            │
│  │   ├── PRO/                                      │
│  │   │   ├── Garden/                               │
│  │   │   │   ├── GBL 18V-750/                      │
│  │   │   │   │   └── product.json  ← Same as webapp│
│  │   └── DIY/                                      │
│  └── _public-collections/                          │
│      ├── my_collection.json                        │
│      └── team_favorites.json                       │
└─────────────────────────────────────────────────────┘
```

---

## 📊 Feature Matrix

| Feature | Priority | Complexity | Time Estimate |
|---------|----------|------------|---------------|
| **Core Browser** | P0 | Medium | 2 days |
| Dockable panel | P0 | Low | 1 day |
| Category tree | P0 | Medium | 1 day |
| Product list | P0 | Low | 1 day |
| Search/filter | P0 | Medium | 1 day |
| **Preview** | P0 | Medium | 1 day |
| Image preview | P0 | Low | 0.5 day |
| Product info | P0 | Low | 0.5 day |
| **Insert** | P0 | High | 2 days |
| Single insert | P0 | Medium | 1 day |
| Multi-select | P1 | Medium | 1 day |
| Insert options | P1 | Medium | 1 day |
| **Collections** | P1 | Medium | 2 days |
| Favorites | P1 | Low | 0.5 day |
| Collections CRUD | P1 | Medium | 1 day |
| Share collections | P1 | Low | 0.5 day |
| **Polish** | P1 | Low | 2 days |
| Error handling | P1 | Low | 0.5 day |
| Performance | P1 | Medium | 1 day |
| Documentation | P1 | Low | 0.5 day |

**Total Estimated Time:** 12-15 days (2-3 weeks)

---

## 📝 JSON Schema (Shared with Webapp)

The plugin uses the same JSON format as the webapp for seamless integration:

```json
{
  "productName": "GBL 18V-750",
  "description": "Cordless blower",
  "sku": "0.601.9H1.100",
  "range": "PRO",
  "category": "Garden",
  "subcategory": "Blowers",
  "tags": ["cordless", "18V", "lightweight"],
  "notes": "Internal notes",
  "packaging": {
    "length": 45.5,
    "width": 30.0,
    "height": 25.0,
    "weight": 2.5
  },
  "holders": [
    {
      "variant": "Tego",
      "color": "RAL7043",
      "codArticol": "BO.161.9LL8600",
      "fileName": "Tego_RAL7043_BO.161.9LL8600.3dm",
      "fullPath": "M:\\...\\Tego_RAL7043_BO.161.9LL8600.3dm",
      "preview": "Tego_RAL7043_BO.161.9LL8600.jpg"
    }
  ],
  "previews": {
    "meshPreview": "GBL 18V-750 mesh.jpg"
  },
  "metadata": {
    "createdDate": "2025-01-15",
    "modifiedDate": "2025-10-07",
    "version": "1.0"
  }
}
```

---

## 🎨 UI/UX Considerations

### Design Principles
1. **Familiar:** Similar to Chaos Cosmos (V-Ray users know it)
2. **Fast:** Instant search, cached data
3. **Visual:** Preview-first approach
4. **Integrated:** Feels native to Rhino
5. **Flexible:** Collections and favorites

### User Workflows

#### Workflow 1: Browse & Insert
```
1. Open panel (BoschMediaBrowser command)
2. Browse category tree → Select product
3. Preview shows image + info
4. Click "Insert" → Select point in viewport
5. Linked file inserted into Rhino
```

#### Workflow 2: Search & Insert
```
1. Type in search: "GBL 18V"
2. Results filtered instantly
3. Select product → Preview
4. Insert with one click
```

#### Workflow 3: Collections
```
1. Browse products
2. Right-click → "Add to Collection"
3. Create new collection: "My Project"
4. Later: Switch to Collections tab
5. Insert all items from collection
```

---

## 🔧 Technical Decisions

### Language & Framework
- **C#** - Rhino 8 uses .NET 7
- **RhinoCommon** - Official Rhino SDK
- **Eto.Forms** - Cross-platform UI (Rhino's standard)

### Data Source
- **Primary:** JSON files on network (M:\ drive)
- **Cache:** In-memory for performance
- **Sync:** Manual refresh or auto-detect changes

### File Insert Method
- **Linked Blocks:** Use Rhino's linked block system
- **Preserves network path:** Always references M:\ drive
- **Update propagation:** Changes to .3dm update all instances

### Collections Storage
- **Location:** `M:\...\__NEW DB__\_public-collections\`
- **Format:** JSON (list of product names)
- **Sharing:** Network folder for team access
- **Underscore prefix:** Hidden from category tree

---

## 🚀 Development Roadmap

### Week 1: Specification & Design
- [ ] Finalize requirements document
- [ ] Complete architecture design
- [ ] UI/UX mockups
- [ ] Component specifications
- [ ] JSON schema agreement
- [ ] Development environment setup

### Week 2: Core Development
- [ ] Sprint 1: Infrastructure (2 days)
- [ ] Sprint 2: UI Foundation (2 days)
- [ ] Sprint 3: Browser (2 days)

### Week 3: Features & Polish
- [ ] Sprint 4: Preview & Insert (2 days)
- [ ] Sprint 5: Collections (2 days)
- [ ] Sprint 6: Polish & Testing (2 days)

### Week 4: Integration & Deployment
- [ ] Integration testing with webapp
- [ ] User acceptance testing
- [ ] Documentation
- [ ] RHI installer package
- [ ] Training materials
- [ ] Deployment

---

## 📚 Reference Materials

### Existing Specs
- `BoschMediaBrowserSpec/BETTER_ARCHITECTURE.md` - Architecture overview
- `BoschMediaBrowserSpec/` - Complete spec kit (97 items)

### Integration Points
- Webapp JSON format (established)
- Network folder structure (defined)
- Public collections path (standardized)
- Underscore-prefix hidden folders (implemented)

### Similar Projects
- V-Ray Chaos Cosmos (UI/UX reference)
- Enscape Asset Library (functionality)
- Rhino Toolbars (docking behavior)

---

## ✅ Prerequisites (Before Starting Phase 5)

### Webapp Must Be Complete
- [x] Phase 1-3 done ✅
- [x] Phase 4A done ✅
- [ ] Phase 4B done (pagination, bulk ops)
- [ ] Team deployment successful
- [ ] JSON schema finalized

### Technical Setup
- [ ] Visual Studio 2022 installed
- [ ] Rhino 8 SDK installed
- [ ] .NET 7 SDK
- [ ] NuGet packages configured
- [ ] Development Rhino license

### Documentation Ready
- [ ] Requirements doc
- [ ] Architecture design
- [ ] UI mockups
- [ ] Component specs
- [ ] Test plan

---

## 🎯 Success Criteria

**Plugin is successful when:**
- ✅ Dockable panel integrates seamlessly into Rhino
- ✅ Can browse all products from network
- ✅ Search finds products instantly (<100ms)
- ✅ Can insert single/multiple holders
- ✅ Collections and favorites work
- ✅ Team can collaborate via shared collections
- ✅ Same JSON format as webapp (no conversion)
- ✅ Stable, no crashes
- ✅ Professional UI/UX
- ✅ Complete documentation

---

## 📞 Next Steps

### When to Start Phase 5
1. **Complete Phase 4B** (pagination, bulk ops)
2. **Deploy webapp** to team successfully
3. **Gather feedback** on JSON schema
4. **Finalize requirements** based on real usage
5. **Begin spec kit process** → Requirements → Architecture → Implementation

### Estimated Start Date
- After Phase 4B completion (~1 week)
- Total Phase 5 duration: 3-4 weeks
- Target completion: End of October

---

**This is a planning preview. Full spec kit documentation will be created when Phase 4 is complete and webapp is deployed to the team. 🦏**
