using System.Text.Json;
using BoschMediaBrowser.Core.Models;

namespace BoschMediaBrowser.Core.Services;

/// <summary>
/// Service for loading and managing product data from JSON files
/// Handles JSON loading, caching, FileSystemWatcher, and category derivation
/// </summary>
public class DataService
{
    private readonly string _basePath;
    private List<Product> _cachedProducts = new();
    private DateTime _lastLoadTime = DateTime.MinValue;
    private FileSystemWatcher? _watcher;
    private Action<string>? _logger;

    public DataService(string basePath, Action<string>? logger = null)
    {
        _basePath = basePath;
        _logger = logger;
    }
    
    private void Log(string message)
    {
        _logger?.Invoke(message);
        System.Diagnostics.Debug.WriteLine(message);
    }

    /// <summary>
    /// Gets all loaded products
    /// </summary>
    public IReadOnlyList<Product> Products => _cachedProducts.AsReadOnly();

    /// <summary>
    /// Get all products (alias for UI compatibility)
    /// </summary>
    public List<Product> GetProducts()
    {
        return _cachedProducts;
    }

    /// <summary>
    /// Event fired when products are reloaded
    /// </summary>
    public event EventHandler? ProductsReloaded;

    /// <summary>
    /// Load all product JSONs from the base path
    /// </summary>
    public async Task<int> LoadProductsAsync(CancellationToken cancellationToken = default)
    {

        if (!Directory.Exists(_basePath))
        {
            throw new DirectoryNotFoundException($"Base path not found: {_basePath}");
        }

        var products = new List<Product>();
        var jsonFiles = Directory.GetFiles(_basePath, "*.json", SearchOption.AllDirectories);
        
        Log($"=== DataService: Found {jsonFiles.Length} JSON files ===");

        foreach (var jsonFile in jsonFiles)
        {
            // Skip files in underscore-prefixed folders (e.g., _public-collections, _holders)
            if (IsInHiddenFolder(jsonFile))
            {
                Log($"  SKIPPED (hidden): {Path.GetFileName(jsonFile)}");
                continue;
            }

            try
            {
                var product = await LoadProductFromFileAsync(jsonFile, cancellationToken);
                if (product != null)
                {
                    // Derive taxonomy if missing
                    EnsureTaxonomy(product);
                    products.Add(product);
                    Log($"  ✓ LOADED: {product.ProductName} from {Path.GetFileName(jsonFile)}");
                }
                else
                {
                    Log($"  ✗ SKIPPED (null): {Path.GetFileName(jsonFile)}");
                    Log($"    Deserialization returned null - check JSON structure");
                }
            }
            catch (Exception ex)
            {
                // Log error for debugging (products not loading)
                Log($"  ✗ FAILED: {Path.GetFileName(jsonFile)}");
                Log($"    Error: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Log($"    Inner: {ex.InnerException.Message}");
                }
            }
        }

        _cachedProducts = products;
        _lastLoadTime = DateTime.UtcNow;

        ProductsReloaded?.Invoke(this, EventArgs.Empty);

        return products.Count;
    }

