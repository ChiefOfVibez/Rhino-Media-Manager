using System.Text.Json;
using BoschMediaBrowser.Core.Models;

namespace BoschMediaBrowser.Core.Services;

/// <summary>
/// Service for persisting user data (favourites, tags, collections)
/// Data stored in %AppData%/BoschMediaBrowser/
/// </summary>
public class UserDataService
{
    private readonly string _dataPath;
    private readonly string _userDataFile;
    private UserData _userData = new();

    public UserDataService(string? dataPath = null)
    {

        // Default to %AppData%/BoschMediaBrowser/ if not specified
        _dataPath = dataPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BoschMediaBrowser"
        );

        _userDataFile = Path.Combine(_dataPath, "userdata.json");

        // Ensure directory exists
        if (!Directory.Exists(_dataPath))
        {
            Directory.CreateDirectory(_dataPath);
        }
    }

    /// <summary>
    /// Get current user data
    /// </summary>
    public UserData GetUserData() => _userData;

    /// <summary>
    /// Load user data from disk
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_userDataFile))
        {
            _userData = new UserData();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_userDataFile, cancellationToken);
            _userData = JsonSerializer.Deserialize<UserData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new UserData();
        }
        catch (Exception)
        {
            _userData = new UserData();
        }
    }

    /// <summary>
    /// Save user data to disk
    /// </summary>
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _userData.LastSaved = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(_userData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_userDataFile, json, cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
    }

    // ===== Favourites =====

    /// <summary>
    /// Check if product is favourited
    /// </summary>
    public bool IsFavourite(string productId)
    {
        return _userData.Favourites.Any(f => f.ProductId == productId);
    }

    /// <summary>
    /// Add product to favourites
    /// </summary>
    public async Task<bool> AddFavouriteAsync(string productId)
    {
        if (IsFavourite(productId))
        {
            return false; // Already favourited
        }

        _userData.Favourites.Add(new Favourite
        {
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        });

        await SaveAsync();
        return true;
    }

    /// <summary>
    /// Remove product from favourites
    /// </summary>
    public async Task<bool> RemoveFavouriteAsync(string productId)
    {
        var favourite = _userData.Favourites.FirstOrDefault(f => f.ProductId == productId);
        if (favourite == null)
        {
            return false;
        }

        _userData.Favourites.Remove(favourite);
        await SaveAsync();
        return true;
    }

    /// <summary>
    /// Get all favourite product IDs
    /// </summary>
    public List<string> GetFavouriteProductIds()
    {
        return _userData.Favourites.Select(f => f.ProductId).ToList();
    }

    /// <summary>
    /// Get all favourites (for UI)
    /// </summary>
    public List<Favourite> GetAllFavourites()
    {
        return _userData.Favourites;
    }

    /// <summary>
    /// Remove favourite (synchronous wrapper for UI)
    /// </summary>
    public bool RemoveFavourite(string productId)
    {
        return RemoveFavouriteAsync(productId).GetAwaiter().GetResult();
    }

    // ===== Tags =====

    /// <summary>
    /// Get tags for a product
    /// </summary>
    public List<string> GetProductTags(string productId)
    {
        return _userData.Tags
            .Where(t => t.ProductId == productId)
            .Select(t => t.Name)
            .ToList();
    }

    /// <summary>
    /// Add tag to product
    /// </summary>
    public async Task<bool> AddTagAsync(string productId, string tagName)
    {
        // Check if tag already exists
        if (_userData.Tags.Any(t => t.ProductId == productId && t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        _userData.Tags.Add(new Tag
        {
            ProductId = productId,
            Name = tagName,
            CreatedAt = DateTime.UtcNow
        });

        await SaveAsync();
        return true;
    }

    /// <summary>
    /// Remove tag from product
    /// </summary>
    public async Task<bool> RemoveTagAsync(string productId, string tagName)
    {
        var tag = _userData.Tags.FirstOrDefault(t =>
            t.ProductId == productId &&
            t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));

        if (tag == null)
        {
            return false;
        }

        _userData.Tags.Remove(tag);
        await SaveAsync();
        return true;
    }

    /// <summary>
    /// Get all unique tag names
    /// </summary>
    public List<string> GetAllTagNames()
    {
        return _userData.Tags
            .Select(t => t.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    /// <summary>
    /// Get tags for product (alias for UI compatibility)
    /// </summary>
    public List<string> GetTagsForProduct(string productId)
    {
        return GetProductTags(productId);
    }

    // ===== Collections =====

    /// <summary>
    /// Get all collections
    /// </summary>
    public List<Collection> GetCollections()
    {
        return _userData.Collections;
    }

    /// <summary>
    /// Get collection by ID
    /// </summary>
    public Collection? GetCollectionById(string collectionId)
    {
        return _userData.Collections.FirstOrDefault(c => c.Id == collectionId);
    }

    /// <summary>
    /// Create new collection
    /// </summary>
    public async Task<Collection> CreateCollectionAsync(string name, List<string>? productIds = null)
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            ProductIds = productIds ?? new List<string>(),
            CreatedAt = DateTime.UtcNow
        };

        _userData.Collections.Add(collection);
        await SaveAsync();

        return collection;
    }

    /// <summary>
    /// Update collection
    /// </summary>
    public async Task<bool> UpdateCollectionAsync(Collection collection)
    {
        var existing = GetCollectionById(collection.Id);
        if (existing == null)
        {
            return false;
        }

        existing.Name = collection.Name;
        existing.ProductIds = collection.ProductIds;
        existing.LastModified = DateTime.UtcNow;

        await SaveAsync();
        return true;
    }

    /// <summary>
    /// Delete collection
    /// </summary>
    public async Task<bool> DeleteCollectionAsync(string collectionId)
    {
        var collection = GetCollectionById(collectionId);
        if (collection == null)
        {
            return false;
        }

        _userData.Collections.Remove(collection);
        await SaveAsync();
        return true;
    }

    /// <summary>
    /// Add product to collection
    /// </summary>
    public async Task<bool> AddToCollectionAsync(string collectionId, string productId)
    {
        var collection = GetCollectionById(collectionId);
        if (collection == null || collection.ProductIds.Contains(productId))
        {
            return false;
        }

        collection.ProductIds.Add(productId);
        collection.LastModified = DateTime.UtcNow;
        await SaveAsync();
        return true;
    }

    /// <summary>
    /// Remove product from collection
    /// </summary>
    public async Task<bool> RemoveFromCollectionAsync(string collectionId, string productId)
    {
        var collection = GetCollectionById(collectionId);
        if (collection == null)
        {
            return false;
        }

        var removed = collection.ProductIds.Remove(productId);
        if (removed)
        {
            collection.LastModified = DateTime.UtcNow;
            await SaveAsync();
        }

        return removed;
    }

    // ===== Synchronous wrappers for UI =====

    /// <summary>
    /// Create collection (synchronous wrapper)
    /// </summary>
    public Collection CreateCollection(string name, string? description = null)
    {
        return CreateCollectionAsync(name).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Update collection (synchronous wrapper)
    /// </summary>
    public bool UpdateCollection(Collection collection)
    {
        return UpdateCollectionAsync(collection).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Delete collection (synchronous wrapper)
    /// </summary>
    public bool DeleteCollection(string collectionId)
    {
        return DeleteCollectionAsync(collectionId).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Add product to collection (synchronous wrapper)
    /// </summary>
    public bool AddProductToCollection(string collectionId, string productId)
    {
        return AddToCollectionAsync(collectionId, productId).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Remove product from collection (synchronous wrapper)
    /// </summary>
    public bool RemoveProductFromCollection(string collectionId, string productId)
    {
        return RemoveFromCollectionAsync(collectionId, productId).GetAwaiter().GetResult();
    }

    // ===== Layout Collections =====

    /// <summary>
    /// Get all local layout collections
    /// </summary>
    public List<LayoutCollection> GetLayoutCollections()
    {
        return _userData.LayoutCollections.Where(lc => lc.Scope == CollectionScope.Local).ToList();
    }

    /// <summary>
    /// Get layout collection by ID
    /// </summary>
    public LayoutCollection? GetLayoutCollectionById(string layoutId)
    {
        return _userData.LayoutCollections.FirstOrDefault(lc => lc.Id == layoutId);
    }

    /// <summary>
    /// Create new layout collection
    /// </summary>
    public async Task<LayoutCollection> CreateLayoutCollectionAsync(string name, List<LayoutItem> items, CollectionScope scope = CollectionScope.Local)
    {
        var layout = new LayoutCollection
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Scope = scope,
            Items = items,
            CreatedAt = DateTime.UtcNow
        };

        if (scope == CollectionScope.Local)
        {
            _userData.LayoutCollections.Add(layout);
            await SaveAsync();
        }

        return layout;
    }

    /// <summary>
    /// Delete layout collection
    /// </summary>
    public async Task<bool> DeleteLayoutCollectionAsync(string layoutId)
    {
        var layout = GetLayoutCollectionById(layoutId);
        if (layout == null || layout.Scope != CollectionScope.Local)
        {
            return false; // Can't delete public layouts from local data
        }

        _userData.LayoutCollections.Remove(layout);
        await SaveAsync();
        return true;
    }
}
