@echo off
REM Bosch Media Browser - Plugin Installer Launcher
REM Runs the PowerShell installation script

echo.
echo === Bosch Media Browser Plugin Installer ===
echo.

PowerShell.exe -ExecutionPolicy Bypass -File "%~dp0Install-Plugin.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Installation failed or was cancelled.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Press any key to close...
pause >nul
