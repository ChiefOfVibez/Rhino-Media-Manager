using BoschMediaBrowser.Core.Models;

namespace BoschMediaBrowser.Core.Services;

/// <summary>
/// Service for managing thumbnail previews and caching
/// </summary>
public class ThumbnailService
{
    private readonly string _cachePath;
    private readonly Dictionary<string, string> _cacheIndex = new();

    public ThumbnailService(string cachePath)
    {
        _cachePath = cachePath;

        // Ensure cache directory exists
        if (!Directory.Exists(_cachePath))
        {
            Directory.CreateDirectory(_cachePath);
        }
    }

    /// <summary>
    /// Get thumbnail path for a product preview
    /// Returns cached path if exists, otherwise returns original path
    /// </summary>
    public string GetThumbnailPath(Product product, PreviewType previewType)
    {
        var previewPath = GetPreviewFullPath(product, previewType);

        if (string.IsNullOrEmpty(previewPath) || !File.Exists(previewPath))
        {
            return string.Empty;
        }

        // Check if cached version exists
        var cacheKey = GenerateCacheKey(product.Id, previewType);
        if (_cacheIndex.TryGetValue(cacheKey, out var cachedPath) && File.Exists(cachedPath))
        {
            return cachedPath;
        }

        // Return original path (can be cached later)
        return previewPath;
    }

    /// <summary>
    /// Cache a thumbnail (copy to cache directory)
    /// </summary>
    public async Task<string?> CacheThumbnailAsync(Product product, PreviewType previewType, CancellationToken cancellationToken = default)
    {
        var sourcePath = GetPreviewFullPath(product, previewType);

        if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
        {
            return null;
        }

        var cacheKey = GenerateCacheKey(product.Id, previewType);
        var extension = Path.GetExtension(sourcePath);
        var cachedFileName = $"{cacheKey}{extension}";
        var cachedPath = Path.Combine(_cachePath, cachedFileName);

        try
        {
            // Copy file to cache
            await CopyFileAsync(sourcePath, cachedPath, cancellationToken);

            // Update cache index
            _cacheIndex[cacheKey] = cachedPath;

            return cachedPath;
        }
        catch (Exception)
        {
            return sourcePath; // Return original on failure
        }
    }

    /// <summary>
    /// Pre-cache thumbnails for multiple products
    /// </summary>
    public async Task<int> PreCacheThumbnailsAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
    {
        int cachedCount = 0;

        foreach (var product in products)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // Cache mesh preview (most important)
            var cached = await CacheThumbnailAsync(product, PreviewType.Mesh, cancellationToken);
            if (cached != null)
            {
                cachedCount++;
            }
        }
        return cachedCount;
    }

    /// <summary>
    /// Clear all cached thumbnails
    /// </summary>
    public void ClearCache()
    {
        try
        {
            var files = Directory.GetFiles(_cachePath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            _cacheIndex.Clear();
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Clear expired cache entries (older than specified days)
    /// </summary>
    public void ClearExpiredCache(int expirationDays = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-expirationDays);
            var files = Directory.GetFiles(_cachePath);
            int deletedCount = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastAccessTime < cutoffDate)
                {
                    File.Delete(file);
                    deletedCount++;

                    // Remove from index
                    var cacheKey = _cacheIndex.FirstOrDefault(kvp => kvp.Value == file).Key;
                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        _cacheIndex.Remove(cacheKey);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public CacheStats GetCacheStats()
    {
        var files = Directory.GetFiles(_cachePath);
        var totalSize = files.Sum(f => new FileInfo(f).Length);

        return new CacheStats
        {
            FileCount = files.Length,
            TotalSizeBytes = totalSize,
            TotalSizeMB = totalSize / (1024.0 * 1024.0),
            CachePath = _cachePath
        };
    }

    /// <summary>
    /// Get preview full path from product
    /// </summary>
    private string GetPreviewFullPath(Product product, PreviewType previewType)
    {
        return previewType switch
        {
            PreviewType.Mesh => product.Previews.MeshPreview?.FullPath ?? string.Empty,
            PreviewType.Grafica => product.Previews.GraficaPreview?.FullPath ?? string.Empty,
            PreviewType.Packaging => product.Packaging?.PreviewPath ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Generate cache key for a product preview
    /// </summary>
    private string GenerateCacheKey(string productId, PreviewType previewType)
    {
        var normalized = productId.Replace(Path.DirectorySeparatorChar, '_').Replace(" ", "_");
        return $"{normalized}_{previewType}";
    }

    /// <summary>
    /// Copy file asynchronously
    /// </summary>
    private async Task CopyFileAsync(string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        const int bufferSize = 81920; // 80KB buffer

        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
        using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);

        await sourceStream.CopyToAsync(destStream, bufferSize, cancellationToken);
    }
}

/// <summary>
/// Preview type enumeration
/// </summary>
public enum PreviewType
{
    Mesh,
    Grafica,
    Packaging
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStats
{
    public int FileCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public double TotalSizeMB { get; set; }
    public string CachePath { get; set; } = string.Empty;
}
