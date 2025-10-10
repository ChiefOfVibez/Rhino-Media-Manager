@echo off
REM Populate Excel workbook from products.json

echo ============================================================
echo Excel Workbook Generator
echo ============================================================
echo.

REM Check Python
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    pause
    exit /b 1
)

REM Check if openpyxl is installed
python -c "import openpyxl" >nul 2>&1
if errorlevel 1 (
    echo Installing openpyxl library...
    pip install openpyxl
)

REM Run populate script
python populate_excel_v2.py

pause
