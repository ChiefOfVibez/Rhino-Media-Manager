# Bosch Media Browser - Rhino Plugin Installer
# Installs the plugin to Rhino 8 on Windows

Write-Host "=== Bosch Media Browser Plugin Installer ===" -ForegroundColor Cyan
Write-Host ""

# Paths
$pluginDll = "src\BoschMediaBrowser.Rhino\bin\Release\net7.0\BoschMediaBrowser.Rhino.dll"
$coreDll = "src\BoschMediaBrowser.Core\bin\Release\net7.0\BoschMediaBrowser.Core.dll"
$rhinoPluginsDir = "$env:APPDATA\McNeel\Rhinoceros\8.0\Plug-ins"

# Check if files exist
if (-not (Test-Path $pluginDll)) {
    Write-Host "ERROR: Plugin DLL not found. Please build the solution first:" -ForegroundColor Red
    Write-Host "  dotnet build BoschMediaBrowser.sln --configuration Release" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $coreDll)) {
    Write-Host "ERROR: Core DLL not found. Please build the solution first." -ForegroundColor Red
    exit 1
}

# Create plugin directory if it doesn't exist
if (-not (Test-Path $rhinoPluginsDir)) {
    New-Item -ItemType Directory -Path $rhinoPluginsDir -Force | Out-Null
    Write-Host "Created Rhino plugins directory: $rhinoPluginsDir" -ForegroundColor Green
}

# Plugin installation folder
$pluginGuid = "B05C43D0-ED1A-4B05-8E12-345678ABCDEF"
$installDir = Join-Path $rhinoPluginsDir "BoschMediaBrowser\$pluginGuid"

# Create installation directory
if (Test-Path $installDir) {
    Write-Host "Removing existing installation..." -ForegroundColor Yellow
    Remove-Item -Path $installDir -Recurse -Force
}

New-Item -ItemType Directory -Path $installDir -Force | Out-Null

# Copy files
Write-Host "Installing plugin files..." -ForegroundColor Cyan
Copy-Item $pluginDll -Destination (Join-Path $installDir "BoschMediaBrowser.rhp")
Copy-Item $coreDll -Destination (Join-Path $installDir "BoschMediaBrowser.Core.dll")

Write-Host ""
Write-Host "=== Installation Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Plugin installed to: $installDir" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Close Rhino if it's running"
Write-Host "  2. Start Rhino 8"
Write-Host "  3. The plugin should load automatically"
Write-Host "  4. Run 'ShowMediaBrowser' command to open the panel"
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Cyan
Write-Host "  - Set base path to your product database folder"
Write-Host "  - Example: M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__"
Write-Host ""
