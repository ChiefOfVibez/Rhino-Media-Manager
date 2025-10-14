"""
Auto-populate product JSON files with missing data
- Tool 3DM file paths (mesh and grafica)
- Proxy mesh paths
- Holder previews
- Packaging details
"""
import json
import sys
from pathlib import Path
from datetime import datetime
from typing import Dict, Optional, List

# Add parent to path
sys.path.insert(0, str(Path(__file__).parent.parent))

BASE_PATH = Path(r"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__")
TOOLS_PATH = BASE_PATH / "Tools and Holders"

def find_file(folder: Path, patterns: List[str]) -> Optional[str]:
    """Find first matching file by patterns (case insensitive)"""
    for pattern in patterns:
        for file in folder.glob(pattern):
            if file.is_file():
                return str(file)
        # Try case insensitive
        for file in folder.iterdir():
            if file.is_file() and file.name.lower() == pattern.lower():
                return str(file)
    return None

def autopop_product_json(product_folder: Path, force: bool = False) -> Dict:
    """Auto-populate a single product JSON with file paths"""
    product_name = product_folder.name
    json_path = product_folder / f"{product_name}.json"
    
    if not json_path.exists():
        raise FileNotFoundError(f"JSON not found: {json_path}")
    
    # Load existing JSON
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    print(f"📝 Auto-populating: {product_name}")
    changes = []
    
    # === CRITICAL: ENSURE REQUIRED FIELDS ===
    # Product model requires these fields to be present and non-null
    
    # 1. productName - REQUIRED
    if not data.get('productName'):
        data['productName'] = product_name
        changes.append(f"✓ FIXED: Added missing productName field")
    
    # 2. description - REQUIRED (can be empty string)
    if 'description' not in data:
        data['description'] = ""
        changes.append(f"✓ FIXED: Added missing description field")
    
    # 3. range - REQUIRED
    if not data.get('range'):
        data['range'] = ""
        changes.append(f"⚠️  WARNING: range field empty (will be set from folder path)")
    
    # 4. category - REQUIRED
    if not data.get('category'):
        data['category'] = ""
        changes.append(f"⚠️  WARNING: category field empty (will be set from folder path)")
    
    # 5. tags - REQUIRED (can be empty list)
    if 'tags' not in data:
        data['tags'] = []
        changes.append(f"✓ FIXED: Added missing tags field")
    
    # 6. holders - REQUIRED (can be empty list)
    if 'holders' not in data:
        data['holders'] = []
        changes.append(f"✓ FIXED: Added missing holders field")
    
    # 7. Fix malformed holders (strings instead of objects)
    if isinstance(data.get('holders'), list):
        fixed_holders = []
        for i, holder in enumerate(data['holders']):
            if isinstance(holder, str):
                # Parse string format: "Variant_Color_Code"
                parts = holder.split('_')
                if len(parts) >= 3:
                    fixed_holder = {
                        "variant": parts[0],
                        "color": parts[1],
                        "codArticol": '_'.join(parts[2:]),
                        "fileName": f"{holder}.3dm",
                        "fullPath": "",
                        "preview": ""
                    }
                    fixed_holders.append(fixed_holder)
                    changes.append(f"✓ FIXED: Converted holder string to object: {holder}")
                else:
                    changes.append(f"⚠️  WARNING: Malformed holder string: {holder}")
            elif isinstance(holder, dict):
                fixed_holders.append(holder)
        
        if len(fixed_holders) != len(data['holders']):
            data['holders'] = fixed_holders
            changes.append(f"✓ FIXED: Replaced {len(data['holders']) - len(fixed_holders)} invalid holders")
    
    # === AUTO-EXTRACT RANGE AND CATEGORY FROM FOLDER PATH ===
    # Path structure: ...\\Tools and Holders\\{RANGE}\\{CATEGORY}\\{ProductName}
    try:
        path_parts = product_folder.parts
        tools_idx = next(i for i, part in enumerate(path_parts) if part == 'Tools and Holders')
        
        if tools_idx + 2 < len(path_parts):
            extracted_range = path_parts[tools_idx + 1]  # DIY or PRO
            extracted_category = path_parts[tools_idx + 2]  # Drills, Garden, etc.
            
            if not data.get('range') or force:
                data['range'] = extracted_range
                changes.append(f"✓ Set range to '{extracted_range}' from folder path")
            
            if not data.get('category') or force:
                data['category'] = extracted_category
                changes.append(f"✓ Set category to '{extracted_category}' from folder path")
    except (StopIteration, IndexError):
        pass  # Not in expected folder structure
    
    # === TOOL 3DM FILES ===
    if 'previews' not in data:
        data['previews'] = {}
    
    previews = data['previews']
    
    # 1. Mesh 3D file
    if force or 'mesh3d' not in previews or not previews['mesh3d'].get('fullPath'):
        mesh_path = find_file(product_folder, [
            f"{product_name}_mesh.3dm",
            f"{product_name}_Mesh.3dm",
            f"{product_name}.3dm"
        ])
        if mesh_path:
            previews['mesh3d'] = {
                "fileName": Path(mesh_path).name,
                "fullPath": mesh_path
            }
            changes.append(f"✓ Added mesh3d path")
    
    # 2. Mesh Preview (PNG/JPG)
    if force or 'meshPreview' not in previews or not previews.get('meshPreview', {}).get('fullPath'):
        preview_path = find_file(product_folder, [
            f"{product_name}_mesh.png",
            f"{product_name}_mesh.jpg",
            f"{product_name}_Mesh.PNG",
            f"{product_name}_Mesh.JPG",
            f"{product_name}_Mesh.jpg",
            f"{product_name}.jpg",
            f"{product_name}.png"
        ])
        if preview_path:
            previews['meshPreview'] = {
                "fileName": Path(preview_path).name,
                "fullPath": preview_path
            }
            changes.append(f"✓ Added meshPreview path")
    
    # 3. Grafica 3D file
    if force or 'grafica3d' not in previews or not previews.get('grafica3d', {}).get('fullPath'):
        grafica_path = find_file(product_folder, [
            f"{product_name}_grafica.3dm",
            f"{product_name}_Grafica.3dm",
            f"{product_name}_graphics.3dm"
        ])
        if grafica_path:
            previews['grafica3d'] = {
                "fileName": Path(grafica_path).name,
                "fullPath": grafica_path
            }
            changes.append(f"✓ Added grafica3d path")
    
    # 4. Grafica Preview
    if force or 'graficaPreview' not in previews or not previews.get('graficaPreview', {}).get('fullPath'):
        grafica_preview = find_file(product_folder, [
            f"{product_name}_grafica.png",
            f"{product_name}_grafica.jpg",
            f"{product_name}_Grafica.PNG",
            f"{product_name}_Grafica.jpg"
        ])
        if grafica_preview:
            previews['graficaPreview'] = {
                "fileName": Path(grafica_preview).name,
                "fullPath": grafica_preview
            }
            changes.append(f"✓ Added graficaPreview path")
    
    # === PROXY MESH ===
    if force or 'proxyMesh' not in previews or not previews.get('proxyMesh', {}).get('fullPath'):
        proxy_path = find_file(product_folder, [
            f"{product_name}_proxy mesh.3dm",
            f"{product_name}_proxy_mesh.3dm",
            f"{product_name}_Proxy Mesh.3dm",
            f"{product_name}_proxy.3dm"
        ])
        if proxy_path:
            previews['proxyMesh'] = {
                "fileName": Path(proxy_path).name,
                "fullPath": proxy_path,
                "description": "Lightweight mesh for viewport display, switches to full mesh for rendering"
            }
            changes.append(f"✓ Added proxyMesh path")
    
    # === PACKAGING ===
    if 'packaging' not in data:
        data['packaging'] = {}
    
    packaging = data['packaging']
    
    if force or not packaging.get('fullPath'):
        packaging_path = find_file(product_folder, [
            f"{product_name}_packaging.3dm",
            f"{product_name}_Packaging.3dm",
            f"{product_name}_package.3dm"
        ])
        if packaging_path:
            packaging['fileName'] = Path(packaging_path).name
            packaging['fullPath'] = packaging_path
            changes.append(f"✓ Added packaging 3DM path")
    
    if force or not packaging.get('previewPath'):
        pkg_preview = find_file(product_folder, [
            f"{product_name}_packaging.png",
            f"{product_name}_packaging.jpg",
            f"{product_name}_Packaging.PNG",
            f"{product_name}_package.jpg"
        ])
        if pkg_preview:
            packaging['preview'] = Path(pkg_preview).name
            packaging['previewPath'] = pkg_preview
            changes.append(f"✓ Added packaging preview")
    
    # === HOLDERS (FILES AND PREVIEWS) ===
    if 'holders' in data and isinstance(data['holders'], list):
        for idx, holder in enumerate(data['holders']):
            # Skip if holder is a string (malformed data)
            if not isinstance(holder, dict):
                continue
            
            variant = holder.get('variant', '')
            color = holder.get('color', '')
            cod = holder.get('codArticol', '')
            
            # === Find holder 3DM file ===
            if force or not holder.get('fullPath'):
                # Look in Holders folder structure
                # Structure: .../{RANGE}/{CATEGORY}/Holders/{variant}_{color}_{cod}.3dm
                category_folder = product_folder.parent
                holders_base = TOOLS_PATH / "Holders"
                
                # Get category from path
                category_name = category_folder.name
                
                # Search locations (prioritize category-specific paths)
                search_paths = [
                    holders_base / category_name,  # Central Holders/{category} (PRIMARY)
                    category_folder / "Holders",  # Same category
                    holders_base  # Root Holders folder (fallback)
                ]
                
                holder_filename = holder.get('fileName', f"{variant}_{color}_{cod}.3dm")
                
                # Try multiple filename variations (handle double .3dm extension)
                filename_variations = [
                    holder_filename,                    # Normal: variant_color_code.3dm
                    f"{holder_filename}.3dm",          # Double extension: variant_color_code.3dm.3dm
                    holder_filename.replace('.3dm', '') + '.3dm'  # Clean and add single .3dm
                ]
                
                for search_path in search_paths:
                    if search_path.exists():
                        found = False
                        for filename_var in filename_variations:
                            holder_file = search_path / filename_var
                            if holder_file.exists():
                                # Update both filename and fullPath
                                holder['fileName'] = filename_var
                                holder['fullPath'] = str(holder_file).replace('\\', '/')
                                changes.append(f"✓ Found holder file: {variant} - {color} at {search_path.name}/")
                                found = True
                                break
                        if found:
                            break
            
            # === Find holder preview ===
            if force or not holder.get('preview'):
                category_folder = product_folder.parent
                
                # Search for previews in multiple locations
                preview_search_paths = [
                    category_folder / 'Holders' / 'previews',
                    category_folder / 'Holders' / 'Previews',
                    TOOLS_PATH / 'Holders' / category_folder.name / 'previews',
                    TOOLS_PATH / 'Holders' / category_folder.name / 'Previews'
                ]
                
                preview_patterns = [
                    f"{variant}_{color}_{cod}.png",
                    f"{variant}_{color}_{cod}.jpg",
                    f"{variant}_{color}.png",
                    f"{variant}_{color}.jpg"
                ]
                
                for previews_folder in preview_search_paths:
                    if previews_folder.exists():
                        for pattern in preview_patterns:
                            preview_file = previews_folder / pattern
                            if preview_file.exists():
                                holder['preview'] = str(preview_file)
                                changes.append(f"✓ Added holder preview: {variant} - {color}")
                                break
                        if holder.get('preview'):
                            break
    
    # === HOLDER TRANSFORMS ===
    # Initialize transform data for each holder variant (transform strategy)
    # Use variant as key (color is irrelevant for transforms - all same-variant holders have same position)
    if 'holders' in data and isinstance(data['holders'], list) and len(data['holders']) > 0:
        # ALWAYS ensure holderTransforms dict exists
        if 'holderTransforms' not in data or not isinstance(data.get('holderTransforms'), dict):
            data['holderTransforms'] = {}
            changes.append(f"✓ Initialized holderTransforms dictionary")
        
        # Set reference holder if not set (default to Tego, or first holder variant)
        if not data.get('referenceHolder'):
            # Check if Tego exists
            tego_holder = next((h for h in data['holders'] if h.get('variant', '').lower() == 'tego'), None)
            if tego_holder:
                data['referenceHolder'] = tego_holder.get('variant', 'Tego')
            else:
                data['referenceHolder'] = data['holders'][0].get('variant', 'Default')
            changes.append(f"✓ Set referenceHolder to '{data['referenceHolder']}'")
        
        # ALWAYS ensure transforms exist for all holder variants
        variants_processed = set()
        for holder in data['holders']:
            variant = holder.get('variant', '')
            
            if variant and variant not in variants_processed:
                if variant not in data['holderTransforms']:
                    is_reference = (variant == data.get('referenceHolder'))
                    data['holderTransforms'][variant] = {
                        "translation": [0.0, 0.0, 0.0],
                        "rotation": [0.0, 0.0, 0.0],
                        "scale": [1.0, 1.0, 1.0],
                        "notes": "Reference position" if is_reference else "Needs adjustment"
                    }
                    changes.append(f"✓ Initialized transform for holder variant '{variant}'")
                variants_processed.add(variant)
    
    # === METADATA ===
    if 'metadata' not in data:
        data['metadata'] = {}
    
    metadata = data['metadata']
    
    # Fix invalid date strings (e.g., "null" as a string)
    if 'createdDate' in metadata:
        if metadata['createdDate'] == "null" or metadata['createdDate'] == "":
            metadata['createdDate'] = None
            changes.append(f"✓ FIXED: Converted invalid createdDate string 'null' to null")
    
    if 'lastModified' in metadata:
        if metadata['lastModified'] == "null" or metadata['lastModified'] == "":
            metadata['lastModified'] = datetime.now().isoformat()
            changes.append(f"✓ FIXED: Replaced invalid lastModified with current timestamp")
    
    if not metadata.get('lastModified'):
        metadata['lastModified'] = datetime.now().isoformat()
        changes.append(f"✓ Updated lastModified timestamp")
    else:
        # Always update on autopop
        metadata['lastModified'] = datetime.now().isoformat()
    
    # Save updated JSON
    if changes:
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        
        print(f"  Changes made:")
        for change in changes:
            print(f"    {change}")
        print()
        
        return {
            "success": True,
            "productName": product_name,
            "changes": changes,
            "jsonPath": str(json_path)
        }
    else:
        print(f"  ⚠️ No changes needed\n")
        return {
            "success": True,
            "productName": product_name,
            "changes": [],
            "message": "No changes needed"
        }

