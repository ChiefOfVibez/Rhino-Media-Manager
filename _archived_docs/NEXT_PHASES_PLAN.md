# Next Development Phases - Action Plan

**Priority Order:** Phase 1 → Phase 2 → Phase 3  
**Requested by User:** Steps 2, 3, 1

---

## 🎯 Phase 1: Make All Fields Editable

**Goal:** Enable editing of every product parameter in the web UI

### Tasks Breakdown

#### 1.1 Basic Information Fields (30 min)
**Currently:** Read-only display  
**Target:** Editable with validation

**Fields to enable:**
- [x] Description (already editable)
- [ ] SKU (add input field)
- [ ] Category (dropdown or text)
- [ ] Subcategory (text input)
- [ ] Range (dropdown: PRO/DIY)
- [ ] Notes (textarea - already editable)

**Implementation:**
```html
<!-- Replace static display with input -->
<input x-model="editingProduct.sku" 
       type="text"
       class="form-input" 
       placeholder="0.601.9H1.100">
```

#### 1.2 Tags Management Enhancement (15 min)
**Currently:** Can add/remove tags  
**Enhancement:** Better UX

**Features:**
- [ ] Tag suggestions (from other products)
- [ ] Bulk tag operations
- [ ] Tag validation (no special chars)

#### 1.3 Holder Array Editing (60 min)
**Currently:** Static holder list  
**Target:** Full CRUD operations

**Features:**
- [ ] Edit holder in-place (variant, color, cod)
- [ ] Add new holder to product
- [ ] Remove holder from product
- [ ] Reorder holders (drag & drop optional)

**UI Mockup:**
```
Holders (4 total) [+ Add Holder]
├─ Variant: [Tego ▼] Color: [RAL7043 ▼] Cod: [BO.161.9LL8600] [❌]
├─ Variant: [Tego ▼] Color: [RAL9006 ▼] Cod: [BO.161.9LL8601] [❌]
├─ Variant: [Traverse ▼] Color: [RAL7043 ▼] Cod: [NN.ALL.BO07802] [❌]
└─ Variant: [Traverse ▼] Color: [RAL9006 ▼] Cod: [NN.ALL.BO07803] [❌]
```

**Implementation:**
```javascript
addHolder() {
    if (!this.editingProduct.holders) {
        this.editingProduct.holders = [];
    }
    this.editingProduct.holders.push({
        variant: '',
        color: '',
        codArticol: '',
        fileName: '',
        fullPath: '',
        preview: ''
    });
},

removeHolder(index) {
    if (confirm('Remove this holder?')) {
        this.editingProduct.holders.splice(index, 1);
    }
},

updateHolder(index, field, value) {
    this.editingProduct.holders[index][field] = value;
    // Deep clone to trigger reactivity
    this.editingProduct = JSON.parse(JSON.stringify(this.editingProduct));
}
```

#### 1.4 File Path Overrides (30 min)
**Currently:** Auto-populated from filesystem  
**Target:** Allow manual override

**Fields:**
- [ ] Mesh 3D file path
- [ ] Mesh preview path
- [ ] Grafica 3D file path
- [ ] Grafica preview path
- [ ] Packaging 3D file path
- [ ] Packaging preview path

**UI:**
```html
<div class="flex gap-2">
    <input x-model="editingProduct.previews.mesh3d" 
           type="text" 
           placeholder="GBL 18V-750_mesh.3dm"
           class="flex-1">
    <button @click="autoPopMeshPath()">🔄 Auto</button>
    <button @click="browseMeshFile()">📁 Browse</button>
</div>
```

#### 1.5 Validation & Dirty State (30 min)
**Features:**
- [ ] Required field validation
- [ ] Unsaved changes warning
- [ ] Revert changes button
- [ ] Visual indicators for modified fields

**Implementation:**
```javascript
data: {
    originalProduct: null,
    isDirty: false,
},

editProduct(product) {
    this.editingProduct = JSON.parse(JSON.stringify(product));
    this.originalProduct = JSON.parse(JSON.stringify(product));
    this.isDirty = false;
},

checkDirty() {
    this.isDirty = JSON.stringify(this.editingProduct) !== 
                   JSON.stringify(this.originalProduct);
},

revertChanges() {
    if (confirm('Discard all changes?')) {
        this.editingProduct = JSON.parse(JSON.stringify(this.originalProduct));
        this.isDirty = false;
    }
}
```

