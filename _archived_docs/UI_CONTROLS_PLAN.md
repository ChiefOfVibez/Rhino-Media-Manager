# UI Controls Implementation Plan (T017-T020)

**Timeline:** Today (build) + Tomorrow (test in Rhino)  
**Estimated Time:** 8-10 hours  
**Status:** Ready to implement after .NET 7 SDK install

---

## Overview

Now that the Core layer is complete and tested, we'll build the **Eto.Forms UI controls** that make up the Media Browser interface.

**What we're building:**
- 🗂️ **CategoryTree** - Hierarchical category navigation
- 🔍 **FiltersBar** - Range/holder/tag filters
- 🖼️ **ThumbnailGrid** - Virtualized product grid with thumbnails
- 📋 **DetailPane** - Product details with preview image
- ⭐ **FavouritesView** - Favourites management
- 📚 **CollectionsView** - Collections management

---

## Architecture

```
MediaBrowserPanel (existing)
├── Toolbar (existing: search, refresh, settings)
├── MainContent (NEW - split panel)
│   ├── Left Sidebar (NEW)
│   │   ├── CategoryTree (T017)
│   │   └── FiltersBar (T017)
│   └── Right Content (NEW)
│       ├── ThumbnailGrid (T018)
│       └── DetailPane (T019)
└── Status Bar (existing)
```

---

## Task Breakdown

### T017: Category Tree & Filters Bar (~2-3 hours)

**Files to create:**
```
src/BoschMediaBrowser.Rhino/UI/Controls/CategoryTree.cs
src/BoschMediaBrowser.Rhino/UI/Controls/FiltersBar.cs
```

**CategoryTree Features:**
- ✅ Hierarchical tree from SearchService.BuildCategoryTree()
- ✅ Expand/collapse nodes
- ✅ Selection handling (filter by category)
- ✅ Root nodes: "All Products", DIY, PRO, "Tools and Holders"
- ✅ Update on data reload

**FiltersBar Features:**
- ✅ Range checkboxes (DIY, PRO)
- ✅ Category dropdown (from unique categories)
- ✅ Holder variant dropdown
- ✅ Tag filter (multi-select)
- ✅ "Clear Filters" button
- ✅ Wire to SearchService.FilterAndSort()

---

### T018: Thumbnail Grid (~3-4 hours)

**File to create:**
```
src/BoschMediaBrowser.Rhino/UI/Controls/ThumbnailGrid.cs
```

**Features:**
- ✅ **Virtualized scrolling** (only render visible items)
- ✅ **Grid layout** with configurable columns
- ✅ **Product cards** showing:
  - Thumbnail preview (from ThumbnailService)
  - Product name
  - SKU
  - Category badge
  - Favourite star icon
- ✅ **Selection model:**
  - Single click: select item, update DetailPane
  - Ctrl+click: multi-select
  - Shift+click: range select
- ✅ **Lazy image loading** (load thumbnails on demand)
- ✅ **Pagination controls** (prev/next, page X of Y)
- ✅ **Empty state** ("No products found")

**Performance:**
- Use Eto.Forms.Drawable or GridView
- Only load thumbnails for visible items
- Cache loaded images in memory

---

### T019: Detail Pane (~2 hours)

**File to create:**
```
src/BoschMediaBrowser.Rhino/UI/Controls/DetailPane.cs
```

**Features:**
- ✅ **Product info section:**
  - Large preview image (with zoom?)
  - Product name (editable? or read-only)
  - Description
  - SKU
  - Category path
  - Range badge
- ✅ **Action buttons:**
  - ⭐ Toggle Favourite
  - 🏷️ Add/Remove Tags
  - 📁 Open Folder (Reveal in Explorer)
  - ➕ Add to Collection
  - 🔧 Insert (placeholder for T021)
- ✅ **Holder selection:**
  - Dropdown: holder variants
  - Dropdown: colors (if holder selected)
  - Checkbox: "No holder"
- ✅ **Preview tabs:**
  - Mesh preview
  - Grafica preview
  - Packaging preview
- ✅ **Metadata:**
  - Tags list (clickable to filter)
  - Collections list
  - File paths (expandable)

**Updates when:**
- Grid selection changes
- Product is favourited/tagged
- Holder selection changes

---

### T020: Favourites & Collections Views (~2 hours)

**Files to create:**
```
src/BoschMediaBrowser.Rhino/UI/Views/FavouritesView.cs
src/BoschMediaBrowser.Rhino/UI/Views/CollectionsView.cs
```

**FavouritesView Features:**
- ✅ Tab in main panel ("Favourites")
- ✅ Show all favourited products
- ✅ Same grid as main view (reuse ThumbnailGrid)
- ✅ Filter/sort favourites
- ✅ Remove from favourites action

**CollectionsView Features:**
- ✅ Tab in main panel ("Collections")
- ✅ **Left:** Collection list
  - Create new collection
  - Rename collection
  - Delete collection
- ✅ **Right:** Products in selected collection
  - Same grid (reuse ThumbnailGrid)
  - Add products (from main view)
  - Remove products
  - Reorder products (drag & drop?)

**Dialog for creating collection:**
- Simple input dialog
- Collection name (required)
- Optional description

---

## Implementation Order

