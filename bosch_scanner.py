"""
Bosch Product Database Scanner
Scans folder structure and generates products.json
"""
import os
import json
from pathlib import Path
from datetime import datetime

BASE_PATH = r"\\MattHQ-SVDC01\Share\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__"
MAIN_HOLDERS_PATH = r"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders\Holders"
OUTPUT_JSON = "products.json"

def scan_products():
    """Scan the folder structure and return list of products"""
    products = []
    
    tools_path = Path(BASE_PATH) / "Tools and Holders"
    
    if not tools_path.exists():
        print(f"ERROR: Path not found: {tools_path}")
        return products
    
    print(f"Scanning: {tools_path}")
    
    # Walk through Tools and Holders\<Range>\<Category>\<Product>
    for range_folder in sorted(tools_path.iterdir()):
        if not range_folder.is_dir() or range_folder.name.startswith('_'):
            continue
        
        range_name = range_folder.name  # DIY or PRO
        print(f"  Range: {range_name}")
        
        for category_folder in sorted(range_folder.iterdir()):
            if not category_folder.is_dir() or category_folder.name.startswith('_'):
                continue
            
            category_name = category_folder.name
            print(f"    Category: {category_name}")
            
            for product_folder in sorted(category_folder.iterdir()):
                if not product_folder.is_dir() or product_folder.name.startswith('_'):
                    continue
                
                # Skip non-product folders (Holders, previews, etc.)
                skip_folders = ['Holders', 'holders', 'previews', 'Previews', 'temp', 'Temp']
                if product_folder.name in skip_folders:
                    continue
                
                # Only process if folder contains a JSON file
                json_files = list(product_folder.glob('*.json'))
                if not json_files:
                    print(f"      ⚠️ Skipping {product_folder.name} (no JSON found)")
                    continue
                
                try:
                    product = scan_product(product_folder, range_name, category_name)
                    if product:
                        products.append(product)
                        print(f"      ✓ {product['productName']}")
                except Exception as e:
                    print(f"      ✗ ERROR scanning {product_folder.name}: {e}")
    
    return products

def scan_product(folder, range_name, category_name):
    """Scan a single product folder and extract metadata"""
    product_name = folder.name
    
    # Read description from <Product>.txt
    description = read_description(folder, product_name)
    
    # Read tags
    tags = read_tags(folder)
    
    # Extract SKU and Cod Articol
    sku = product_name  # Can be enhanced with regex parsing
    cod_articol = ""
    
    # Find product preview image
    product_preview = find_product_preview(folder, product_name)
    
    # Scan holders
    holders = scan_holders(folder, category_name, range_name)
    
    # Extract default holder variant and color
    default_holder = ""
    default_color = ""
    if holders:
        default_holder = holders[0]["variant"]
        if holders[0]["colors"]:
            default_color = holders[0]["colors"][0]
    
    # Scan graphics
    graphics = scan_graphics(folder, category_name, range_name)
    
    # Extract default graphic holder
    default_graphic = ""
    default_gh_width = ""
    default_gh_color = ""
    if graphics:
        default_graphic = graphics[0]["prefix"]
        default_gh_width = graphics[0]["width"]
        if graphics[0]["colors"]:
            default_gh_color = graphics[0]["colors"][0]
    
    # Check for packaging
    has_packaging = check_packaging(folder, product_name)
    
    return {
        "productName": product_name,
        "description": description,
        "sku": sku,
        "codArticol": cod_articol,
        "range": range_name,
        "category": category_name,
        "subcategory": "",
        "folderPath": str(folder),
        "productPreview": product_preview,
        "holderVariant": default_holder,
        "colorVariant": default_color,
        "holderPreview": find_holder_preview(folder, default_holder) if default_holder else "",
        "ghVariant": default_graphic,
        "ghWidth": default_gh_width,
        "ghColor": default_gh_color,
        "hasPackaging": has_packaging,
        "tags": tags,
        "notes": "",  # Will be filled from Excel
        "holders": holders,
        "graphics": graphics,
        "lastUpdated": datetime.now().isoformat()
    }

def read_description(folder, product_name):
    """Read description from <Product>.txt"""
    desc_file = folder / f"{product_name}.txt"
    if desc_file.exists():
        try:
            with open(desc_file, 'r', encoding='utf-8') as f:
                return f.readline().strip()
        except:
            pass
    return ""

