"""
FastAPI server for managing product database
"""
from fastapi import FastAPI, HTTPException, Request
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse, JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
from pathlib import Path
import json
import subprocess
from datetime import datetime
import time
import threading
import sys

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent))

from audit_log import log_action, get_recent_logs, get_product_history

# Import our existing tools
autopop_product_json = None
scan_database_func = None

try:
    from autopop_product_json import autopop_product_json
except ImportError:
    pass

try:
    from bosch_scanner import scan_database as scan_database_func
except ImportError:
    pass

app = FastAPI(title="Bosch Product Database", version="1.0.0")

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Paths
BASE_PATH = Path(r"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__")
TOOLS_PATH = BASE_PATH / "Tools and Holders"

# In-memory cache for products
products_cache = {}
products_cache_timestamp = 0
CACHE_DURATION = 120  # Cache duration in seconds (optimized for 500+ products)
cache_lock = threading.Lock()

# Pydantic models
class ProductBase(BaseModel):
    productName: str
    description: str = ""
    sku: str = ""
    range: str = "PRO"
    category: str = ""
    subcategory: str = ""
    tags: List[str] = []
    notes: str = ""

class HolderInfo(BaseModel):
    variant: str
    color: str
    codArticol: str = ""
    fileName: Optional[str] = None
    fullPath: Optional[str] = None
    preview: Optional[str] = None

class ProductComplete(ProductBase):
    holders: List[HolderInfo] = []
    previews: dict = {}
    packaging: dict = {}
    metadata: dict = {}

# API Routes
@app.get("/")
async def root():
    """Serve the main web interface"""
    return FileResponse(Path(__file__).parent / "static" / "index.html")

def scan_products():
    """Scan filesystem for products - called by cache refresh"""
    products = []
    
    if not TOOLS_PATH.exists():
        return products
    
    # Scan for all JSON files
    for range_folder in TOOLS_PATH.iterdir():
        if not range_folder.is_dir() or range_folder.name.startswith('_'):
            continue
        
        for category_folder in range_folder.iterdir():
            if not category_folder.is_dir() or category_folder.name.startswith('_'):
                continue
            
            for product_folder in category_folder.iterdir():
                if not product_folder.is_dir() or product_folder.name.startswith('_'):
                    continue
                
                # Find JSON file
                json_files = list(product_folder.glob("*.json"))
                if json_files:
                    try:
                        with open(json_files[0], 'r', encoding='utf-8') as f:
                            data = json.load(f)
                            data['_folder'] = str(product_folder)
                            data['_jsonPath'] = str(json_files[0])
                            products.append(data)
                    except Exception as e:
                        print(f"Error reading {json_files[0]}: {e}")
    
    return products

@app.get("/api/products")
async def list_products(force_refresh: bool = False):
    """Get all products from database (cached)"""
    global products_cache, products_cache_timestamp
    
    current_time = time.time()
    
    # Check if cache is valid
    with cache_lock:
        cache_age = current_time - products_cache_timestamp
        
        if force_refresh or cache_age > CACHE_DURATION or not products_cache:
            print(f"🔄 Refreshing product cache... (age: {cache_age:.1f}s)")
            start = time.time()
            products = scan_products()
            products_cache = {"products": products, "count": len(products)}
            products_cache_timestamp = current_time
            print(f"✅ Cache refreshed in {time.time() - start:.2f}s ({len(products)} products)")
        else:
            print(f"📦 Using cached data (age: {cache_age:.1f}s)")
    
    return products_cache

@app.get("/api/products/{product_name}")
async def get_product(product_name: str):
    """Get a specific product by name"""
    # Search for product
    for range_folder in TOOLS_PATH.iterdir():
        if not range_folder.is_dir() or range_folder.name.startswith('_'):
            continue
        
        for category_folder in range_folder.iterdir():
            if not category_folder.is_dir() or category_folder.name.startswith('_'):
                continue
            
            for product_folder in category_folder.iterdir():
                if product_folder.name == product_name and product_folder.is_dir():
                    json_files = list(product_folder.glob("*.json"))
                    if json_files:
                        with open(json_files[0], 'r', encoding='utf-8') as f:
                            data = json.load(f)
                            data['_folder'] = str(product_folder)
                            data['_jsonPath'] = str(json_files[0])
                            return data
    
    raise HTTPException(status_code=404, detail="Product not found")

class NewProductRequest(BaseModel):
    folderPath: str
    range: str = "PRO"
    category: str = ""
    subcategory: str = ""
    description: str = ""
    sku: str = ""
    codArticol: str = ""
    tags: List[str] = []
    notes: str = ""
    holders: List[str] = []
    packaging: dict = {}