### Testing Checklist
- [ ] Can edit all basic fields
- [ ] Can add/remove/edit holders
- [ ] Can override file paths manually
- [ ] Unsaved changes warning works
- [ ] Revert changes works
- [ ] Validation prevents invalid data
- [ ] Save persists all changes
- [ ] Auto-populate preserves manual edits

---

## 🎯 Phase 2: File Renaming on Network

**Goal:** Automatically rename 3D files when holder parameters change

### Tasks Breakdown

#### 2.1 Detect Parameter Changes (30 min)
**Track changes to:**
- Holder variant
- Holder color
- Holder cod articol

**Implementation:**
```javascript
getOldHolderFileName(holder) {
    const { variant, color, codArticol } = holder;
    if (!variant || !color || !codArticol) return null;
    return `${variant}_${color}_${codArticol}.3dm`;
},

detectFileRename(oldHolder, newHolder) {
    const oldName = this.getOldHolderFileName(oldHolder);
    const newName = this.getOldHolderFileName(newHolder);
    return oldName !== newName ? { oldName, newName } : null;
}
```

#### 2.2 Backend File Rename Endpoint (60 min)
**New API endpoint:** `POST /api/rename-file`

**Request:**
```json
{
  "oldPath": "M:\\...\\Tego_RAL7043_BO.161.9LL8600.3dm",
  "newPath": "M:\\...\\Tego_RAL7043_BO.161.9LL8601.3dm",
  "productName": "GBL 18V-750",
  "holderIndex": 0
}
```

**Response:**
```json
{
  "success": true,
  "oldPath": "...",
  "newPath": "...",
  "previewRenamed": true
}
```

**server.py:**
```python
class RenameFileRequest(BaseModel):
    oldPath: str
    newPath: str
    productName: str
    holderIndex: int

@app.post("/api/rename-file")
async def rename_file(request: RenameFileRequest):
    """Rename a holder file on network"""
    import os
    import shutil
    
    old_path = Path(request.oldPath)
    new_path = Path(request.newPath)
    
    # Validate paths
    if not old_path.exists():
        raise HTTPException(status_code=404, detail="Old file not found")
    
    if new_path.exists():
        raise HTTPException(status_code=400, detail="New filename already exists")
    
    try:
        # Rename 3D file
        shutil.move(str(old_path), str(new_path))
        
        # Try to rename preview if exists
        preview_renamed = False
        old_preview = old_path.parent / "Previews" / f"{old_path.stem}.jpg"
        if old_preview.exists():
            new_preview = new_path.parent / "Previews" / f"{new_path.stem}.jpg"
            shutil.move(str(old_preview), str(new_preview))
            preview_renamed = True
        
        # Log action
        log_action("rename_file", request.productName, "system", "webapp",
                 {"old": str(old_path), "new": str(new_path)})
        
        return {
            "success": True,
            "oldPath": str(old_path),
            "newPath": str(new_path),
            "previewRenamed": preview_renamed
        }
    
    except PermissionError:
        raise HTTPException(status_code=403, detail="Permission denied. File may be open in Rhino.")
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
```

#### 2.3 Frontend Integration (45 min)
**When to trigger:**
- On holder parameter change (variant, color, cod)
- Before save
- With user confirmation

**Implementation:**
```javascript
async saveProduct() {
    // Check for file renames needed
    const renames = [];
    
    this.editingProduct.holders.forEach((holder, index) => {
        const original = this.originalProduct.holders[index];
        if (original) {
            const rename = this.detectFileRename(original, holder);
            if (rename && holder.fullPath) {
                renames.push({
                    holderIndex: index,
                    oldPath: holder.fullPath,
                    newPath: holder.fullPath.replace(
                        rename.oldName,
                        rename.newName
                    )
                });
            }
        }
    });
    
    // If renames detected, ask user
    if (renames.length > 0) {
        const msg = `This will rename ${renames.length} file(s) on the network:\n\n` +
                    renames.map(r => `${Path(r.oldPath).name} → ${Path(r.newPath).name}`).join('\n') +
                    '\n\nContinue?';
        
        if (!confirm(msg)) {
            return;
        }
        
        // Perform renames
        for (const rename of renames) {
            try {
                const response = await fetch('/api/rename-file', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        oldPath: rename.oldPath,
                        newPath: rename.newPath,
                        productName: this.editingProduct.productName,
                        holderIndex: rename.holderIndex
                    })
                });
                
                if (response.ok) {
                    // Update paths in product
                    this.editingProduct.holders[rename.holderIndex].fullPath = rename.newPath;
                    this.editingProduct.holders[rename.holderIndex].fileName = Path(rename.newPath).name;
                } else {
                    const error = await response.json();
                    alert(`Failed to rename: ${error.detail}`);
                    return; // Abort save
                }
            } catch (error) {
                alert(`Error renaming file: ${error.message}`);
                return; // Abort save
            }
        }
    }
    
    // Continue with normal save
    // ... existing save code ...
}
```

