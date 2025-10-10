#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Reset Rhino's panel state to prevent auto-opening the Bosch Media Browser panel
.DESCRIPTION
    This script clears Rhino's saved panel visibility state for the Bosch Media Browser,
    preventing it from automatically opening on startup.
#>

Write-Host "=== Resetting Bosch Media Browser Panel State ===" -ForegroundColor Cyan

# Check if Rhino is running
$rhinoProcess = Get-Process -Name "Rhino" -ErrorAction SilentlyContinue
if ($rhinoProcess) {
    Write-Host ""
    Write-Host "ERROR: Rhino is currently running!" -ForegroundColor Red
    Write-Host "Please close Rhino before running this script." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

# Rhino 8 settings location
$rhinoSettingsPath = "$env:APPDATA\McNeel\Rhinoceros\8.0\settings"

Write-Host ""
Write-Host "Searching for panel state files in:" -ForegroundColor Gray
Write-Host "  $rhinoSettingsPath" -ForegroundColor Gray
Write-Host ""

# Panel GUID
$panelGuid = "A3B5C7D9-1E2F-4A5B-8C9D-0E1F2A3B4C5D"

# Find and backup/delete panel state files
$found = $false

if (Test-Path $rhinoSettingsPath) {
    # Search for any files containing the panel GUID
    $stateFiles = Get-ChildItem -Path $rhinoSettingsPath -Recurse -File | 
        Where-Object { 
            (Select-String -Path $_.FullName -Pattern $panelGuid -Quiet -ErrorAction SilentlyContinue) 
        }
    
    if ($stateFiles) {
        $found = $true
        Write-Host "Found panel state references in the following files:" -ForegroundColor Yellow
        
        foreach ($file in $stateFiles) {
            Write-Host "  $($file.FullName)" -ForegroundColor Gray
            
            # Create backup
            $backupPath = "$($file.FullName).backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
            Copy-Item -Path $file.FullName -Destination $backupPath
            Write-Host "    ✓ Backup created: $backupPath" -ForegroundColor Green
            
            # Try to remove panel reference from file
            try {
                $content = Get-Content -Path $file.FullName -Raw
                $newContent = $content -replace "(?s)<[^>]*$panelGuid[^>]*>.*?</[^>]*>", ""
                Set-Content -Path $file.FullName -Value $newContent
                Write-Host "    ✓ Removed panel reference from file" -ForegroundColor Green
            }
            catch {
                Write-Host "    ⚠ Could not modify file: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    }
}

# Also check window state files
$windowStatePath = "$env:APPDATA\McNeel\Rhinoceros\8.0\settings\window-state"
if (Test-Path $windowStatePath) {
    $windowFiles = Get-ChildItem -Path $windowStatePath -Recurse -File | 
        Where-Object { 
            (Select-String -Path $_.FullName -Pattern $panelGuid -Quiet -ErrorAction SilentlyContinue) 
        }
    
    if ($windowFiles) {
        $found = $true
        Write-Host ""
        Write-Host "Found window state references:" -ForegroundColor Yellow
        
        foreach ($file in $windowFiles) {
            Write-Host "  $($file.FullName)" -ForegroundColor Gray
            
            # Backup and delete entire window state file
            $backupPath = "$($file.FullName).backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
            Copy-Item -Path $file.FullName -Destination $backupPath
            Remove-Item -Path $file.FullName -Force
            Write-Host "    ✓ File backed up and removed" -ForegroundColor Green
        }
    }
}

Write-Host ""
if ($found) {
    Write-Host "✓ Panel state has been reset!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Start Rhino 8" -ForegroundColor White
    Write-Host "  2. The Bosch Media Browser panel should NOT open automatically" -ForegroundColor White
    Write-Host "  3. Run 'ShowMediaBrowser' command to open it manually" -ForegroundColor White
} else {
    Write-Host "No panel state found - panel should not auto-open." -ForegroundColor Green
    Write-Host ""
    Write-Host "If the panel still opens automatically:" -ForegroundColor Yellow
    Write-Host "  1. Close Rhino" -ForegroundColor White
    Write-Host "  2. Delete: $env:APPDATA\McNeel\Rhinoceros\8.0\settings" -ForegroundColor White
    Write-Host "  3. Restart Rhino (settings will be recreated)" -ForegroundColor White
}

Write-Host ""