    /// <summary>
    /// Reload products from disk
    /// </summary>
    public async Task<int> ReloadAsync(CancellationToken cancellationToken = default)
    {
        return await LoadProductsAsync(cancellationToken);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    public Product? GetProductById(string productId)
    {
        return _cachedProducts.FirstOrDefault(p => p.Id == productId);
    }

    /// <summary>
    /// Start watching for file changes
    /// </summary>
    public void StartWatching()
    {
        if (_watcher != null)
        {
            return; // Already watching
        }

        _watcher = new FileSystemWatcher(_basePath)
        {
            Filter = "*.json",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;

        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Stop watching for file changes
    /// </summary>
    public void StopWatching()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    /// <summary>
    /// Load a single product from JSON file
    /// </summary>
    private async Task<Product?> LoadProductFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            
            // Try to deserialize
            var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (product != null)
            {
                // Set folder path from file location
                product.FolderPath = Path.GetDirectoryName(filePath) ?? string.Empty;

                // Generate ID if missing
                if (string.IsNullOrEmpty(product.Id))
                {
                    product.Id = GenerateProductId(product);
                }
                
                System.Diagnostics.Debug.WriteLine($"✓ Loaded product: {product.ProductName} (ID: {product.Id})");
                return product;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✗ Failed to deserialize: {filePath}");
                Log($"    JSON preview (first 200 chars): {json.Substring(0, Math.Min(200, json.Length))}");
                return null;
            }
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"✗ JSON ERROR loading {Path.GetFileName(filePath)}: {jsonEx.Message}");
            Log($"    JSON Parse Error in {Path.GetFileName(filePath)}: {jsonEx.Message}");
            if (jsonEx.InnerException != null)
            {
                Log($"    Inner: {jsonEx.InnerException.Message}");
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"✗ ERROR loading {Path.GetFileName(filePath)}: {ex.Message}");
            Log($"    Unexpected error: {ex.GetType().Name} - {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Ensure product has taxonomy fields (derive from folder path if missing)
    /// </summary>
    private void EnsureTaxonomy(Product product)
    {
        if (string.IsNullOrEmpty(product.CategoryPath) || string.IsNullOrEmpty(product.Range))
        {
            DeriveTaxonomyFromPath(product);
        }
    }

    /// <summary>
    /// Derive taxonomy fields from folder path
    /// Supports optional "Tools and Holders" wrapper folder
    /// Range is identified by first occurrence of DIY or PRO
    /// </summary>
    private void DeriveTaxonomyFromPath(Product product)
    {
        if (string.IsNullOrEmpty(product.FolderPath))
        {
            return;
        }

        // Get relative path from base
        var relativePath = Path.GetRelativePath(_basePath, product.FolderPath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return;
        }

        // Find first occurrence of DIY or PRO
        int rangeIndex = -1;
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i].Equals("DIY", StringComparison.OrdinalIgnoreCase) ||
                segments[i].Equals("PRO", StringComparison.OrdinalIgnoreCase))
            {
                rangeIndex = i;
                product.Range = segments[i].ToUpper();
                break;
            }
        }

        if (rangeIndex == -1)
        {
            // No DIY/PRO found, use first segment as range
            product.Range = segments[0];
            rangeIndex = 0;
        }

        // Top category is everything before the range
        if (rangeIndex > 0)
        {
            product.TopCategory = string.Join(" > ", segments.Take(rangeIndex));
        }
        else
        {
            product.TopCategory = null; // No wrapper folder
        }

        // Category is the first segment after range (if exists)
        if (rangeIndex + 1 < segments.Length)
        {
            product.Category = segments[rangeIndex + 1];
        }

        // Build full category path
        var pathSegments = new List<string>();
        if (!string.IsNullOrEmpty(product.TopCategory))
        {
            pathSegments.Add(product.TopCategory);
        }
        pathSegments.Add(product.Range);
        if (rangeIndex + 1 < segments.Length)
        {
            // Add remaining segments except the last (product folder)
            pathSegments.AddRange(segments.Skip(rangeIndex + 1).Take(segments.Length - rangeIndex - 2));
        }

        product.CategoryPath = string.Join(" > ", pathSegments);
        product.PathSegments = pathSegments;
    }

    /// <summary>
    /// Check if file is in a hidden folder (underscore-prefixed)
    /// Skip specific system folders like _public-collections, _holders, _templates
    /// but allow __NEW DB__ and other folders
    /// </summary>
    private bool IsInHiddenFolder(string filePath)
    {
        var relativePath = Path.GetRelativePath(_basePath, filePath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar);

        // Skip only specific hidden folders
        var hiddenFolders = new[] { "_public-collections", "_holders", "_templates", "_archive", "_backup" };
        return segments.Any(s => hiddenFolders.Contains(s, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Generate unique product ID from folder path and name
    /// </summary>
    private string GenerateProductId(Product product)
    {
        var relativePath = Path.GetRelativePath(_basePath, product.FolderPath);
        var normalized = relativePath.Replace(Path.DirectorySeparatorChar, '_').Replace(" ", "_");
        return $"{normalized}_{product.ProductName}";
    }

    /// <summary>
    /// Handle file change events
    /// </summary>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce: only reload if last load was more than 2 seconds ago
        if ((DateTime.UtcNow - _lastLoadTime).TotalSeconds > 2)
        {
            _ = Task.Run(() => ReloadAsync());
        }
    }

    /// <summary>
    /// Handle file rename events
    /// </summary>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        _ = Task.Run(() => ReloadAsync());
    }

    /// <summary>
    /// Dispose and stop watching
    /// </summary>
    public void Dispose()
    {
        StopWatching();
    }
}