def read_tags(folder):
    """Read tags from tags.txt"""
    tags_file = folder / "tags.txt"
    if tags_file.exists():
        try:
            with open(tags_file, 'r', encoding='utf-8') as f:
                content = f.read().strip()
                return [t.strip() for t in content.split(',') if t.strip()]
        except:
            pass
    return []

def find_product_preview(folder, product_name):
    """Find product preview image"""
    # Check for <Product>_mesh.(png|jpg)
    for ext in ['.png', '.jpg', '.jpeg', '.PNG', '.JPG', '.JPEG']:
        preview = folder / f"{product_name}_mesh{ext}"
        if preview.exists():
            return str(preview)
        preview = folder / f"{product_name}_Mesh{ext}"
        if preview.exists():
            return str(preview)
    
    # Fallback to <Product>.(png|jpg)
    for ext in ['.png', '.jpg', '.jpeg', '.PNG', '.JPG', '.JPEG']:
        preview = folder / f"{product_name}{ext}"
        if preview.exists():
            return str(preview)
    
    # Last resort: TEGO or TRAVERSE
    for name in ['TEGO', 'TRAVERSE']:
        for ext in ['.png', '.jpg', '.jpeg', '.PNG', '.JPG', '.JPEG']:
            preview = folder / f"{name}{ext}"
            if preview.exists():
                return str(preview)
    
    return ""

def find_holder_preview(folder, variant):
    """Find holder preview image"""
    if not variant:
        return ""
    
    for ext in ['.png', '.jpg', '.jpeg', '.PNG', '.JPG', '.JPEG']:
        # Prefer <Variant>_holder.*
        preview = folder / f"{variant}_holder{ext}"
        if preview.exists():
            return str(preview)
        
        # Fallback to <Variant>.*
        preview = folder / f"{variant}{ext}"
        if preview.exists():
            return str(preview)
    
    return ""

