# Republic - Backup & Disaster Recovery Utility
# Usage:
#   .\Tools\backup_restore.ps1 -Action Backup
#   .\Tools\backup_restore.ps1 -Action List
#   .\Tools\backup_restore.ps1 -Action Restore -BackupPath ".\backups\republic_backup_20261017_120000.zip"

param(
    [ValidateSet("Backup", "Restore", "List")]
    [string]$Action = "Backup",
    [string]$BackupPath = "",
    [string]$BackupDir = "./backups"
)

$ErrorActionPreference = "Stop"

function Ensure-Dir($path) {
    if (-not (Test-Path -Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

Ensure-Dir $BackupDir

switch ($Action) {
    "Backup" {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $targetArchive = Join-Path $BackupDir "republic_backup_$timestamp.zip"
        $stagingDir = Join-Path $BackupDir "staging_$timestamp"
        Ensure-Dir $stagingDir

        Write-Host "Creating Republic sovereign state backup..." -ForegroundColor Cyan

        $filesToBackup = @(
            "republic_game.sav",
            "presidential_desk_state.json",
            "presidential_desk_request.json",
            "presidential_desk_result.json",
            "shared_world.json"
        )

        $count = 0
        foreach ($file in $filesToBackup) {
            if (Test-Path $file) {
                Copy-Item -Path $file -Destination $stagingDir -Force
                $count++
                Write-Host "  [+] Staged: $file" -ForegroundColor Gray
            }
        }

        # Also backup Godot user data if present
        $godotUserData = Join-Path $env:APPDATA "Godot\app_userdata\Republic"
        if (Test-Path $godotUserData) {
            $godotStaging = Join-Path $stagingDir "godot_userdata"
            Ensure-Dir $godotStaging
            Copy-Item -Path "$godotUserData\*" -Destination $godotStaging -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  [+] Staged Godot app_userdata" -ForegroundColor Gray
        }

        if ($count -eq 0 -and (-not (Test-Path "$stagingDir\godot_userdata"))) {
            Write-Host "Warning: No live simulation save files found in root directory." -ForegroundColor Yellow
        }

        Compress-Archive -Path "$stagingDir\*" -DestinationPath $targetArchive -Force
        Remove-Item -Path $stagingDir -Recurse -Force

        Write-Host "Backup completed successfully!" -ForegroundColor Green
        Write-Host "Archive: $targetArchive" -ForegroundColor Green
    }

    "List" {
        Write-Host "Available Republic state backups in '$BackupDir':" -ForegroundColor Cyan
        $archives = Get-ChildItem -Path $BackupDir -Filter "*.zip" | Sort-Object LastWriteTime -Descending
        if ($archives.Count -eq 0) {
            Write-Host "  No backups found." -ForegroundColor Yellow
        } else {
            foreach ($a in $archives) {
                Write-Host "  - $($a.Name) ($([math]::Round($a.Length / 1KB, 1)) KB) - $($a.LastWriteTime)" -ForegroundColor White
            }
        }
    }

    "Restore" {
        if (-not $BackupPath -or (-not (Test-Path $BackupPath))) {
            Write-Host "Error: Valid -BackupPath must be specified for restore action." -ForegroundColor Red
            exit 1
        }

        Write-Host "Restoring Republic simulation state from: $BackupPath" -ForegroundColor Cyan
        $restoreTemp = Join-Path $BackupDir "restore_temp_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        Ensure-Dir $restoreTemp

        Expand-Archive -Path $BackupPath -DestinationPath $restoreTemp -Force

        # Restore root save files
        $filesToRestore = @(
            "republic_game.sav",
            "presidential_desk_state.json",
            "shared_world.json"
        )

        foreach ($file in $filesToRestore) {
            $sourceFile = Join-Path $restoreTemp $file
            if (Test-Path $sourceFile) {
                # Preserve backup of existing if present
                if (Test-Path $file) {
                    Copy-Item -Path $file -Destination "$file.bak" -Force
                }
                Copy-Item -Path $sourceFile -Destination "." -Force
                Write-Host "  [Restored]: $file" -ForegroundColor Green
            }
        }

        # Restore Godot user data if present
        $godotSource = Join-Path $restoreTemp "godot_userdata"
        if (Test-Path $godotSource) {
            $godotTarget = Join-Path $env:APPDATA "Godot\app_userdata\Republic"
            Ensure-Dir $godotTarget
            Copy-Item -Path "$godotSource\*" -Destination $godotTarget -Recurse -Force
            Write-Host "  [Restored]: Godot app_userdata" -ForegroundColor Green
        }

        Remove-Item -Path $restoreTemp -Recurse -Force
        Write-Host "State restoration completed successfully." -ForegroundColor Green
    }
}
