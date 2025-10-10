# Setup Instructions - Excel Database

## File Locations

### ✅ Excel File (Network Location)
```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Bosch_Product_Database.xlsm
```
**This is where the Excel file is created and updated.**

### ✅ Scripts (Local Project Folder)
```
E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel\
├── bosch_scanner.py        ← Scans JSONs
├── scan_database.bat
├── populate_excel.py       ← Creates/updates Excel
├── populate_excel.bat
├── autopop_product_json.py ← Auto-fills JSONs
├── autopop_product_json.bat
└── ExcelMacros.vba         ← Import this into Excel
```

### ✅ Product JSONs (Network Location)
```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders\
└── PRO\
    └── Garden\
        └── GBL 18V-750\
            └── GBL 18V-750.json
```

---

## Initial Setup (One-Time)

### Step 1: Generate Excel File

If Excel file doesn't exist yet:

```bash
# From project folder:
cd E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel

# Scan products and create Excel
scan_database.bat
populate_excel.bat
```

**Result:** Creates `M:\Proiectare\...\Bosch_Product_Database.xlsm`

### Step 2: Import VBA Macros

1. **Open Excel file:**
   ```
   M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Bosch_Product_Database.xlsm
   ```

2. **Enable Developer Tab:**
   - File → Options → Customize Ribbon
   - Check ✅ Developer
   - Click OK

3. **Open VBA Editor:**
   - Press **Alt + F11**

4. **Import Module:**
   - File → Import File...
   - Browse to: `E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel\ExcelMacros.vba`
   - Click Open

5. **Verify Paths in Module:**
   - In VBA Editor, double-click `BoschDatabaseMacros` module
   - Check these lines (should already be correct):
     ```vba
     Const SCRIPTS_PATH As String = "E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel\"
     Const EXCEL_PATH As String = "M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Bosch_Product_Database.xlsm"
     ```

6. **Add Worksheet Event Handler:**
   - In VBA Project Explorer (left panel), expand `Microsoft Excel Objects`
   - Double-click the worksheet (Sheet1 or Product Database)
   - Paste this code:
     ```vba
     Private Sub Worksheet_SelectionChange(ByVal Target As Range)
         If Target.Row = 1 And Target.Column >= 1 And Target.Column <= 12 Then
             Application.EnableEvents = False
             Select Case Target.Column
                 Case 1 To 3
                     BoschDatabaseMacros.ScanDatabase
                 Case 4 To 6
                     BoschDatabaseMacros.RefreshExcel
                 Case 7 To 9
                     BoschDatabaseMacros.InsertPreviews
                 Case 10 To 12
                     BoschDatabaseMacros.ExportNotes
             End Select
             Application.EnableEvents = True
         End If
     End Sub
     ```

7. **Save and Close:**
   - Press **Ctrl + S**
   - Close VBA Editor (Alt + Q)
   - Save Excel file

8. **Enable Macros:**
   - Close Excel
   - Reopen: `M:\Proiectare\...\Bosch_Product_Database.xlsm`
   - Click **Enable Content** when security warning appears

---

## Daily Workflow

### Add New Products

1. **Create product JSONs:**
   ```
   Copy TEMPLATE_minimal.json → M:\...\Tools and Holders\PRO\Garden\ProductName\ProductName.json
   Edit: name, description, SKU, holders
   ```

2. **Auto-fill JSONs:**
   ```bash
   cd E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel
   autopop_product_json.bat
   ```

3. **Update Excel:**
   ```bash
   scan_database.bat
   populate_excel.bat
   ```

4. **Open Excel:**
   ```
   M:\Proiectare\...\Bosch_Product_Database.xlsm
   ```

### Use Excel Buttons (After VBA Import)

1. **Open Excel:** `M:\Proiectare\...\Bosch_Product_Database.xlsm`

2. **Click buttons in Row 1:**
   - **🔄 SCAN DATABASE** (A1-C1) → Scans JSONs
   - **📊 REFRESH EXCEL** (D1-F1) → Updates Excel
   - **🖼️ INSERT PREVIEWS** (G1-I1) → Embeds images
   - **💾 EXPORT NOTES** (J1-L1) → Creates CSV

---

## File Organization

