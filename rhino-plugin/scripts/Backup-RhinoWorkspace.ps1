# Bosch Media Browser - Rhino Workspace Backup/Restore
# Preserves Rhino workspace customization during plugin development

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Backup","Restore")]
    [string]$Action
)

$rhinoSettingsPath = "$env:APPDATA\McNeel\Rhinoceros\8.0\settings"
$backupDir = "$env:APPDATA\BoschMediaBrowser\WorkspaceBackup"

# Files to backup (preserve workspace customization)
$workspaceFiles = @(
    "window_positions-Scheme__Default.xml",  # Toolbar/panel positions
    "settings-Scheme__Default.xml",          # Main settings (command prompt, filters, etc.)
    "scheme__Default.xml",                   # Workspace scheme
    "window-state-Scheme__Default.xml",      # Window state
    "command-history.txt"                    # Command history
)

function Backup-Workspace {
    Write-Host "=== Backing Up Rhino Workspace ===" -ForegroundColor Cyan
    Write-Host ""
    
    if (!(Test-Path $rhinoSettingsPath)) {
        Write-Host "Rhino settings path not found - nothing to backup" -ForegroundColor Yellow
        return
    }
    
    # Create backup directory
    if (!(Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    }
    
    $backupCount = 0
    foreach ($file in $workspaceFiles) {
        $sourcePath = Join-Path $rhinoSettingsPath $file
        if (Test-Path $sourcePath) {
            $destPath = Join-Path $backupDir $file
            try {
                Copy-Item -Path $sourcePath -Destination $destPath -Force
                Write-Host "✓ Backed up: $file" -ForegroundColor Green
                $backupCount++
            } catch {
                Write-Host "! Could not backup: $file" -ForegroundColor Yellow
            }
        }
    }
    
    if ($backupCount -eq 0) {
        Write-Host "No workspace files found to backup" -ForegroundColor Yellow
    } else {
        Write-Host ""
        Write-Host "Backed up $backupCount workspace file(s)" -ForegroundColor Green
        Write-Host "Location: $backupDir" -ForegroundColor Gray
    }
    Write-Host ""
}

function Restore-Workspace {
    Write-Host "=== Restoring Rhino Workspace ===" -ForegroundColor Cyan
    Write-Host ""
    
    if (!(Test-Path $backupDir)) {
        Write-Host "No workspace backup found - skipping restore" -ForegroundColor Yellow
        return
    }
    
    if (!(Test-Path $rhinoSettingsPath)) {
        Write-Host "Rhino settings path not found - cannot restore" -ForegroundColor Yellow
        return
    }
    
    $restoreCount = 0
    foreach ($file in $workspaceFiles) {
        $sourcePath = Join-Path $backupDir $file
        if (Test-Path $sourcePath) {
            $destPath = Join-Path $rhinoSettingsPath $file
            try {
                Copy-Item -Path $sourcePath -Destination $destPath -Force
                Write-Host "✓ Restored: $file" -ForegroundColor Green
                $restoreCount++
            } catch {
                Write-Host "! Could not restore: $file" -ForegroundColor Yellow
            }
        }
    }
    
    if ($restoreCount -eq 0) {
        Write-Host "No workspace files found to restore" -ForegroundColor Yellow
    } else {
        Write-Host ""
        Write-Host "Restored $restoreCount workspace file(s)" -ForegroundColor Green
    }
    Write-Host ""
}

# Execute requested action
if ($Action -eq "Backup") {
    Backup-Workspace
} else {
    Restore-Workspace
}