def autopop_all_products(force: bool = False):
    """Auto-populate all products in database"""
    if not TOOLS_PATH.exists():
        print(f"ERROR: Tools path not found: {TOOLS_PATH}")
        return
    
    results = []
    
    for range_folder in TOOLS_PATH.iterdir():
        if not range_folder.is_dir() or range_folder.name.startswith('_'):
            continue
        
        for category_folder in range_folder.iterdir():
            if not category_folder.is_dir() or category_folder.name.startswith('_'):
                continue
            
            for product_folder in category_folder.iterdir():
                if not product_folder.is_dir() or product_folder.name.startswith('_'):
                    continue
                
                # Skip non-product folders
                if product_folder.name.lower() in ['holders', 'previews', 'temp']:
                    continue
                
                json_file = product_folder / f"{product_folder.name}.json"
                if json_file.exists():
                    try:
                        result = autopop_product_json(product_folder, force=force)
                        results.append(result)
                    except Exception as e:
                        print(f"  ✗ ERROR: {e}\n")
                        results.append({
                            "success": False,
                            "productName": product_folder.name,
                            "error": str(e)
                        })
    
    return results

def main():
    """CLI entry point"""
    import argparse
    
    parser = argparse.ArgumentParser(description="Auto-populate product JSONs with missing data")
    parser.add_argument('--all', action='store_true', help='Process all products')
    parser.add_argument('--product', type=str, help='Process specific product by name')
    parser.add_argument('--force', action='store_true', help='Force update even if data exists')
    
    args = parser.parse_args()
    
    print("=" * 70)
    print("BOSCH PRODUCT JSON AUTO-POPULATOR")
    print("=" * 70)
    print()
    
    if args.all:
        print("Processing all products...\n")
        results = autopop_all_products(force=args.force)
        
        success_count = sum(1 for r in results if r.get('success'))
        total_changes = sum(len(r.get('changes', [])) for r in results)
        
        print("=" * 70)
        print(f"✅ Complete: {success_count}/{len(results)} products processed")
        print(f"📝 Total changes: {total_changes}")
        
    elif args.product:
        # Search for product
        found = False
        for range_folder in TOOLS_PATH.iterdir():
            if not range_folder.is_dir():
                continue
            for category_folder in range_folder.iterdir():
                if not category_folder.is_dir():
                    continue
                product_folder = category_folder / args.product
                if product_folder.exists() and product_folder.is_dir():
                    autopop_product_json(product_folder, force=args.force)
                    found = True
                    break
            if found:
                break
        
        if not found:
            print(f"❌ Product not found: {args.product}")
    else:
        parser.print_help()

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Cancelled by user")
    except Exception as e:
        print(f"\n❌ ERROR: {e}")
        import traceback
        traceback.print_exc()
    
    input("\nPress Enter to exit...")