#### 2.4 Error Handling (30 min)
**Scenarios:**
1. File is open in Rhino → Permission denied
2. New filename already exists → Conflict
3. Network path not accessible → Timeout
4. Partial rename failure → Rollback

**Implementation:**
```javascript
handleRenameError(error, rename) {
    if (error.detail?.includes('Permission denied')) {
        alert('⚠️ Cannot rename file - it may be open in Rhino.\n\n' +
              'Please close the file and try again.');
    } else if (error.detail?.includes('already exists')) {
        alert('❌ A file with that name already exists.\n\n' +
              `Target: ${Path(rename.newPath).name}`);
    } else {
        alert(`❌ Rename failed: ${error.detail || 'Unknown error'}`);
    }
}
```

### Testing Checklist
- [ ] Can rename holder file (variant change)
- [ ] Can rename holder file (color change)
- [ ] Can rename holder file (cod change)
- [ ] Preview file renamed together
- [ ] Confirmation dialog shows
- [ ] Error handling for open files
- [ ] Error handling for existing files
- [ ] Paths updated in JSON
- [ ] Audit log records rename
- [ ] Multiple renames in one save work

---

## 🎯 Phase 3: Toast Notifications

**Goal:** Replace all `alert()` with modern toast notifications

### Tasks Breakdown

#### 3.1 Choose Implementation (15 min)
**Options:**
1. **Custom Alpine.js** (recommended) - 0 dependencies
2. **Toastify** - Lightweight library (3KB)
3. **Alpine Notify Plugin** - Official Alpine plugin

**Decision:** Custom Alpine.js (most flexible, no dependencies)

#### 3.2 Create Toast Component (45 min)
**File:** Add to `index.html` `<head>` section

```html
<style>
.toast {
    position: fixed;
    top: 20px;
    right: 20px;
    min-width: 300px;
    max-width: 500px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    padding: 16px;
    margin-bottom: 10px;
    display: flex;
    align-items: start;
    gap: 12px;
    animation: slideIn 0.3s ease-out;
    z-index: 9999;
}

.toast.success { border-left: 4px solid #10b981; }
.toast.error { border-left: 4px solid #ef4444; }
.toast.warning { border-left: 4px solid #f59e0b; }
.toast.info { border-left: 4px solid #3b82f6; }

@keyframes slideIn {
    from {
        transform: translateX(400px);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

.toast.removing {
    animation: slideOut 0.3s ease-in forwards;
}

@keyframes slideOut {
    to {
        transform: translateX(400px);
        opacity: 0;
    }
}
</style>

<!-- Toast Container -->
<div x-data="toastManager()" 
     x-init="$watch('toasts', () => console.log('Toasts:', toasts))"
     class="toast-container">
    <template x-for="toast in toasts" :key="toast.id">
        <div :class="'toast ' + toast.type + (toast.removing ? ' removing' : '')"
             x-show="!toast.hidden"
             x-transition>
            <div class="toast-icon">
                <span x-show="toast.type === 'success'">✅</span>
                <span x-show="toast.type === 'error'">❌</span>
                <span x-show="toast.type === 'warning'">⚠️</span>
                <span x-show="toast.type === 'info'">ℹ️</span>
            </div>
            <div class="flex-1">
                <div class="font-semibold" x-text="toast.title"></div>
                <div class="text-sm text-gray-600" x-text="toast.message"></div>
            </div>
            <button @click="removeToast(toast.id)" class="text-gray-400 hover:text-gray-600">
                ✕
            </button>
        </div>
    </template>
</div>

<script>
function toastManager() {
    return {
        toasts: [],
        nextId: 1,
        
        show(type, title, message, duration = 5000) {
            const id = this.nextId++;
            const toast = { id, type, title, message, hidden: false, removing: false };
            this.toasts.push(toast);
            
            if (duration > 0) {
                setTimeout(() => this.removeToast(id), duration);
            }
            
            return id;
        },
        
        success(title, message, duration) {
            return this.show('success', title, message, duration);
        },
        
        error(title, message, duration = 0) {
            return this.show('error', title, message, duration);  // Errors don't auto-dismiss
        },
        
        warning(title, message, duration) {
            return this.show('warning', title, message, duration);
        },
        
        info(title, message, duration) {
            return this.show('info', title, message, duration);
        },
        
        removeToast(id) {
            const toast = this.toasts.find(t => t.id === id);
            if (toast) {
                toast.removing = true;
                setTimeout(() => {
                    this.toasts = this.toasts.filter(t => t.id !== id);
                }, 300);
            }
        }
    }
}

// Global toast function
window.toast = {
    success: (title, message) => Alpine.store('toasts').success(title, message),
    error: (title, message) => Alpine.store('toasts').error(title, message),
    warning: (title, message) => Alpine.store('toasts').warning(title, message),
    info: (title, message) => Alpine.store('toasts').info(title, message),
};

// Alpine store for global access
document.addEventListener('alpine:init', () => {
    Alpine.store('toasts', toastManager());
});
</script>
```

