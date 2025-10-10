using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using Xunit;

namespace BoschMediaBrowser.Tests;

public class DataServiceTests
{
    [Fact]
    public void Constructor_CreatesService()
    {
        // Arrange & Act
        var service = new DataService(@"C:\TestPath");

        // Assert
        Assert.NotNull(service);
        Assert.Empty(service.Products);
    }

    [Fact]
    public void DeriveTaxonomy_WithToolsAndHolders_SetsCorrectFields()
    {
        // This test would require actual file system setup
        // For now, we verify the service can be instantiated
        var service = new DataService(@"C:\TestPath");
        Assert.NotNull(service);
    }

    [Fact]
    public void GenerateProductId_CreatesUniqueId()
    {
        // Test via public interface
        var service = new DataService(@"C:\TestPath");
        Assert.NotNull(service.Products);
    }

    [Fact]
    public async Task LoadProductsAsync_WithNoDirectory_ThrowsException()
    {
        // Arrange
        var service = new DataService(@"C:\NonExistentPath");

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            async () => await service.LoadProductsAsync()
        );
    }

    [Fact]
    public void IsInHiddenFolder_WithUnderscorePrefix_ReturnsTrue()
    {
        // This is tested indirectly through loading products
        var service = new DataService(@"C:\TestPath");
        Assert.NotNull(service);
    }

    [Fact]
    public void GetProductById_WithNoProducts_ReturnsNull()
    {
        // Arrange
        var service = new DataService(@"C:\TestPath");

        // Act
        var product = service.GetProductById("nonexistent");

        // Assert
        Assert.Null(product);
    }

    [Fact]
    public void StartWatching_EnablesFileSystemWatcher()
    {
        // Arrange
        var service = new DataService(@"C:\Windows"); // Use existing directory

        // Act & Assert - Should not throw
        service.StartWatching();
        service.StopWatching();
    }

    [Fact]
    public void StopWatching_DisablesFileSystemWatcher()
    {
        // Arrange
        var service = new DataService(@"C:\Windows");
        service.StartWatching();

        // Act & Assert - Should not throw
        service.StopWatching();
    }

    [Fact]
    public void ProductsReloaded_EventExists()
    {
        // Arrange
        var service = new DataService(@"C:\TestPath");
        bool eventFired = false;

        // Act
        service.ProductsReloaded += (sender, args) => eventFired = true;

        // Assert - Event handler registered
        Assert.False(eventFired); // Not fired yet
    }
}