@app.post("/api/products/new")
async def create_product(request: NewProductRequest):
    """Create a new product JSON file"""
    folder_path = Path(request.folderPath)
    
    if not folder_path.exists():
        raise HTTPException(status_code=400, detail="Folder path does not exist")
    
    if not folder_path.is_dir():
        raise HTTPException(status_code=400, detail="Path is not a directory")
    
    # Extract product name from folder name
    product_name = folder_path.name
    
    # Check if JSON already exists
    existing_json = list(folder_path.glob("*.json"))
    if existing_json:
        raise HTTPException(status_code=400, detail="Product JSON already exists in this folder")
    
    # Create minimal JSON with all fields
    minimal_json = {
        "productName": product_name,
        "description": request.description,
        "sku": request.sku,
        "codArticol": request.codArticol,
        "range": request.range,
        "category": request.category,
        "subcategory": request.subcategory,
        "tags": request.tags,
        "notes": request.notes,
        "previews": {},
        "packaging": request.packaging if request.packaging else {},
        "holders": request.holders,
        "metadata": {
            "createdDate": json.dumps(None),  # Will be set by autopop
            "lastModified": json.dumps(None)
        }
    }
    
    # Save JSON
    json_path = folder_path / f"{product_name}.json"
    try:
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(minimal_json, f, indent=2, ensure_ascii=False)
        
        return {
            "success": True,
            "productName": product_name,
            "jsonPath": str(json_path),
            "message": "Product created successfully"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Failed to create JSON: {str(e)}")

@app.post("/api/products/{product_name}/auto-populate")
async def auto_populate_product(product_name: str, request: Request):
    """Auto-populate a product JSON from file structure"""
    if not autopop_product_json:
        raise HTTPException(status_code=501, detail="Auto-population module not available")
    
    # Get client info
    client_ip = request.client.host if request.client else "unknown"
    user_agent = request.headers.get("user-agent", "unknown")
    
    # Find product folder
    for range_folder in TOOLS_PATH.iterdir():
        if not range_folder.is_dir() or range_folder.name.startswith('_'):
            continue
        
        for category_folder in range_folder.iterdir():
            if not category_folder.is_dir() or category_folder.name.startswith('_'):
                continue
            
            for product_folder in category_folder.iterdir():
                if product_folder.name == product_name and product_folder.is_dir():
                    json_files = list(product_folder.glob("*.json"))
                    if json_files:
                        # Run auto-population
                        success = autopop_product_json(json_files[0])
                        if success:
                            # Log the action
                            log_action("auto_populate", product_name, client_ip, user_agent, 
                                     {"status": "success", "file": str(json_files[0])})
                            
                            # Return updated data
                            with open(json_files[0], 'r', encoding='utf-8') as f:
                                return json.load(f)
                        else:
                            log_action("auto_populate", product_name, client_ip, user_agent, 
                                     {"status": "failed"})
                            raise HTTPException(status_code=500, detail="Auto-population failed")
    
    raise HTTPException(status_code=404, detail="Product not found")

@app.put("/api/products/{product_name}")
async def update_product(product_name: str, product: ProductComplete):
    """Update a product JSON"""
    # Find product folder
    for range_folder in TOOLS_PATH.iterdir():
        if not range_folder.is_dir() or range_folder.name.startswith('_'):
            continue
        
        for category_folder in range_folder.iterdir():
            if not category_folder.is_dir() or category_folder.name.startswith('_'):
                continue
            
            for product_folder in category_folder.iterdir():
                if product_folder.name == product_name and product_folder.is_dir():
                    json_files = list(product_folder.glob("*.json"))
                    if json_files:
                        # Save updated JSON - include all fields, even None values
                        product_dict = product.dict(exclude_none=False, exclude_unset=False)
                        with open(json_files[0], 'w', encoding='utf-8') as f:
                            json.dump(product_dict, f, indent=2, ensure_ascii=False)
                        
                        # Invalidate cache
                        global products_cache_timestamp
                        products_cache_timestamp = 0
                        print("🔄 Cache invalidated after product update")
                        
                        return {"success": True, "message": "Product updated"}
    
    raise HTTPException(status_code=404, detail="Product not found")

@app.post("/api/cache/refresh")
async def refresh_cache():
    """Force refresh the product cache"""
    global products_cache_timestamp
    products_cache_timestamp = 0
    print("🔄 Cache invalidated by user request")
    # Trigger a refresh by calling list_products
    result = await list_products(force_refresh=True)
    return {"success": True, "message": f"Cache refreshed with {result['count']} products"}

@app.get("/api/holders")
async def list_holders():
    """Get all available holders"""
    holders_path = TOOLS_PATH / "Holders"
    holders = []
    
    if holders_path.exists():
        for file in holders_path.glob("*.3dm"):
            holders.append({
                "fileName": file.name,
            })
    
    return {"holders": holders}

@app.post("/api/scan")
async def scan_database(request: Request):
    """Trigger database scan"""
    if not scan_database_func:
        raise HTTPException(status_code=501, detail="Scanner not available")
    
    # Get client IP
    client_ip = request.client.host if request.client else "unknown"
    user_agent = request.headers.get("user-agent", "unknown")
    
    try:
        scan_database_func()
        
        # Log the action
        log_action("scan_database", None, client_ip, user_agent, {"status": "success"})
        
        return {"success": True, "message": "Database scanned successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/preview/{range_name}/{category}/{product_name}/{filename}")
async def get_preview(range_name: str, category: str, product_name: str, filename: str):
    """Serve preview images"""
    file_path = TOOLS_PATH / range_name / category / product_name / filename
    
    if not file_path.exists():
        raise HTTPException(status_code=404, detail="Preview not found")
    
    return FileResponse(file_path)

@app.get("/api/holder-preview/{filename}")
async def get_holder_preview(filename: str):
    """Serve holder preview images from Holders folder"""
    # Search in Holders folder structure - look in category/Previews subfolders
    holders_path = TOOLS_PATH / "Holders"
    
    # First try in category-specific Previews folders (e.g., Holders/Garden/Previews/)
    for category_folder in holders_path.iterdir():
        if category_folder.is_dir() and not category_folder.name.startswith('_'):
            # Try "Previews" folder
            previews_folder = category_folder / "Previews"
            if previews_folder.exists():
                preview_file = previews_folder / filename
                if preview_file.exists() and preview_file.is_file():
                    return FileResponse(preview_file)
            
            # Try "previews" folder (lowercase)
            previews_folder = category_folder / "previews"
            if previews_folder.exists():
                preview_file = previews_folder / filename
                if preview_file.exists() and preview_file.is_file():
                    return FileResponse(preview_file)
    
    # Fallback: search recursively for the file anywhere
    for file in holders_path.rglob(filename):
        if file.is_file():
            return FileResponse(file)
    
    raise HTTPException(status_code=404, detail="Holder preview not found")

class PathRequest(BaseModel):
    path: str

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
    if not os.path.exists(str(old_path)):
        raise HTTPException(status_code=404, detail="Old file not found")
    
    if os.path.exists(str(new_path)):
        raise HTTPException(status_code=400, detail="New filename already exists")
    
    try:
        # Rename 3D file
        shutil.move(str(old_path), str(new_path))
        print(f"✅ Renamed: {old_path.name} → {new_path.name}")
        
        # Try to rename preview if exists
        preview_renamed = False
        old_preview = old_path.parent / "Previews" / f"{old_path.stem}.jpg"
        if os.path.exists(str(old_preview)):
            new_preview = new_path.parent / "Previews" / f"{new_path.stem}.jpg"
            shutil.move(str(old_preview), str(new_preview))
            preview_renamed = True
            print(f"✅ Renamed preview: {old_preview.name} → {new_preview.name}")
        
        # Log action
        log_action("rename_file", request.productName, "system", "webapp",
                 {"old": str(old_path), "new": str(new_path), "holderIndex": request.holderIndex})
        
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

@app.post("/api/open-folder")
async def open_folder(request: PathRequest):
    """Open folder in Windows Explorer"""
    import subprocess
    try:
        # Use explorer.exe to open folder
        subprocess.Popen(['explorer', request.path])
        return {"success": True}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/reveal-file")
async def reveal_file(request: PathRequest):
    """Reveal a file in Windows Explorer"""
    import subprocess
    import os
    
    try:
        file_path_str = request.path
        print(f"Attempting to reveal: {file_path_str}")
        
        # Use os.path.exists() for UNC path support on Windows
        path_exists = os.path.exists(file_path_str)
        print(f"Path exists (os.path.exists): {path_exists}")
        
        if path_exists:
            # Use /select parameter to highlight the file
            # explorer.exe handles UNC paths natively
            result = subprocess.run(
                ['explorer', '/select,', file_path_str],
                capture_output=True,
                text=True,
                shell=False
            )
            print(f"Subprocess result: {result.returncode}")
            if result.stderr:
                print(f"Subprocess stderr: {result.stderr}")
            if result.stdout:
                print(f"Subprocess stdout: {result.stdout}")
            return {"success": True}
        else:
            print(f"File not found: {file_path_str}")
            raise HTTPException(status_code=404, detail=f"File not found: {file_path_str}")
    except HTTPException:
        raise
    except Exception as e:
        print(f"Error in reveal_file: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))

# Audit Log Endpoints
@app.get("/api/audit/recent")
async def get_audit_log(limit: int = 100):
    """Get recent audit log entries"""
    logs = get_recent_logs(limit)
    return {"logs": logs, "count": len(logs)}

@app.get("/api/audit/product/{product_name}")
async def get_product_audit_log(product_name: str, limit: int = 50):
    """Get audit history for a specific product"""
    logs = get_product_history(product_name, limit)
    return {"logs": logs, "count": len(logs), "product": product_name}

# Preview Extraction Endpoint
@app.post("/api/extract-previews")
async def extract_previews(request: Request):
    """Extract preview images from 3D files"""
    try:
        # Import the extraction module
        import sys
        sys.path.insert(0, str(Path(__file__).parent.parent))
        from extract_3d_previews import batch_extract_previews
        
        # Get client info
        client_ip = request.client.host if request.client else "unknown"
        user_agent = request.headers.get("user-agent", "unknown")
        
        # Extract previews from Holders folder
        holders_path = TOOLS_PATH / "Holders"
        count = batch_extract_previews(holders_path, recursive=True, overwrite=False)
        
        # Log the action
        log_action("extract_previews", None, client_ip, user_agent, 
                 {"extracted_count": count, "folder": str(holders_path)})
        
        return {
            "success": True,
            "extracted": count,
            "message": f"Extracted {count} preview images"
        }
    except ImportError:
        raise HTTPException(
            status_code=501, 
            detail="Preview extraction not available. Install: pip install rhino3dm Pillow"
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Extraction failed: {str(e)}")

# File Browse Endpoint
class BrowseFileRequest(BaseModel):
    folder: str
    fileTypes: List[str]

@app.post("/api/browse-file")
def browse_file(request: BrowseFileRequest):
    """Open file picker using Python tkinter - WORKS WITH ANYDESK/RDP"""
    import os
    
    try:
        print(f"\n{'='*60}")
        print(f"📂 FILE PICKER REQUEST")
        print(f"{'='*60}")
        print(f"Folder: {request.folder}")
        print(f"Exists: {os.path.exists(request.folder)}")
        
        # Verify folder exists
        if not os.path.exists(request.folder):
            print(f"❌ ERROR: Folder does not exist!")
            return {"selected": False, "error": "Folder does not exist"}
        
        print(f"🚀 Launching tkinter file picker...")
        
        # Use tkinter for better remote desktop support
        import tkinter as tk
        from tkinter import filedialog
        
        # Create root window (hidden)
        root = tk.Tk()
        root.withdraw()
        root.attributes('-topmost', True)  # Force on top for AnyDesk
        
        # Open file dialog
        file_path = filedialog.askopenfilename(
            parent=root,
            title='Select Preview Image',
            initialdir=request.folder,
            filetypes=[
                ('Image Files', '*.jpg *.jpeg *.png *.JPG *.JPEG *.PNG'),
                ('All Files', '*.*')
            ]
        )
        
        # Clean up
        root.destroy()
        
        print(f"\n📊 RESULT:")
        print(f"Selected: '{file_path}'")
        
        if file_path and os.path.exists(file_path):
            print(f"✅ SUCCESS: User selected file")
            print(f"   Path: {file_path}")
            print(f"   Name: {os.path.basename(file_path)}")
            print(f"{'='*60}\n")
            return {
                "selected": True,
                "filePath": file_path,
                "fileName": os.path.basename(file_path)
            }
        else:
            print(f"ℹ️ No file selected (user cancelled)")
            print(f"{'='*60}\n")
            return {"selected": False}
            
    except Exception as e:
        print(f"❌ EXCEPTION: {str(e)}")
        import traceback
        traceback.print_exc()
        print(f"{'='*60}\n")
        raise HTTPException(status_code=500, detail=str(e))

# Mount static files
app.mount("/static", StaticFiles(directory=Path(__file__).parent / "static"), name="static")

if __name__ == "__main__":
    import uvicorn
    print("Starting Bosch Product Database Web UI...")
    print("Open http://localhost:8000 in your browser")
    uvicorn.run("server:app", host="0.0.0.0", port=8000, reload=True)