### Network Location (M: Drive)
```
M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\
├── Bosch_Product_Database.xlsm     ← Main Excel file (open this)
│
├── Tools and Holders\
│   ├── PRO\
│   │   └── Garden\
│   │       └── GBL 18V-750\
│   │           ├── GBL 18V-750.json
│   │           ├── GBL 18V-750_mesh.3dm
│   │           └── GBL 18V-750_mesh.jpg
│   │
│   └── Holders\
│       ├── Tego_RAL7043_BO.161.9LL8600.3dm
│       └── Garden\
│           └── previews\
│               └── Tego_RAL7043_BO.161.9LL8600.jpg
│
└── _public-collections\            ← Hidden from scanner
```

### Local Project Folder (E: Drive)
```
E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel\
├── bosch_scanner.py                ← Scan script
├── populate_excel.py               ← Excel generator
├── autopop_product_json.py         ← JSON auto-filler
├── ExcelMacros.vba                 ← Import into Excel
├── products.json                   ← Generated database
├── COMPLETE_WORKFLOW.md            ← Main guide
└── *.bat                           ← Batch runners
```

---

## Why Two Locations?

### Network (M:\ Drive)
- **Excel file** - Accessible by everyone, central location
- **Product files** - Source data (JSONs, 3D files)
- **Shared access** - Team can collaborate

### Local (E:\ Drive)
- **Scripts** - Python/batch files for automation
- **Development** - Where you run commands
- **Fast execution** - Local scripts are faster

---

## Workflow Diagram

```
LOCAL PROJECT FOLDER                    NETWORK LOCATION
(E:\CURSOR\...)                         (M:\Proiectare\...)

                                        Product JSONs
                                        └── GBL 18V-750.json
                                                │
                                                │
    ┌──────────────────────────┐               │
    │ autopop_product_json.py  │◄──────────────┘
    │ (fills in paths)         │
    └─────────────┬────────────┘
                  │
                  │ Updated JSONs
                  │
    ┌─────────────▼────────────┐
    │ bosch_scanner.py         │
    │ (reads all JSONs)        │
    └─────────────┬────────────┘
                  │
                  │ products.json
                  │
    ┌─────────────▼────────────┐
    │ populate_excel.py        │─────────────┐
    │ (creates/updates Excel)  │             │
    └──────────────────────────┘             │
                                             │
                                             ▼
                                    Bosch_Product_Database.xlsm
                                    (M:\Proiectare\...)
```

---

## Troubleshooting

### "Excel file not found"
**Check:** `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Bosch_Product_Database.xlsm` exists  
**Fix:** Run `populate_excel.bat` to create it

### "Script not found" in VBA
**Check:** Paths in ExcelMacros.vba match your setup  
**Fix:** Update `SCRIPTS_PATH` constant in VBA module

### Buttons don't work
**Check:** VBA macros imported and enabled  
**Fix:** Follow Step 2 above to import VBA module

### Excel on wrong drive
**Check:** populate_excel.py line 12:
```python
OUTPUT_EXCEL = r"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Bosch_Product_Database.xlsm"
```
**Should be:** Network path (M:\), not local path (E:\)

---

## Quick Reference

### Commands (Run from project folder)
```bash
autopop_product_json.bat    # Auto-fill JSON files
scan_database.bat           # Scan JSONs → products.json
populate_excel.bat          # Update Excel from products.json
```

### File Locations
```
Excel:    M:\Proiectare\...\Bosch_Product_Database.xlsm
Scripts:  E:\CURSOR\...\Bosch Products DB in excel\
JSONs:    M:\Proiectare\...\Tools and Holders\PRO\Garden\...
```

### Excel Buttons (After VBA Import)
```
Row 1, Columns A-C:   SCAN DATABASE
Row 1, Columns D-F:   REFRESH EXCEL
Row 1, Columns G-I:   INSERT PREVIEWS
Row 1, Columns J-L:   EXPORT NOTES
```

---

## Summary

✅ **Excel file:** Network location (M: drive) - shared access  
✅ **Scripts:** Local project folder (E: drive) - fast execution  
✅ **VBA macros:** Import once into network Excel file  
✅ **Buttons:** Work after VBA import + macro enable  
✅ **Workflow:** Edit JSONs → Run scripts → Open Excel  

**Next:** Follow COMPLETE_WORKFLOW.md for detailed usage! 📖
