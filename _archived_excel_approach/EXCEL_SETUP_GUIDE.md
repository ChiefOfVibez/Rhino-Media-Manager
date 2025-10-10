# Excel Setup Guide - Enable Buttons & Dropdowns

## Problem

Excel file shows styled cells instead of working buttons because:
1. No VBA macros imported
2. File saved as `.xlsx` instead of `.xlsm`
3. Buttons are just styled cells, not clickable

## Solution: Import VBA Module

### Step 1: Open Excel in Developer Mode

1. **Open Excel file:** `Bosch_3D_ProductDatabase_clean.xlsm`

2. **Enable Developer Tab:**
   - File → Options → Customize Ribbon
   - Check ✅ **Developer** on the right side
   - Click OK

### Step 2: Import VBA Module

1. **Open Visual Basic Editor:**
   - Press **Alt + F11** (or Developer tab → Visual Basic)

2. **Import the Macros Module:**
   - In VBA Editor: File → Import File...
   - Select: `ExcelMacros.vba`
   - Click Open

3. **Update Paths in Code:**
   - In VBA Editor, find the module: `BoschDatabaseMacros`
   - At the top, update these lines:
     ```vba
     Const SCRIPTS_PATH As String = "E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel\"
     ```
   - Change to YOUR actual path

4. **Add Worksheet Event Handler:**
   - In VBA Project Explorer (left panel), find `Microsoft Excel Objects`
   - Double-click the worksheet (e.g., `Sheet1` or `Product Database`)
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

5. **Save:**
   - Press **Ctrl + S**
   - Close VBA Editor (Alt + Q)

### Step 3: Save as Macro-Enabled

1. **File → Save As**
2. **File type:** Excel Macro-Enabled Workbook (*.xlsm)
3. **Location:** Same folder
4. **Save**

### Step 4: Enable Macros

1. **Close and reopen** the Excel file
2. **Security Warning** appears: "Macros have been disabled"
3. Click **Enable Content**

---

## Testing Buttons

### Test Each Button:

1. **🔄 SCAN DATABASE** (Click cells A1-C1)
   - Runs scanner
   - Creates/updates `products.json`
   - Shows message when done

2. **📊 REFRESH EXCEL** (Click cells D1-F1)
   - Updates Excel from `products.json`
   - Closes and refreshes file

3. **🖼️ INSERT PREVIEWS** (Click cells G1-I1)
   - Embeds preview images
   - Can be slow with many products

4. **💾 EXPORT NOTES** (Click cells J1-L1)
   - Creates CSV with notes
   - Opens folder with file

---

## Alternative: Use Batch Files

If VBA buttons don't work, use batch files directly:

```bash
# Step 1: Scan database
scan_database.bat

# Step 2: Refresh Excel (close Excel first!)
populate_excel.bat
```

---

## Dropdowns (Future Feature)

Dropdowns for holder selection will be added after:
1. Scanner reads all available holders from Holders folder
2. Populates dropdown lists dynamically
3. VBA updates previews when selection changes

**For now:** Holders shown as comma-separated text.

---

## Scanner Fix Applied

**Problem:** "previews" folder picked up as product

**Fixed:** Scanner now:
- ✅ Skips folders: `Holders`, `previews`, `temp`
- ✅ Only processes folders with JSON files
- ✅ Ignores underscore-prefixed folders

**Test it:**
```bash
python bosch_scanner.py
```

Should NOT show "previews" as a product anymore.

---

## Complete Setup Checklist

- [ ] Import `ExcelMacros.vba` into Excel
- [ ] Update `SCRIPTS_PATH` in VBA code
- [ ] Add Worksheet_SelectionChange event
- [ ] Save as `.xlsm` (macro-enabled)
- [ ] Enable macros when opening
- [ ] Test all 4 buttons
- [ ] Run scanner to verify no "previews" product

---

## Troubleshooting

### Buttons Don't Work
**Fix:**
1. Check macros are enabled (File → Options → Trust Center → Trust Center Settings → Macro Settings → Enable all macros)
2. Check VBA module imported correctly
3. Check worksheet event handler added
4. Check SCRIPTS_PATH is correct

### "Macro not found"
**Fix:**
1. In VBA Editor, check module name is `BoschDatabaseMacros`
2. Check sub names match: `ScanDatabase`, `RefreshExcel`, etc.
3. Save and close VBA Editor

### Security Warning Keeps Appearing
**Fix:**
1. File → Options → Trust Center → Trust Center Settings
2. Trusted Locations → Add Location
3. Browse to your folder
4. Check "Subfolders of this location are also trusted"

### "Cannot run macro in workbook hidden by Windows"
**Fix:** Close all Excel instances, reopen file

---

## Summary

✅ **VBA macros** make buttons functional  
✅ **Worksheet events** detect button clicks  
✅ **Scanner fixed** to skip non-product folders  
✅ **`.xlsm` format** required for macros  
✅ **Enable macros** on file open  

**Next:** Test the full workflow end-to-end!
