using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using Xunit;

namespace BoschMediaBrowser.Tests;

public class SettingsServiceTests
{
    private readonly string _testSettingsPath;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _testSettingsPath = Path.Combine(Path.GetTempPath(), "BoschMediaBrowserTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testSettingsPath);
        _service = new SettingsService(_testSettingsPath);
    }

    [Fact]
    public async Task LoadAsync_WithNoFile_CreatesDefaults()
    {
        // Act
        var settings = await _service.LoadAsync();

        // Assert
        Assert.NotNull(settings);
        Assert.NotEmpty(settings.BaseServerPath);
        Assert.Equal(1200.0, settings.GridSpacing);
        Assert.Equal(192, settings.ThumbnailSize);
        Assert.True(settings.LinkedInsertDefault);
    }

    [Fact]
    public async Task SaveAsync_PersistsSettings()
    {
        // Arrange
        var settings = await _service.LoadAsync();
        settings.GridSpacing = 1500.0;

        // Act
        await _service.UpdateAsync(settings);

        // Create new service to load saved settings
        var newService = new SettingsService(_testSettingsPath);
        var loaded = await newService.LoadAsync();

        // Assert
        Assert.Equal(1500.0, loaded.GridSpacing);
    }

    [Fact]
    public async Task UpdateSettingAsync_UpdatesSpecificSetting()
    {
        // Arrange
        await _service.LoadAsync();

        // Act
        await _service.UpdateSettingAsync(s => s.ThumbnailSize = 256);

        // Assert
        var settings = _service.GetSettings();
        Assert.Equal(256, settings.ThumbnailSize);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_RestoresDefaults()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.UpdateSettingAsync(s => s.GridSpacing = 2000.0);

        // Act
        await _service.ResetToDefaultsAsync();

        // Assert
        var settings = _service.GetSettings();
        Assert.Equal(1200.0, settings.GridSpacing);
    }

    [Fact]
    public void ValidateSettings_WithValidSettings_ReturnsValid()
    {
        // Arrange
        var settings = new Settings
        {
            BaseServerPath = @"C:\Windows", // Existing directory
            ThumbnailSize = 192,
            GridSpacing = 1200.0
        };

        // Act
        var result = _service.ValidateSettings(settings);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateSettings_WithEmptyBasePath_ReturnsInvalid()
    {
        // Arrange
        var settings = new Settings { BaseServerPath = "" };

        // Act
        var result = _service.ValidateSettings(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("required"));
    }

    [Fact]
    public void ValidateSettings_WithNonExistentPath_ReturnsInvalid()
    {
        // Arrange
        var settings = new Settings { BaseServerPath = @"C:\NonExistent\Path" };

        // Act
        var result = _service.ValidateSettings(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("does not exist"));
    }

    [Fact]
    public void ValidateSettings_WithInvalidThumbnailSize_ReturnsInvalid()
    {
        // Arrange
        var settings = new Settings
        {
            BaseServerPath = @"C:\Windows",
            ThumbnailSize = 1000 // Too large
        };

        // Act
        var result = _service.ValidateSettings(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Thumbnail size"));
    }

    [Fact]
    public void ValidateSettings_WithNegativeGridSpacing_ReturnsInvalid()
    {
        // Arrange
        var settings = new Settings
        {
            BaseServerPath = @"C:\Windows",
            GridSpacing = -100.0
        };

        // Act
        var result = _service.ValidateSettings(settings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Grid spacing"));
    }

    [Fact]
    public void GetSettings_ReturnsCurrentSettings()
    {
        // Act
        var settings = _service.GetSettings();

        // Assert
        Assert.NotNull(settings);
    }

    // Cleanup
    ~SettingsServiceTests()
    {
        try
        {
            if (Directory.Exists(_testSettingsPath))
            {
                Directory.Delete(_testSettingsPath, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
