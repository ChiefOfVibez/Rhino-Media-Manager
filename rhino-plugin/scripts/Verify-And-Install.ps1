# Bosch Media Browser - Verification and Installation Script
# This script rebuilds the plugin, checks file dates, and installs with verification

Write-Host "=== Bosch Media Browser - Rebuild & Install ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Rebuild the plugin
Write-Host "=== Step 1: Rebuilding Plugin ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "Running dotnet clean..." -ForegroundColor Gray
dotnet clean
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: dotnet clean failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Clean complete." -ForegroundColor Green
Write-Host ""

Write-Host "Running dotnet build..." -ForegroundColor Gray
dotnet build src\BoschMediaBrowser.Rhino\BoschMediaBrowser.Rhino.csproj --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: dotnet build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build complete." -ForegroundColor Green
Write-Host ""

# Step 2: Check source files
$pluginDll = "src\BoschMediaBrowser.Rhino\bin\Release\net7.0\BoschMediaBrowser.Rhino.dll"
$coreDll = "src\BoschMediaBrowser.Core\bin\Release\net7.0\BoschMediaBrowser.Core.dll"

if (Test-Path $pluginDll) {
    $fileInfo = Get-Item $pluginDll
    Write-Host "Plugin DLL found:" -ForegroundColor Green
    Write-Host "  Path: $pluginDll"
    Write-Host "  Size: $($fileInfo.Length) bytes"
    Write-Host "  Last Modified: $($fileInfo.LastWriteTime)"
} else {
    Write-Host "ERROR: Plugin DLL not found!" -ForegroundColor Red
    exit 1
}

Write-Host ""