### Phase 1: Layout Structure (30 min)
1. Update `MediaBrowserPanel.cs` to use split panels
2. Create placeholder areas for controls

### Phase 2: Core Controls (4 hours)
1. **CategoryTree** (1h)
2. **FiltersBar** (1h)
3. **ThumbnailGrid** (2h) - most complex

### Phase 3: Detail Pane (2 hours)
1. **DetailPane** layout (1h)
2. Action buttons wiring (1h)

### Phase 4: Views (2 hours)
1. **FavouritesView** (45min)
2. **CollectionsView** (1h 15min)

### Phase 5: Integration & Polish (1 hour)
1. Wire all events between controls
2. Test data flow
3. Handle edge cases
4. Loading states

---

## Technical Decisions

### Eto.Forms Controls to Use

```csharp
// Layout
Splitter - For resizable panels
StackLayout - For vertical/horizontal stacking
TableLayout - For structured layouts

// Data
TreeGridView - For category tree
GridView - For product grid (with custom cells)
Drawable - For custom thumbnail rendering (if needed)

// Input
Button, CheckBox, DropDown, TextBox
SearchBox - For search input

// Containers
TabControl - For Favourites/Collections tabs
GroupBox - For filter sections
Scrollable - For scrollable areas
```

### Event Flow

```
User Action → Control Event → Service Call → Update UI

Example:
1. User clicks category → CategoryTree.SelectionChanged
2. Fire event: CategorySelected(string category)
3. MediaBrowserPanel handles event
4. Call: SearchService.FilterAndSort(filters)
5. Update: ThumbnailGrid.SetProducts(filteredProducts)
6. Update: StatusBar ("Showing 42 of 150 products")
```

### State Management

```csharp
// MediaBrowserPanel will maintain:
private List<Product> _allProducts;
private List<Product> _filteredProducts;
private Product? _selectedProduct;
private Filters _currentFilters;

// When filters change:
_filteredProducts = _searchService.FilterAndSort(_allProducts, _currentFilters);
_thumbnailGrid.SetProducts(_filteredProducts);
```

---

## Testing Plan

### Without Rhino (Today)
- ✅ Build all controls
- ✅ Verify no compile errors
- ✅ Check UI layout in code review

### With Rhino (Tomorrow)
- ✅ Load plugin in Rhino 8
- ✅ Open panel with `ShowMediaBrowser`
- ✅ Verify products load from M:\ drive
- ✅ Test category navigation
- ✅ Test filters
- ✅ Test grid selection
- ✅ Test detail pane updates
- ✅ Test favourites/tags/collections
- ✅ Performance test (scroll, load thumbnails)

---

## Known Challenges

### Challenge 1: Thumbnail Loading Performance
**Issue:** Loading hundreds of thumbnails can freeze UI  
**Solution:** 
- Lazy load (only load visible thumbnails)
- Async thumbnail cache with cancellation tokens
- Show placeholder image while loading

### Challenge 2: Virtualized Grid
**Issue:** Eto.Forms doesn't have built-in virtualization  
**Solution:**
- Use GridView with pagination (50 items per page)
- Or: Custom Drawable with scroll handling
- Render only visible rows

### Challenge 3: Category Tree Depth
**Issue:** Deep hierarchies hard to navigate  
**Solution:**
- Default: collapse all, expand DIY/PRO
- Breadcrumb bar showing current path
- "Expand All" / "Collapse All" buttons

---

## File Structure After Implementation

```
src/BoschMediaBrowser.Rhino/
├── UI/
│   ├── MediaBrowserPanel.cs (updated)
│   ├── Controls/
│   │   ├── CategoryTree.cs (NEW)
│   │   ├── FiltersBar.cs (NEW)
│   │   ├── ThumbnailGrid.cs (NEW)
│   │   └── DetailPane.cs (NEW)
│   └── Views/
│       ├── FavouritesView.cs (NEW)
│       └── CollectionsView.cs (NEW)
```

---

## Success Criteria

✅ **T017 Complete When:**
- Category tree shows hierarchy from real data
- Clicking category filters products
- Filters update product list

✅ **T018 Complete When:**
- Grid shows products with thumbnails
- Selection works (single/multi)
- Pagination works smoothly
- DetailPane updates on selection

✅ **T019 Complete When:**
- Product details display correctly
- Preview image loads
- Favourite/tag actions work
- Holder selection functional

✅ **T020 Complete When:**
- Favourites view shows favourited products
- Collections can be created/renamed/deleted
- Products can be added/removed from collections

---

## Tomorrow's Test Checklist

```
[ ] Open Rhino 8 (license reactivated)
[ ] Load plugin (drag .rhp into Rhino)
[ ] Run: ShowMediaBrowser
[ ] Verify: Products load from M:\ drive
[ ] Test: Category navigation
[ ] Test: Search functionality
[ ] Test: Filters (range, category, tags)
[ ] Test: Grid selection & multi-select
[ ] Test: Detail pane updates
[ ] Test: Favourite a product
[ ] Test: Add tags to product
[ ] Test: Create collection
[ ] Test: Add products to collection
[ ] Test: Thumbnail loading performance
[ ] Check: Memory usage with many products
```

---

**Ready to implement! Let's build the UI controls.** 🚀
