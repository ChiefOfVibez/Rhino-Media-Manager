using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using Xunit;

namespace BoschMediaBrowser.Tests;

public class SearchServiceTests
{
    private readonly SearchService _service;
    private readonly List<Product> _testProducts;

    public SearchServiceTests()
    {
        _service = new SearchService();
        _testProducts = CreateTestProducts();
    }

    [Fact]
    public void ApplyFilters_WithSearchText_FiltersProducts()
    {
        // Arrange
        var filters = new Filters { SearchText = "Drill" };

        // Act
        var results = _service.ApplyFilters(_testProducts, filters).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("GSR 18V-28", results[0].ProductName);
    }

    [Fact]
    public void ApplyFilters_WithRangeFilter_FiltersProducts()
    {
        // Arrange
        var filters = new Filters { Ranges = new List<string> { "PRO" } };

        // Act
        var results = _service.ApplyFilters(_testProducts, filters).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Equal("PRO", p.Range));
    }

    [Fact]
    public void ApplyFilters_WithCategoryFilter_FiltersProducts()
    {
        // Arrange
        var filters = new Filters { Categories = new List<string> { "Garden" } };

        // Act
        var results = _service.ApplyFilters(_testProducts, filters).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("Garden", results[0].Category);
    }

    [Fact]
    public void ApplyFilters_WithTagFilter_FiltersProducts()
    {
        // Arrange
        var filters = new Filters { TagsInclude = new List<string> { "cordless" } };

        // Act
        var results = _service.ApplyFilters(_testProducts, filters).ToList();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Sort_ByName_Ascending_SortsCorrectly()
    {
        // Act
        var results = _service.Sort(_testProducts, "name", "asc").ToList();

        // Assert
        Assert.Equal("GBH 2-28", results[0].ProductName);
        Assert.Equal("GSR 18V-28", results[1].ProductName);
        Assert.Equal("PST 700 E", results[2].ProductName);
    }

    [Fact]
    public void Sort_ByName_Descending_SortsCorrectly()
    {
        // Act
        var results = _service.Sort(_testProducts, "name", "desc").ToList();

        // Assert
        Assert.Equal("PST 700 E", results[0].ProductName);
        Assert.Equal("GSR 18V-28", results[1].ProductName);
        Assert.Equal("GBH 2-28", results[2].ProductName);
    }

    [Fact]
    public void Sort_ByCategory_SortsCorrectly()
    {
        // Act
        var results = _service.Sort(_testProducts, "category", "asc").ToList();

        // Assert
        Assert.Equal("Drilling", results[0].Category);
        Assert.Equal("Drilling", results[1].Category);
        Assert.Equal("Garden", results[2].Category);
    }

    [Fact]
    public void FilterAndSort_AppliesBoth()
    {
        // Arrange
        var filters = new Filters
        {
            Ranges = new List<string> { "PRO" },
            SortBy = "name",
            SortDir = "asc"
        };

        // Act
        var results = _service.FilterAndSort(_testProducts, filters).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("GBH 2-28", results[0].ProductName);
        Assert.Equal("GSR 18V-28", results[1].ProductName);
    }

    [Fact]
    public void Paginate_ReturnsCorrectPage()
    {
        // Act
        var result = _service.Paginate(_testProducts, page: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void Paginate_SecondPage_ReturnsCorrectItems()
    {
        // Act
        var result = _service.Paginate(_testProducts, page: 2, pageSize: 2);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void GetUniqueRanges_ReturnsDistinctRanges()
    {
        // Act
        var ranges = _service.GetUniqueRanges(_testProducts);

        // Assert
        Assert.Equal(2, ranges.Count);
        Assert.Contains("DIY", ranges);
        Assert.Contains("PRO", ranges);
    }

    [Fact]
    public void GetUniqueCategories_ReturnsDistinctCategories()
    {
        // Act
        var categories = _service.GetUniqueCategories(_testProducts);

        // Assert
        Assert.Equal(2, categories.Count);
        Assert.Contains("Drilling", categories);
        Assert.Contains("Garden", categories);
    }

    [Fact]
    public void BuildCategoryTree_CreatesHierarchy()
    {
        // Act
        var tree = _service.BuildCategoryTree(_testProducts);

        // Assert
        Assert.NotNull(tree);
        Assert.NotEmpty(tree.Children);
        Assert.True(tree.Children.Any(c => c.Name == "Tools and Holders"));
    }

    private List<Product> CreateTestProducts()
    {
        return new List<Product>
        {
            new Product
            {
                Id = "1",
                ProductName = "GSR 18V-28",
                Description = "Professional cordless drill",
                Range = "PRO",
                Category = "Drilling",
                TopCategory = "Tools and Holders",
                PathSegments = new List<string> { "Tools and Holders", "PRO", "Drilling" },
                Tags = new List<string> { "cordless", "drill" },
                Holders = new List<Holder>
                {
                    new Holder { Variant = "TEGO", Color = "RAL7016" }
                }
            },
            new Product
            {
                Id = "2",
                ProductName = "GBH 2-28",
                Description = "Rotary hammer",
                Range = "PRO",
                Category = "Drilling",
                TopCategory = "Tools and Holders",
                PathSegments = new List<string> { "Tools and Holders", "PRO", "Drilling" },
                Tags = new List<string> { "hammer" }
            },
            new Product
            {
                Id = "3",
                ProductName = "PST 700 E",
                Description = "Jigsaw",
                Range = "DIY",
                Category = "Garden",
                TopCategory = "Tools and Holders",
                PathSegments = new List<string> { "Tools and Holders", "DIY", "Garden" },
                Tags = new List<string> { "cordless", "saw" }
            }
        };
    }
}