if (Test-Path $coreDll) {
    $fileInfo = Get-Item $coreDll
    Write-Host "Core DLL found:" -ForegroundColor Green
    Write-Host "  Path: $coreDll"
    Write-Host "  Size: $($fileInfo.Length) bytes"
    Write-Host "  Last Modified: $($fileInfo.LastWriteTime)"
} else {
    Write-Host "ERROR: Core DLL not found!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Files are ready for installation." -ForegroundColor Green
Write-Host ""

# Step 3: Check if Rhino is running
Write-Host "=== Step 3: Checking Rhino Status ===" -ForegroundColor Cyan
$rhinoProcess = Get-Process -Name "Rhino" -ErrorAction SilentlyContinue
if ($rhinoProcess) {
    Write-Host "WARNING: Rhino is currently running!" -ForegroundColor Yellow
    Write-Host "Process ID: $($rhinoProcess.Id)"
    Write-Host ""
    Write-Host "Please close Rhino before continuing." -ForegroundColor Yellow
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "Rhino is not running. Good!" -ForegroundColor Green
Write-Host ""

# Step 4: Reset panel state to prevent auto-opening
Write-Host "=== Step 4: Resetting Panel State ===" -ForegroundColor Cyan
$panelGuid = "A3B5C7D9-1E2F-4A5B-8C9D-0E1F2A3B4C5D"
$rhinoSettingsPath = "$env:APPDATA\McNeel\Rhinoceros\8.0\settings"

if (Test-Path $rhinoSettingsPath) {
    Write-Host "Searching for panel state files..." -ForegroundColor Gray
    
    # Search for files containing the panel GUID
    $stateFiles = @()
    Get-ChildItem -Path $rhinoSettingsPath -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object {
        try {
            if (Select-String -Path $_.FullName -Pattern $panelGuid -Quiet -ErrorAction SilentlyContinue) {
                $stateFiles += $_
            }
        } catch {
            # Ignore errors
        }
    }
    
    if ($stateFiles.Count -gt 0) {
        Write-Host "Found $($stateFiles.Count) file(s) with panel state references" -ForegroundColor Yellow
        
        foreach ($file in $stateFiles) {
            try {
                # Create backup
                $backupPath = "$($file.FullName).backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
                Copy-Item -Path $file.FullName -Destination $backupPath -ErrorAction SilentlyContinue
                
                # Try to remove panel reference
                $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
                if ($content) {
                    $escapedGuid = [regex]::Escape($panelGuid)
                    $pattern = "(?s)<[^>]*$escapedGuid[^>]*>.*?</[^>]*>"
                    $newContent = $content -replace $pattern, ""
                    Set-Content -Path $file.FullName -Value $newContent -ErrorAction SilentlyContinue
                    Write-Host "  OK Cleaned: $($file.Name)" -ForegroundColor Green
                }
            }
            catch {
                Write-Host "  ! Could not clean: $($file.Name)" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "No panel state found - good!" -ForegroundColor Green
    }
} else {
    Write-Host "Settings path not found (expected on first run)" -ForegroundColor Gray
}

Write-Host ""

# Step 5: Install
Write-Host "=== Step 5: Installing Plugin ===" -ForegroundColor Cyan
$rhinoPluginsDir = "$env:APPDATA\McNeel\Rhinoceros\8.0\Plug-ins"
$pluginGuid = "B05C43D0-ED1A-4B05-8E12-345678ABCDEF"
$installDir = Join-Path $rhinoPluginsDir "BoschMediaBrowser\$pluginGuid"

# Remove old installation
if (Test-Path $installDir) {
    Write-Host "Removing old installation..." -ForegroundColor Yellow
    Remove-Item -Path $installDir -Recurse -Force -ErrorAction Stop
    Write-Host "Old files deleted." -ForegroundColor Green
}

# Create directory
New-Item -ItemType Directory -Path $installDir -Force | Out-Null

# Copy files
Write-Host "Copying files..." -ForegroundColor Cyan
Copy-Item $pluginDll -Destination (Join-Path $installDir "BoschMediaBrowser.rhp")
Copy-Item $coreDll -Destination (Join-Path $installDir "BoschMediaBrowser.Core.dll")

# Verify installation
Write-Host ""
Write-Host "Verifying installation..." -ForegroundColor Cyan
$installedRhp = Join-Path $installDir "BoschMediaBrowser.rhp"
$installedCore = Join-Path $installDir "BoschMediaBrowser.Core.dll"

if ((Test-Path $installedRhp) -and (Test-Path $installedCore)) {
    $rhpInfo = Get-Item $installedRhp
    $coreInfo = Get-Item $installedCore
    
    Write-Host "Installation verified!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Installed files:" -ForegroundColor White
    Write-Host "  RHP: $($rhpInfo.Length) bytes - Modified: $($rhpInfo.LastWriteTime)"
    Write-Host "  Core: $($coreInfo.Length) bytes - Modified: $($coreInfo.LastWriteTime)"
    Write-Host ""
    Write-Host "Installation location: $installDir" -ForegroundColor White
} else {
    Write-Host "ERROR: Installation verification failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Installation Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "OK Panel state reset (prevents auto-opening on startup)" -ForegroundColor Green
Write-Host "OK Plugin files installed" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Start Rhino 8 (panel should NOT open automatically)"
Write-Host "  2. Watch the command window for plugin load messages"
Write-Host "  3. Run 'ShowMediaBrowser' command to open panel manually"
Write-Host "  4. Copy ALL output from command window if any errors occur"
Write-Host ""
Write-Host "Expected output when Rhino starts:" -ForegroundColor Yellow
Write-Host "  === Bosch Media Browser Plugin Loading ==="
Write-Host "  Plugin GUID: ..."
Write-Host "  Panel Type: ..."
Write-Host "  Panel is not visible - good!"
Write-Host "  Bosch Media Browser plugin loaded successfully."
Write-Host ""
Write-Host "When you run 'ShowMediaBrowser':" -ForegroundColor Yellow
Write-Host "  === MediaBrowserPanel: Constructor (EMPTY) ==="
Write-Host "  MediaBrowserPanel: OnShown called"
Write-Host "  MediaBrowserPanel: Starting deferred init..."
Write-Host ""
