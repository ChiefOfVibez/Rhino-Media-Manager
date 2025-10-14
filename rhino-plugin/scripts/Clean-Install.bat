@echo off
REM Bosch Media Browser - Clean Installation
REM This completely removes the old plugin and installs fresh

echo.
echo === Bosch Media Browser - Clean Install ===
echo.
echo This will:
echo   1. Stop if Rhino is running
echo   2. Delete old plugin files
echo   3. Install fresh files
echo.

REM Check if Rhino is running
tasklist /FI "IMAGENAME eq Rhino.exe" 2>NUL | find /I /N "Rhino.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo ERROR: Rhino is currently running!
    echo Please close Rhino completely and run this script again.
    echo.
    pause
    exit /b 1
)

echo Rhino is not running. Good!
echo.

REM Delete old installation
set "PLUGIN_DIR=%APPDATA%\McNeel\Rhinoceros\8.0\Plug-ins\BoschMediaBrowser"
if exist "%PLUGIN_DIR%" (
    echo Removing old installation...
    rmdir /S /Q "%PLUGIN_DIR%"
    echo Old files deleted.
) else (
    echo No previous installation found.
)

echo.
echo Installing plugin...
echo.

REM Run the installer
PowerShell.exe -ExecutionPolicy Bypass -File "%~dp0Install-Plugin.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Installation failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo === Clean installation complete! ===
echo.
echo You can now start Rhino 8.
echo.
pause
