@echo off
REM ========================================================
REM Project Cleanup Script
REM Removes redundant files now handled by webapp
REM ========================================================

echo.
echo ====================================================
echo Bosch Product Database - Project Cleanup
echo ====================================================
echo.
echo This will remove redundant files that are now
echo handled by the webapp application.
echo.
echo Files to be DELETED:
echo   - __pycache__/ folder
echo   - GBL 18V-750.json (example file)
echo   - products.json (old cache)
echo   - audit_log.jsonl (moved to webapp)
echo   - *.bat files in root (except this cleanup script)
echo   - *.py files in root (except bosch_scanner.py)
echo.
echo Files to be KEPT:
echo   - Bosch_3D_ProductDatabase_clean.xlsm (Excel source)
echo   - webapp/ folder (main application)
echo   - BoschMediaBrowserSpec/ (plugin specs)
echo   - Documentation (*.md files)
echo   - _archived_excel_approach/ (archived work)
echo   - bosch_scanner.py (utility script)
echo.

set /p CONFIRM="Are you sure you want to continue? (Y/N): "
if /i not "%CONFIRM%"=="Y" (
    echo.
    echo Cleanup cancelled.
    pause
    exit /b 0
)

echo.
echo Starting cleanup...
echo.

cd /d "%~dp0"

REM Delete Python cache
if exist "__pycache__" (
    echo [DELETING] __pycache__/
    rmdir /s /q "__pycache__"
)

REM Delete example/old files
if exist "GBL 18V-750.json" (
    echo [DELETING] GBL 18V-750.json
    del /f /q "GBL 18V-750.json"
)

if exist "products.json" (
    echo [DELETING] products.json
    del /f /q "products.json"
)

if exist "audit_log.jsonl" (
    echo [DELETING] audit_log.jsonl
    del /f /q "audit_log.jsonl"
)

if exist "TEMPLATE_minimal.json" (
    echo [DELETING] TEMPLATE_minimal.json
    del /f /q "TEMPLATE_minimal.json"
)

REM Delete old BAT files (except this one and webapp launcher)
if exist "autopop_product_json.bat" (
    echo [DELETING] autopop_product_json.bat
    del /f /q "autopop_product_json.bat"
)

if exist "extract_previews.bat" (
    echo [DELETING] extract_previews.bat
    del /f /q "extract_previews.bat"
)

if exist "scan_database.bat" (
    echo [DELETING] scan_database.bat
    del /f /q "scan_database.bat"
)

if exist "cleanup_old_files.bat" (
    echo [DELETING] cleanup_old_files.bat
    del /f /q "cleanup_old_files.bat"
)

REM Delete old Python scripts (keep bosch_scanner.py)
if exist "autopop_product_json.py" (
    echo [DELETING] autopop_product_json.py
    del /f /q "autopop_product_json.py"
)

if exist "extract_3d_previews.py" (
    echo [DELETING] extract_3d_previews.py
    del /f /q "extract_3d_previews.py"
)

if exist "populate_excel_v2.py" (
    echo [DELETING] populate_excel_v2.py
    del /f /q "populate_excel_v2.py"
)

echo.
echo ====================================================
echo Cleanup Complete!
echo ====================================================
echo.
echo Remaining project structure:
echo.
echo Bosch Products DB in excel/
echo ├── webapp/                          [Main Application]
echo ├── BoschMediaBrowserSpec/           [Plugin Specs]
echo ├── _archived_excel_approach/        [Archive]
echo ├── Bosch_3D_ProductDatabase_clean.xlsm
echo ├── bosch_scanner.py                 [Utility]
echo ├── *.md                              [Documentation]
echo └── CLEANUP_PROJECT.bat              [This script]
echo.
echo Next Steps:
echo 1. Share webapp/ folder with your team
echo 2. They just run START_SERVER.bat
echo 3. All features available in web UI!
echo.
pause
