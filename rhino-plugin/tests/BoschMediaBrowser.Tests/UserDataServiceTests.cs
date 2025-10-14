using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using Xunit;

namespace BoschMediaBrowser.Tests;

public class UserDataServiceTests
{
    private readonly string _testDataPath;
    private readonly UserDataService _service;

    public UserDataServiceTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "BoschMediaBrowserTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _service = new UserDataService(_testDataPath);
    }

    [Fact]
    public async Task LoadAsync_WithNoFile_CreatesDefault()
    {
        // Act
        await _service.LoadAsync();
        var userData = _service.GetUserData();

        // Assert
        Assert.NotNull(userData);
        Assert.Empty(userData.Favourites);
        Assert.Empty(userData.Tags);
        Assert.Empty(userData.Collections);
    }

    [Fact]
    public async Task AddFavouriteAsync_AddsToList()
    {
        // Arrange
        await _service.LoadAsync();

        // Act
        var added = await _service.AddFavouriteAsync("product1");

        // Assert
        Assert.True(added);
        Assert.True(_service.IsFavourite("product1"));
    }

    [Fact]
    public async Task AddFavouriteAsync_Duplicate_ReturnsFalse()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.AddFavouriteAsync("product1");

        // Act
        var added = await _service.AddFavouriteAsync("product1");

        // Assert
        Assert.False(added);
    }

    [Fact]
    public async Task RemoveFavouriteAsync_RemovesFromList()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.AddFavouriteAsync("product1");

        // Act
        var removed = await _service.RemoveFavouriteAsync("product1");

        // Assert
        Assert.True(removed);
        Assert.False(_service.IsFavourite("product1"));
    }

    [Fact]
    public async Task GetFavouriteProductIds_ReturnsAllIds()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.AddFavouriteAsync("product1");
        await _service.AddFavouriteAsync("product2");

        // Act
        var ids = _service.GetFavouriteProductIds();

        // Assert
        Assert.Equal(2, ids.Count);
        Assert.Contains("product1", ids);
        Assert.Contains("product2", ids);
    }

    [Fact]
    public async Task AddTagAsync_AddsToProduct()
    {
        // Arrange
        await _service.LoadAsync();

        // Act
        var added = await _service.AddTagAsync("product1", "important");

        // Assert
        Assert.True(added);
        var tags = _service.GetProductTags("product1");
        Assert.Contains("important", tags);
    }

    [Fact]
    public async Task AddTagAsync_Duplicate_ReturnsFalse()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.AddTagAsync("product1", "important");

        // Act
        var added = await _service.AddTagAsync("product1", "important");

        // Assert
        Assert.False(added);
    }

    [Fact]
    public async Task RemoveTagAsync_RemovesFromProduct()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.AddTagAsync("product1", "important");

        // Act
        var removed = await _service.RemoveTagAsync("product1", "important");

        // Assert
        Assert.True(removed);
        var tags = _service.GetProductTags("product1");
        Assert.Empty(tags);
    }

    [Fact]
    public async Task GetAllTagNames_ReturnsUniqueTagNames()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.AddTagAsync("product1", "tag1");
        await _service.AddTagAsync("product2", "tag2");
        await _service.AddTagAsync("product3", "tag1");

        // Act
        var tagNames = _service.GetAllTagNames();

        // Assert
        Assert.Equal(2, tagNames.Count);
        Assert.Contains("tag1", tagNames);
        Assert.Contains("tag2", tagNames);
    }

    [Fact]
    public async Task CreateCollectionAsync_CreatesCollection()
    {
        // Arrange
        await _service.LoadAsync();

        // Act
        var collection = await _service.CreateCollectionAsync("My Collection", new List<string> { "product1", "product2" });

        // Assert
        Assert.NotNull(collection);
        Assert.Equal("My Collection", collection.Name);
        Assert.Equal(2, collection.ProductIds.Count);
    }

    [Fact]
    public async Task GetCollectionById_ReturnsCorrectCollection()
    {
        // Arrange
        await _service.LoadAsync();
        var created = await _service.CreateCollectionAsync("Test");

        // Act
        var retrieved = _service.GetCollectionById(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
    }

    [Fact]
    public async Task DeleteCollectionAsync_RemovesCollection()
    {
        // Arrange
        await _service.LoadAsync();
        var collection = await _service.CreateCollectionAsync("Test");

        // Act
        var deleted = await _service.DeleteCollectionAsync(collection.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(_service.GetCollectionById(collection.Id));
    }

    [Fact]
    public async Task AddToCollectionAsync_AddsProduct()
    {
        // Arrange
        await _service.LoadAsync();
        var collection = await _service.CreateCollectionAsync("Test");

        // Act
        var added = await _service.AddToCollectionAsync(collection.Id, "product1");

        // Assert
        Assert.True(added);
        var updated = _service.GetCollectionById(collection.Id);
        Assert.Contains("product1", updated!.ProductIds);
    }

    [Fact]
    public async Task RemoveFromCollectionAsync_RemovesProduct()
    {
        // Arrange
        await _service.LoadAsync();
        var collection = await _service.CreateCollectionAsync("Test", new List<string> { "product1" });

        // Act
        var removed = await _service.RemoveFromCollectionAsync(collection.Id, "product1");

        // Assert
        Assert.True(removed);
        var updated = _service.GetCollectionById(collection.Id);
        Assert.Empty(updated!.ProductIds);
    }

    [Fact]
    public async Task CreateLayoutCollectionAsync_CreatesLayout()
    {
        // Arrange
        await _service.LoadAsync();
        var items = new List<LayoutItem>
        {
            new LayoutItem
            {
                ProductId = "product1",
                HolderVariant = "TEGO",
                IncludeHolder = true,
                Transform = new Transform { X = 0, Y = 0, Z = 0 }
            }
        };

        // Act
        var layout = await _service.CreateLayoutCollectionAsync("Demo Layout", items, CollectionScope.Local);

        // Assert
        Assert.NotNull(layout);
        Assert.Equal("Demo Layout", layout.Name);
        Assert.Single(layout.Items);
        Assert.Equal(CollectionScope.Local, layout.Scope);
    }

    [Fact]
    public async Task GetLayoutCollections_ReturnsLocalCollections()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.CreateLayoutCollectionAsync("Layout1", new List<LayoutItem>(), CollectionScope.Local);

        // Act
        var layouts = _service.GetLayoutCollections();

        // Assert
        Assert.Single(layouts);
        Assert.Equal("Layout1", layouts[0].Name);
    }

    [Fact]
    public async Task SaveAsync_PersistsData()
    {
        // Arrange
        await _service.LoadAsync();
        await _service.AddFavouriteAsync("product1");

        // Act
        await _service.SaveAsync();

        // Create new service instance to load saved data
        var newService = new UserDataService(_testDataPath);
        await newService.LoadAsync();

        // Assert
        Assert.True(newService.IsFavourite("product1"));
    }

    // Cleanup
    ~UserDataServiceTests()
    {
        try
        {
            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