#### 3.3 Replace All Alerts (60 min)
**Find:** All `alert(...)` calls  
**Replace:** With appropriate toast

**Examples:**
```javascript
// OLD
alert('✅ Product saved successfully!');

// NEW
window.toast.success('Product Saved', 'All changes have been saved successfully.');
```

```javascript
// OLD
alert('❌ Failed to save product');

// NEW
window.toast.error('Save Failed', 'Could not save product. Please try again.');
```

**List of alerts to replace:**
1. Product save success/failure
2. Auto-populate success/failure
3. Extract previews success/failure
4. Scan database success/failure
5. Reveal in Explorer failures
6. Browse file picker failures
7. Validation errors
8. Network errors

#### 3.4 Add Progress Toasts (30 min)
**For long operations:**
- Auto-populate
- Extract previews
- Database scan
- File rename

**Implementation:**
```javascript
async autoPopulate(product) {
    // Show progress toast
    const toastId = window.toast.info(
        'Auto-Populating', 
        `Scanning files for ${product.productName}...`
    );
    
    try {
        // ... auto-populate logic ...
        
        // Update toast to success
        Alpine.store('toasts').removeToast(toastId);
        window.toast.success(
            'Auto-Populate Complete',
            `Found all files for ${product.productName}`
        );
    } catch (error) {
        Alpine.store('toasts').removeToast(toastId);
        window.toast.error(
            'Auto-Populate Failed',
            error.message
        );
    }
}
```

### Testing Checklist
- [ ] Toast appears on all operations
- [ ] Success toasts auto-dismiss (5s)
- [ ] Error toasts stay visible
- [ ] Multiple toasts stack correctly
- [ ] Can manually dismiss toasts
- [ ] Toasts animate smoothly
- [ ] No more alert() popups
- [ ] Toast types styled correctly

---

## 📊 Implementation Timeline

| Phase | Task | Hours | Priority |
|-------|------|-------|----------|
| **Phase 1** | Basic fields editable | 0.5 | HIGH |
| | Holder array editing | 1.0 | HIGH |
| | File path overrides | 0.5 | MEDIUM |
| | Validation & dirty state | 0.5 | HIGH |
| | **Subtotal** | **2.5h** | |
| **Phase 2** | Detect changes | 0.5 | HIGH |
| | Backend rename endpoint | 1.0 | HIGH |
| | Frontend integration | 0.75 | HIGH |
| | Error handling | 0.5 | MEDIUM |
| | **Subtotal** | **2.75h** | |
| **Phase 3** | Toast component | 0.75 | MEDIUM |
| | Replace all alerts | 1.0 | MEDIUM |
| | Progress toasts | 0.5 | LOW |
| | **Subtotal** | **2.25h** | |
| **TOTAL** | | **7.5h** | |

---

## 🚀 Quick Start Commands

### Start Phase 1
```bash
# No setup needed, start editing index.html
```

### Test Phase 1
```bash
# Open browser
http://localhost:8000
# Edit product → Test all fields editable
```

### Start Phase 2
```bash
# Edit server.py
# Add rename endpoint
```

### Test Phase 2
```bash
# Change holder cod → Save → Check file renamed on M:\ drive
```

### Start Phase 3
```bash
# Add toast component to index.html <head>
# Replace alerts one by one
```

---

## 📝 Notes

- All phases are independent (can be done in any order)
- Phase 2 depends on Phase 1 (need editable holders)
- Phase 3 is purely cosmetic (can be done last)
- Total estimated time: ~7.5 hours
- Can be split across multiple sessions

---

**Ready to start Phase 1!**
