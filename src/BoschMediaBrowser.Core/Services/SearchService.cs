using BoschMediaBrowser.Core.Models;

namespace BoschMediaBrowser.Core.Services;

/// <summary>
/// Service for filtering, sorting, and searching products
/// </summary>
public class SearchService
{
    /// <summary>
    /// Apply filters to product list
    /// </summary>
    public IEnumerable<Product> ApplyFilters(IEnumerable<Product> products, Filters filters)
    {
        var filtered = products;

        // Search text (name, SKU, description, tags)
        if (!string.IsNullOrWhiteSpace(filters.SearchText))
        {
            var searchLower = filters.SearchText.ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.ProductName.ToLowerInvariant().Contains(searchLower) ||
                (p.Sku?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (p.Description?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                p.Tags.Any(t => t.ToLowerInvariant().Contains(searchLower))
            );
        }

        // Filter by ranges
        if (filters.Ranges.Any())
        {
            filtered = filtered.Where(p => filters.Ranges.Contains(p.Range));
        }

        // Filter by categories
        if (filters.Categories.Any())
        {
            filtered = filtered.Where(p => filters.Categories.Contains(p.Category));
        }

        // Filter by holder variants
        if (filters.HolderVariants.Any())
        {
            filtered = filtered.Where(p =>
                p.Holders.Any(h => filters.HolderVariants.Contains(h.Variant))
            );
        }

        // Filter by tags
        if (filters.TagsInclude.Any())
        {
            filtered = filtered.Where(p =>
                filters.TagsInclude.All(tag => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            );
        }

        return filtered;
    }

    /// <summary>
    /// Sort products by specified criteria
    /// </summary>
    public IEnumerable<Product> Sort(IEnumerable<Product> products, string sortBy, string sortDir)
    {
        var ascending = sortDir.Equals("asc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "name" => ascending
                ? products.OrderBy(p => p.ProductName)
                : products.OrderByDescending(p => p.ProductName),

            "category" => ascending
                ? products.OrderBy(p => p.Category).ThenBy(p => p.ProductName)
                : products.OrderByDescending(p => p.Category).ThenByDescending(p => p.ProductName),

            "range" => ascending
                ? products.OrderBy(p => p.Range).ThenBy(p => p.ProductName)
                : products.OrderByDescending(p => p.Range).ThenByDescending(p => p.ProductName),

            "sku" => ascending
                ? products.OrderBy(p => p.Sku ?? string.Empty).ThenBy(p => p.ProductName)
                : products.OrderByDescending(p => p.Sku ?? string.Empty).ThenByDescending(p => p.ProductName),

            _ => products // No sort or unknown sort
        };
    }

    /// <summary>
    /// Apply filters and sorting in one pass
    /// </summary>
    public IEnumerable<Product> FilterAndSort(IEnumerable<Product> products, Filters filters)
    {
        var filtered = ApplyFilters(products, filters);
        return Sort(filtered, filters.SortBy, filters.SortDir);
    }

    /// <summary>
    /// Paginate results
    /// </summary>
    public PagedResult<Product> Paginate(IEnumerable<Product> products, int page, int pageSize)
    {
        var productList = products.ToList();
        var totalCount = productList.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = productList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Product>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// Get unique ranges from products
    /// </summary>
    public List<string> GetUniqueRanges(IEnumerable<Product> products)
    {
        return products
            .Select(p => p.Range)
            .Where(r => !string.IsNullOrEmpty(r))
            .Distinct()
            .OrderBy(r => r)
            .ToList();
    }

    /// <summary>
    /// Get unique categories from products
    /// </summary>
    public List<string> GetUniqueCategories(IEnumerable<Product> products)
    {
        return products
            .Select(p => p.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <summary>
    /// Get unique holder variants from products
    /// </summary>
    public List<string> GetUniqueHolderVariants(IEnumerable<Product> products)
    {
        return products
            .SelectMany(p => p.Holders.Select(h => h.Variant))
            .Where(v => !string.IsNullOrEmpty(v))
            .Distinct()
            .OrderBy(v => v)
            .ToList();
    }

    /// <summary>
    /// Build category tree from products
    /// </summary>
    public CategoryNode BuildCategoryTree(IEnumerable<Product> products)
    {
        var root = new CategoryNode { Name = "Root", Path = string.Empty };

        foreach (var product in products)
        {
            if (product.PathSegments == null || !product.PathSegments.Any())
            {
                continue;
            }

            var currentNode = root;
            var currentPath = string.Empty;

            foreach (var segment in product.PathSegments)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath} > {segment}";

                var childNode = currentNode.Children.FirstOrDefault(c => c.Name == segment);
                if (childNode == null)
                {
                    childNode = new CategoryNode
                    {
                        Name = segment,
                        Path = currentPath,
                        Parent = currentNode
                    };
                    currentNode.Children.Add(childNode);
                }

                childNode.ProductCount++;
                currentNode = childNode;
            }
        }

        return root;
    }
}

/// <summary>
/// Paged result container
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// Category tree node
/// </summary>
public class CategoryNode
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string FullPath => Path; // Alias for compatibility
    public int ProductCount { get; set; }
    public CategoryNode? Parent { get; set; }
    public List<CategoryNode> Children { get; set; } = new();
}