def read_holders_txt(folder, product_name):
    """Read holders.txt to get defined holder variants and colors"""
    holders_file = folder / "holders.txt"
    if not holders_file.exists():
        return None
    
    holders_dict = {}
    try:
        with open(holders_file, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if '=' in line:
                    variant, colors = line.split('=', 1)
                    variant = variant.strip()
                    colors = [c.strip() for c in colors.split(',')]
                    holders_dict[variant] = colors
    except Exception as e:
        print(f"      Warning: Error reading holders.txt: {e}")
        return None
    
    return holders_dict if holders_dict else None

def verify_holder_in_main_folder(variant, color):
    """Check if holder .3dm file exists in main Holders folder"""
    main_holders = Path(MAIN_HOLDERS_PATH)
    if not main_holders.exists():
        return None
    
    # Check for exact filename
    holder_file = main_holders / f"{variant}_{color}.3dm"
    if holder_file.exists():
        return str(holder_file)
    
    return None

def check_packaging(folder, product_name):
    """Check if packaging exists - improved detection"""
    # Check for _packaging.txt file
    packaging_file = folder / f"{product_name}_packaging.txt"
    if packaging_file.exists():
        return True
    
    # Check for packaging folder (case insensitive)
    for item in folder.iterdir():
        if item.is_dir() and item.name.lower() == "packaging":
            return True
    
    # Check for any file with "packaging" in name
    for item in folder.iterdir():
        if "packaging" in item.name.lower():
            return True
    
    return False

def scan_holders(product_folder, category, range_name):
    """Scan holder variants and colors - prefer holders.txt then scan .3dm files"""
    product_name = product_folder.name
    
    # Try to read holders.txt first
    holders_from_txt = read_holders_txt(product_folder, product_name)
    
    if holders_from_txt:
        # Use holders.txt as source, verify files exist in main Holders folder
        print(f"        Using holders.txt for {product_name}")
        holders = []
        for variant, colors in holders_from_txt.items():
            verified_colors = []
            verified_files = []
            for color in colors:
                file_path = verify_holder_in_main_folder(variant, color)
                if file_path:
                    verified_colors.append(color)
                    verified_files.append(file_path)
                else:
                    print(f"          Warning: {variant}_{color}.3dm not found in main Holders folder")
            
            if verified_colors:
                holders.append({
                    "variant": variant,
                    "colors": verified_colors,
                    "previewImage": find_holder_preview(product_folder, variant),
                    "files": verified_files
                })
        return holders
    
    # Fallback: scan .3dm files in product/category holders folder
    holders_folder = find_holders_folder(product_folder, category, range_name)
    if not holders_folder or not holders_folder.exists():
        return []
    
    holders_dict = {}  # variant -> colors[]
    
    # Scan .3dm files for holder patterns
    for file in holders_folder.glob("*.3dm"):
        name = file.stem  # e.g., TEGO_RAL7016
        parts = name.split('_')
        
        # Pattern: VARIANT_COLOR (e.g., TEGO_RAL7016)
        # Exclude graphic patterns (VARIANT_WIDTH_COLOR)
        if len(parts) == 2 and not parts[1].isdigit():
            variant = parts[0]  # TEGO
            color = parts[1]    # RAL7016
            
            if variant not in holders_dict:
                holders_dict[variant] = []
            if color not in holders_dict[variant]:
                holders_dict[variant].append(color)
    
    # Convert to list format
    holders = []
    for variant in sorted(holders_dict.keys()):
        colors = sorted(holders_dict[variant])
        holders.append({
            "variant": variant,
            "colors": colors,
            "previewImage": find_holder_preview(product_folder, variant),
            "files": [str(holders_folder / f"{variant}_{c}.3dm") for c in colors]
        })
    
    return holders

def scan_graphics(product_folder, category, range_name):
    """Scan graphic holder variants from .3dm files"""
    holders_folder = find_holders_folder(product_folder, category, range_name)
    if not holders_folder or not holders_folder.exists():
        return []
    
    graphics_dict = {}  # prefix -> {width -> colors[]}
    
    for file in holders_folder.glob("*.3dm"):
        name = file.stem
        parts = name.split('_')
        
        # Graphic pattern: PREFIX_WIDTH_COLOR (e.g., TEGO_66_RAL7016)
        if len(parts) >= 3:
            prefix = parts[0]
            width_str = parts[1]
            
            # Check if second part is numeric or looks like a width
            if width_str.isdigit() or width_str in ['66', '132', 'DEFAULT']:
                width = width_str
                color = parts[2] if len(parts) > 2 else ""
                
                if prefix not in graphics_dict:
                    graphics_dict[prefix] = {}
                if width not in graphics_dict[prefix]:
                    graphics_dict[prefix][width] = []
                if color and color not in graphics_dict[prefix][width]:
                    graphics_dict[prefix][width].append(color)
    
    # Convert to list format
    graphics = []
    for prefix in sorted(graphics_dict.keys()):
        for width in sorted(graphics_dict[prefix].keys()):
            colors = sorted(graphics_dict[prefix][width])
            graphics.append({
                "prefix": prefix,
                "width": width,
                "colors": colors,
                "files": [str(holders_folder / f"{prefix}_{width}_{c}.3dm") for c in colors]
            })
    
    return graphics

def find_holders_folder(product_folder, category, range_name):
    """Find the Holders folder for this product"""
    # 1. Check product folder first
    for name in ['Holders', '_holders', 'holders']:
        h = product_folder / name
        if h.exists() and h.is_dir():
            return h
    
    # 2. Check category-level Holders/<Category>/<Range>
    parent = product_folder.parent  # Category folder
    holders = parent / 'Holders' / category / range_name
    if holders.exists():
        return holders
    
    # 3. Check category-level Holders/<Range>
    holders = parent / 'Holders' / range_name
    if holders.exists():
        return holders
    
    # 4. Check category-level Holders
    holders = parent / 'Holders'
    if holders.exists():
        # Scan up to 2 parent folders if empty
        if not any(holders.glob("*.3dm")):
            for _ in range(2):
                parent_holders = holders.parent.parent / 'Holders'
                if parent_holders.exists() and any(parent_holders.glob("*.3dm")):
                    return parent_holders
                holders = parent_holders
        return holders
    
    # 5. Look in grandparent (range level)
    grandparent = product_folder.parent.parent
    holders = grandparent / 'Holders' / category
    if holders.exists():
        return holders
    
    return None

def main():
    """Main entry point"""
    print("=" * 60)
    print("Bosch Product Database Scanner")
    print("=" * 60)
    print()
    
    # Scan products
    products = scan_products()
    
    print()
    print(f"Total products found: {len(products)}")
    
    # Save to JSON
    output_path = Path(OUTPUT_JSON)
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(products, f, indent=2, ensure_ascii=False)
    
    print(f"✅ Exported to: {output_path.absolute()}")
    print()
    
    # Summary
    categories = set(p['category'] for p in products)
    ranges = set(p['range'] for p in products)
    
    print("Summary:")
    print(f"  Ranges: {', '.join(sorted(ranges))}")
    print(f"  Categories: {', '.join(sorted(categories))}")
    print()

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print(f"ERROR: {e}")
        import traceback
        traceback.print_exc()
    
    input("Press Enter to exit...")
