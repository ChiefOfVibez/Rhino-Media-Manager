using Eto.Forms;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;

namespace BoschMediaBrowser.Rhino.UI.Controls;

/// <summary>
/// Hierarchical tree control for browsing product categories
/// </summary>
public class CategoryTree : TreeGridView
{
    private readonly SearchService _searchService;
    private List<Product> _products = new();

    public event EventHandler<CategorySelectedEventArgs>? CategorySelected;

    public CategoryTree(SearchService searchService)
    {
        _searchService = searchService;
        
        // Configure tree appearance
        Border = BorderType.Line;
        ShowHeader = false;
        
        // Add single column for category names
        Columns.Add(new GridColumn
        {
            DataCell = new TextBoxCell(0),
            HeaderText = "Category",
            Editable = false,
            AutoSize = true
        });

        // Handle selection
        SelectionChanged += OnSelectionChanged;
    }

    /// <summary>
    /// Load products and build category tree
    /// </summary>
    public void LoadProducts(List<Product> products)
    {
        _products = products;
        
        // TEMPORARILY SKIP BuildTree to test if DataStore assignment causes freeze
        global::Rhino.RhinoApp.WriteLine($"CategoryTree.LoadProducts: SKIPPING BuildTree() to prevent freeze");
        // BuildTree();
    }

    /// <summary>
    /// Build the category tree from products
    /// </summary>
    private void BuildTree()
    {
        DataStore = null; // Clear existing

        var rootNodes = new List<TreeGridItem>();

        // "All Products" node
        var allProductsNode = new TreeGridItem
        {
            Values = new object[] { $"All Products ({_products.Count})" },
            Tag = new CategoryNode { Name = "All", Path = "", ProductCount = _products.Count }
        };
        rootNodes.Add(allProductsNode);

        // Build category tree from SearchService
        var categoryTreeRoot = _searchService.BuildCategoryTree(_products);

        // Convert to TreeGridItems
        foreach (var categoryNode in categoryTreeRoot.Children)
        {
            var treeItem = BuildTreeItem(categoryNode);
            rootNodes.Add(treeItem);
        }

        DataStore = new TreeGridItemCollection(rootNodes);

        // Expand top-level nodes by default
        foreach (var node in rootNodes)
        {
            if (node != allProductsNode)
            {
                node.Expanded = true;
            }
        }
    }

    /// <summary>
    /// Recursively build tree items from category nodes
    /// </summary>
    private TreeGridItem BuildTreeItem(CategoryNode categoryNode)
    {
        var item = new TreeGridItem
        {
            Values = new object[] { $"{categoryNode.Name} ({categoryNode.ProductCount})" },
            Tag = categoryNode
        };

        // Add children recursively
        foreach (var child in categoryNode.Children)
        {
            item.Children.Add(BuildTreeItem(child));
        }

        // Expand if it has few children (better UX)
        item.Expanded = categoryNode.Children.Count <= 3;

        return item;
    }

    /// <summary>
    /// Handle category selection
    /// </summary>
    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        if (SelectedItem is TreeGridItem item && item.Tag is CategoryNode categoryNode)
        {
            CategorySelected?.Invoke(this, new CategorySelectedEventArgs(categoryNode));
        }
    }

    /// <summary>
    /// Expand all nodes
    /// </summary>
    public void ExpandAll()
    {
        if (DataStore is TreeGridItemCollection items)
        {
            foreach (TreeGridItem item in items)
            {
                ExpandRecursive(item);
            }
        }
    }

    /// <summary>
    /// Collapse all nodes
    /// </summary>
    public void CollapseAll()
    {
        if (DataStore is TreeGridItemCollection items)
        {
            foreach (TreeGridItem item in items)
            {
                CollapseRecursive(item);
            }
        }
    }

    private void ExpandRecursive(TreeGridItem item)
    {
        item.Expanded = true;
        foreach (TreeGridItem child in item.Children)
        {
            ExpandRecursive(child);
        }
    }

    private void CollapseRecursive(TreeGridItem item)
    {
        item.Expanded = false;
        foreach (TreeGridItem child in item.Children)
        {
            CollapseRecursive(child);
        }
    }
}

/// <summary>
/// Event args for category selection
/// </summary>
public class CategorySelectedEventArgs : EventArgs
{
    public CategoryNode Category { get; }

    public CategorySelectedEventArgs(CategoryNode category)
    {
        Category = category;
    }
}
