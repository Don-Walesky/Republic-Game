# Republic - Backup & Disaster Recovery Procedures

This guide provides operational runbooks for backing up, verifying, and recovering sovereign simulation states for Republic.

---

## 1. State Persistence Architecture

Republic maintains two tiers of durable state:
1. **Authoritative Engine Save File**:
   - `republic_game.sav`: Binary/structured serialization of the complete sovereign country model (Treasury balance, Cabinet ministers, Election results, National Yield boundary timestamps, and active capital projects).
   - `shared_world.json`: Authoritative multi-country state for multiplayer sessions (known nations, bilateral treaties, diplomatic message logs, trade proposals, and player ownership mapping).
2. **Client State Cache**:
   - `presidential_desk_state.json`: Exported bridge snapshot read by the Godot Presidential Desk.
   - Godot local user directory (`%APPDATA%/Godot/app_userdata/Republic` on Windows, `~/.local/share/godot/app_userdata/Republic` on Linux/macOS, or IndexedDB in WebAssembly).

---

## 2. Automated Backup Execution

Use the repository tool [`Tools/backup_restore.ps1`](../../Tools/backup_restore.ps1):

### Create a Snapshot
```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\backup_restore.ps1 -Action Backup
```
- Bundles all authoritative `.sav`, `.json`, and engine state into a timestamped archive under `./backups/republic_backup_YYYYMMDD_HHMMSS.zip`.
- Archives Godot application user data if present.

### List Existing Backups
```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\backup_restore.ps1 -Action List
```

---

## 3. Disaster Recovery & Rollback Procedure

In the event of corrupted simulation state, inadvertent deletion, or failed deployment:

### 3.1 Restoring from a Snapshot Archive
```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\backup_restore.ps1 -Action Restore -BackupPath ".\backups\republic_backup_20261017_124501.zip"
```
1. Automatically preserves existing active files with `.bak` extensions before writing.
2. Restores `republic_game.sav`, `presidential_desk_state.json`, and `shared_world.json`.
3. Restores Godot client user files.

### 3.2 Handling Corrupt Saves During Live Simulation
If `RepublicHost` or `PresidentialDeskBridge` detects malformed JSON or corrupted binary headers:
- An emergency backup `[save_file].bak` is immediately created on disk.
- Corruption is flagged explicitly via `HasCorruptSaveDetected` and `LastLoadError`.
- The engine refuses to overwrite the corrupted file until restored or reset by the operator, preventing silent data loss.

---

## 4. Web Deployment Rollback

If a newly deployed WebAssembly release candidate encounters runtime exceptions:
1. Revert web server root `/var/www/republic` or Cloudflare Pages deployment to the previous stable release commit.
2. Purge browser cache headers: Ensure `Cache-Control: no-cache, must-revalidate` is configured for `index.html` while `.wasm` and `.pck` assets utilize content-hashed URLs or version query strings.
3. Validate client local storage: If IndexedDB schema migrations occurred, advise players to use "Resume from Save" or clear site cookies.
