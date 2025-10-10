# User Settings & Configuration

## Settings Location

User settings are stored in JSON format at:
```
%AppData%\BoschMediaBrowser\settings.json
```

On Windows, this typically resolves to:
```
C:\Users\<username>\AppData\Roaming\BoschMediaBrowser\settings.json
```

## Settings Schema

```json
{
  "baseServerPath": "M:\\Proiectare\\__SCAN 3D Produse\\__BOSCH\\__NEW DB__",
  "publicCollectionsPath": "M:\\Proiectare\\__SCAN 3D Produse\\__BOSCH\\__NEW DB__\\_public-collections",
  "linkedInsertDefault": true,
  "gridSpacing": 1200,
  "thumbnailSize": 192,
  "thumbnailCachePath": "%AppData%\\BoschMediaBrowser\\thumbnails",
  "lastUsedFilters": {
    "searchText": "",
    "ranges": [],
    "categories": [],
    "sortBy": "Name",
    "sortDir": "asc"
  }
}
```

## Settings Fields

### Paths

**baseServerPath** (string, required)
- Network path to product database root
- Example: `M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__`
- Must have read access

**publicCollectionsPath** (string, optional)
- Path to shared public collections folder
- Default: `<baseServerPath>\_public-collections`
- Must have read access (write for admins)

**thumbnailCachePath** (string, optional)
- Local cache for preview images
- Default: `%AppData%\BoschMediaBrowser\thumbnails`
- Auto-created if missing

### Insertion Defaults

**linkedInsertDefault** (bool, default: `true`)
- `true`: Insert as linked blocks (referenced .3dm files)
- `false`: Insert as embedded geometry (baked)
- Linked blocks are faster and maintain file references

**gridSpacing** (number, default: `1200`)
- Default spacing in millimeters for grid insertion
- Typical values: 600, 1200, 1500
- Can be overridden per insertion

### UI Preferences

**thumbnailSize** (number, default: `192`)
- Thumbnail size in pixels
- Options: 128, 192, 256
- Larger = better quality, slower loading

### Last Used State

**lastUsedFilters** (object)
- Persists last search/filter state
- Restored when plugin reopens
- Can be cleared via "Reset Filters" action

## Configuration via UI

Settings can be modified via:
1. Rhino command: `BoschMediaBrowserSettings`
2. Panel → Gear icon → Settings
3. Settings dialog with live validation

## Advanced Settings (Not in UI)

These can be manually edited in `settings.json`:

**cacheExpirationDays** (number, default: 30)
- Days to keep thumbnails in cache
- Set to 0 to disable cache expiration

**maxConcurrentLoads** (number, default: 5)
- Max parallel thumbnail loads
- Lower = less memory, slower
- Higher = faster, more memory

**autoRefreshOnChange** (bool, default: true)
- Watch for JSON file changes
- Auto-refresh product list
- Disable if causing performance issues

## Resetting Settings

To reset to defaults:
1. Close Rhino
2. Delete: `%AppData%\BoschMediaBrowser\settings.json`
3. Restart Rhino
4. Defaults will be recreated

## Network Path Configuration

**For network drives:**
- Use UNC paths: `\\server\share\path` (preferred)
- Or mapped drives: `M:\path` (requires consistent mapping)

**For local testing:**
- Can use local paths: `C:\TestData\Products`
- Ensure folder structure matches production

## Troubleshooting

**"Cannot find base path" error:**
- Verify network connection
- Check path in Settings
- Ensure read permissions
- Try UNC path instead of mapped drive

**Thumbnails not loading:**
- Check `thumbnailCachePath` exists
- Verify disk space
- Clear cache and reload

**Settings not persisting:**
- Check write permissions on `%AppData%\BoschMediaBrowser`
- Verify JSON is valid (use JSON validator)
- Check Rhino console for errors

---

**Related:**
- [Build Instructions](./README.md)
- [Architecture](../BoschMediaBrowserSpec/specs/001-rhino-media-browser/ARCHITECTURE.md)
