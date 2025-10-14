using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using Xunit;

namespace BoschMediaBrowser.Tests;

public class ThumbnailServiceTests
{
    private readonly string _testCachePath;
    private readonly ThumbnailService _service;

    public ThumbnailServiceTests()
    {
        _testCachePath = Path.Combine(Path.GetTempPath(), "BoschMediaBrowserTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testCachePath);
        _service = new ThumbnailService(_testCachePath);
    }

    [Fact]
    public void Constructor_CreatesCacheDirectory()
    {
        // Assert
        Assert.True(Directory.Exists(_testCachePath));
    }

    [Fact]
    public void GetThumbnailPath_WithNoPreview_ReturnsEmpty()
    {
        // Arrange
        var product = new Product { Id = "test" };

        // Act
        var path = _service.GetThumbnailPath(product, PreviewType.Mesh);

        // Assert
        Assert.Empty(path);
    }

    [Fact]
    public void GetThumbnailPath_WithNonExistentFile_ReturnsEmpty()
    {
        // Arrange
        var product = new Product
        {
            Id = "test",
            Previews = new PreviewRefs
            {
                MeshPreview = new PreviewImage
                {
                    FullPath = @"C:\NonExistent\file.png"
                }
            }
        };

        // Act
        var path = _service.GetThumbnailPath(product, PreviewType.Mesh);

        // Assert
        Assert.Empty(path);
    }

    [Fact]
    public async Task CacheThumbnailAsync_WithNonExistentSource_ReturnsNull()
    {
        // Arrange
        var product = new Product
        {
            Id = "test",
            Previews = new PreviewRefs
            {
                MeshPreview = new PreviewImage
                {
                    FullPath = @"C:\NonExistent\file.png"
                }
            }
        };

        // Act
        var result = await _service.CacheThumbnailAsync(product, PreviewType.Mesh);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ClearCache_RemovesAllFiles()
    {
        // Arrange - Create a test file
        var testFile = Path.Combine(_testCachePath, "test.txt");
        File.WriteAllText(testFile, "test");

        // Act
        _service.ClearCache();

        // Assert
        Assert.Empty(Directory.GetFiles(_testCachePath));
    }

    [Fact]
    public void GetCacheStats_ReturnsCorrectStats()
    {
        // Arrange - Create test files
        File.WriteAllText(Path.Combine(_testCachePath, "file1.txt"), "test1");
        File.WriteAllText(Path.Combine(_testCachePath, "file2.txt"), "test2");

        // Act
        var stats = _service.GetCacheStats();

        // Assert
        Assert.Equal(2, stats.FileCount);
        Assert.True(stats.TotalSizeBytes > 0);
        Assert.Equal(_testCachePath, stats.CachePath);
    }

    [Fact]
    public void ClearExpiredCache_RemovesOldFiles()
    {
        // Arrange - Create test file with old timestamp
        var oldFile = Path.Combine(_testCachePath, "old.txt");
        File.WriteAllText(oldFile, "test");
        File.SetLastAccessTime(oldFile, DateTime.UtcNow.AddDays(-35));

        // Act
        _service.ClearExpiredCache(expirationDays: 30);

        // Assert
        Assert.Empty(Directory.GetFiles(_testCachePath));
    }

    [Fact]
    public async Task PreCacheThumbnailsAsync_WithNoValidPreviews_ReturnsZero()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = "1" },
            new Product { Id = "2" }
        };

        // Act
        var count = await _service.PreCacheThumbnailsAsync(products);

        // Assert
        Assert.Equal(0, count);
    }

    // Cleanup
    ~ThumbnailServiceTests()
    {
        try
        {
            if (Directory.Exists(_testCachePath))
            {
                Directory.Delete(_testCachePath, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
