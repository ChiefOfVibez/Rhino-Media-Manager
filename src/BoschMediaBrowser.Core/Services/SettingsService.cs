using System.Text.Json;
using BoschMediaBrowser.Core.Models;

namespace BoschMediaBrowser.Core.Services;

/// <summary>
/// Service for loading and saving user settings
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private readonly string _settingsFile;
    private Settings _settings = new();

    public SettingsService(string? settingsPath = null)
    {

        // Default to %AppData%/BoschMediaBrowser/ if not specified
        _settingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BoschMediaBrowser"
        );

        _settingsFile = Path.Combine(_settingsPath, "settings.json");

        // Ensure directory exists
        if (!Directory.Exists(_settingsPath))
        {
            Directory.CreateDirectory(_settingsPath);
        }
    }

    /// <summary>
    /// Get current settings
    /// </summary>
    public Settings GetSettings() => _settings;

    /// <summary>
    /// Load settings from disk
    /// </summary>
    public async Task<Settings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsFile))
        {
            _settings = CreateDefaultSettings();
            await SaveAsync(cancellationToken); // Save defaults
            return _settings;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFile, cancellationToken);
            _settings = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? CreateDefaultSettings();

            // Ensure derived paths are set
            EnsurePaths(_settings);
        }
        catch (Exception)
        {
            _settings = CreateDefaultSettings();
        }

        return _settings;
    }

    /// <summary>
    /// Save settings to disk
    /// </summary>
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_settingsFile, json, cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Update settings
    /// </summary>
    public async Task UpdateAsync(Settings newSettings, CancellationToken cancellationToken = default)
    {
        _settings = newSettings;
        EnsurePaths(_settings);
        await SaveAsync(cancellationToken);
    }

    /// <summary>
    /// Update a specific setting value
    /// </summary>
    public async Task UpdateSettingAsync(Action<Settings> updateAction, CancellationToken cancellationToken = default)
    {
        updateAction(_settings);
        await SaveAsync(cancellationToken);
    }

    /// <summary>
    /// Reset to default settings
    /// </summary>
    public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        _settings = CreateDefaultSettings();
        await SaveAsync(cancellationToken);
    }

    /// <summary>
    /// Validate settings
    /// </summary>
    public ValidationResult ValidateSettings(Settings settings)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate base server path
        if (string.IsNullOrWhiteSpace(settings.BaseServerPath))
        {
            result.IsValid = false;
            result.Errors.Add("Base server path is required");
        }
        else if (!Directory.Exists(settings.BaseServerPath))
        {
            result.IsValid = false;
            result.Errors.Add($"Base server path does not exist: {settings.BaseServerPath}");
        }

        // Validate thumbnail size
        if (settings.ThumbnailSize < 64 || settings.ThumbnailSize > 512)
        {
            result.IsValid = false;
            result.Errors.Add("Thumbnail size must be between 64 and 512 pixels");
        }

        // Validate grid spacing
        if (settings.GridSpacing <= 0)
        {
            result.IsValid = false;
            result.Errors.Add("Grid spacing must be positive");
        }

        return result;
    }

    /// <summary>
    /// Create default settings
    /// </summary>
    private Settings CreateDefaultSettings()
    {
        var settings = new Settings
        {
            BaseServerPath = @"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__",
            LinkedInsertDefault = true,
            GridSpacing = 1200.0,
            ThumbnailSize = 192
        };

        EnsurePaths(settings);
        return settings;
    }

    /// <summary>
    /// Ensure derived paths are set
    /// </summary>
    private void EnsurePaths(Settings settings)
    {
        // Set public collections path if not specified
        if (string.IsNullOrEmpty(settings.PublicCollectionsPath))
        {
            settings.PublicCollectionsPath = Path.Combine(settings.BaseServerPath, "_public-collections");
        }

        // Set thumbnail cache path if not specified
        if (string.IsNullOrEmpty(settings.ThumbnailCachePath))
        {
            settings.ThumbnailCachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BoschMediaBrowser",
                "thumbnails"
            );
        }

        // Ensure thumbnail cache directory exists
        if (!Directory.Exists(settings.ThumbnailCachePath))
        {
            try
            {
                Directory.CreateDirectory(settings.ThumbnailCachePath);
            }
            catch (Exception)
            {
            }
        }
    }
}

/// <summary>
/// Settings validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
