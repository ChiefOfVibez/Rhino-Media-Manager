using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;

namespace BoschMediaBrowser.Rhino.UI.Controls;

/// <summary>
/// Grid control displaying products with thumbnails
/// Supports selection, pagination, and lazy thumbnail loading
/// </summary>
public class ThumbnailGrid : Panel
{
    private readonly ThumbnailService _thumbnailService;
    private readonly UserDataService _userDataService;
    
    private List<Product> _allProducts = new();
    private List<Product> _currentPageProducts = new();
    private readonly HashSet<string> _selectedProductIds = new();
    
    // Pagination
    private int _currentPage = 1;
    private int _itemsPerPage = 50;
    private int _totalPages = 1;

    // UI Controls
    private GridView _gridView;
    private Panel _paginationPanel;
    private Label _pageInfoLabel;
    private Button _prevPageButton;
    private Button _nextPageButton;
    private Label _emptyStateLabel;

    // Selection
    private Product? _selectedProduct;

    public event EventHandler<ProductSelectedEventArgs>? ProductSelected;
    public event EventHandler<ProductsSelectedEventArgs>? MultipleProductsSelected;

    public Product? SelectedProduct => _selectedProduct;
    public IReadOnlyCollection<string> SelectedProductIds => _selectedProductIds;

    public ThumbnailGrid(ThumbnailService thumbnailService, UserDataService userDataService)
    {
        _thumbnailService = thumbnailService;
        _userDataService = userDataService;

        InitializeControls();
        BuildLayout();
    }

    private void InitializeControls()
    {
        // GridView for products
        _gridView = new GridView
        {
            ShowHeader = true,
            AllowMultipleSelection = true,
            GridLines = GridLines.Both
        };

        // Configure columns
        _gridView.Columns.Add(new GridColumn
        {
            HeaderText = "⭐",
            DataCell = new TextBoxCell(nameof(ProductGridItem.FavouriteIcon)),
            Width = 30,
            Sortable = false
        });

        _gridView.Columns.Add(new GridColumn
        {
            HeaderText = "Preview",
            DataCell = new TextBoxCell(nameof(ProductGridItem.ThumbnailPlaceholder)),
            Width = 80,
            Sortable = false
        });

        _gridView.Columns.Add(new GridColumn
        {
            HeaderText = "Name",
            DataCell = new TextBoxCell(nameof(ProductGridItem.Name)),
            Width = 250,
            Sortable = true,
            AutoSize = true
        });

        _gridView.Columns.Add(new GridColumn
        {
            HeaderText = "SKU",
            DataCell = new TextBoxCell(nameof(ProductGridItem.SKU)),
            Width = 120,
            Sortable = true
        });

        _gridView.Columns.Add(new GridColumn
        {
            HeaderText = "Range",
            DataCell = new TextBoxCell(nameof(ProductGridItem.Range)),
            Width = 70,
            Sortable = true
        });

        _gridView.Columns.Add(new GridColumn
        {
            HeaderText = "Category",
            DataCell = new TextBoxCell(nameof(ProductGridItem.Category)),
            Width = 150,
            Sortable = true
        });

        // Selection handling
        _gridView.SelectionChanged += OnGridSelectionChanged;

        // Pagination controls
        _prevPageButton = new Button { Text = "< Previous", Enabled = false };
        _prevPageButton.Click += (s, e) => GoToPreviousPage();

        _nextPageButton = new Button { Text = "Next >", Enabled = false };
        _nextPageButton.Click += (s, e) => GoToNextPage();

        _pageInfoLabel = new Label 
        { 
            Text = "Page 0 of 0",
            VerticalAlignment = VerticalAlignment.Center
        };

        // Empty state
        _emptyStateLabel = new Label
        {
            Text = "No products found",
            Font = SystemFonts.Bold(14),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visible = false
        };
    }

    private void BuildLayout()
    {
        // Pagination panel
        _paginationPanel = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Padding = new Padding(10),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Items =
            {
                _prevPageButton,
                _pageInfoLabel,
                _nextPageButton
            }
        };

        // Main layout
        Content = new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Items =
            {
                new StackLayoutItem(_gridView, true), // Fill available space
                new StackLayoutItem(_paginationPanel, false),
                new StackLayoutItem(_emptyStateLabel, true) // Overlay when empty
            }
        };
    }

    /// <summary>
    /// Load products into the grid
    /// </summary>
    public void LoadProducts(List<Product> products)
    {
        _allProducts = products;
        _currentPage = 1;
        _totalPages = (int)Math.Ceiling((double)_allProducts.Count / _itemsPerPage);

        UpdateCurrentPage();
        
        ShowEmptyState(_allProducts.Count == 0);
    }

    /// <summary>
    /// Update the display for the current page
    /// </summary>
    private void UpdateCurrentPage()
    {
        // Calculate visible products for current page
        var startIndex = (_currentPage - 1) * _itemsPerPage;
        _currentPageProducts = _allProducts
            .Skip(startIndex)
            .Take(_itemsPerPage)
            .ToList();

        // TEMPORARILY SKIP grid DataStore update to test if it causes freeze
        global::Rhino.RhinoApp.WriteLine($"ThumbnailGrid.UpdateCurrentPage: SKIPPING DataStore update to prevent freeze");
        
        // // Create grid items
        // var gridItems = new List<ProductGridItem>();
        // var favourites = _userDataService.GetAllFavourites();
        //
        // foreach (var product in _currentPageProducts)
        // {
        //     var isFavourite = favourites.Any(f => f.ProductId == product.Id);
        //     gridItems.Add(new ProductGridItem(product, isFavourite));
        // }
        //
        // _gridView.DataStore = gridItems;
        //
        // // Start loading thumbnails asynchronously
        // LoadThumbnailsAsync(_currentPageProducts);
    }

    /// <summary>
    /// Load thumbnails for visible products
    /// </summary>
    private async void LoadThumbnailsAsync(List<Product> products)
    {
        try
        {
            // Pre-cache thumbnails for current page
            await _thumbnailService.PreCacheThumbnailsAsync(
                products,
                CancellationToken.None
            );

            // Refresh grid to show loaded thumbnails - use AsyncInvoke to prevent deadlock
            Application.Instance.AsyncInvoke(() =>
            {
                // In a real implementation, we'd update the image cells
                // For now, this placeholder indicates thumbnails are cached
            });
        }
        catch (Exception ex)
        {
            // Log error but don't crash UI
            System.Diagnostics.Debug.WriteLine($"Thumbnail loading error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle grid selection changes
    /// </summary>
    private void OnGridSelectionChanged(object? sender, EventArgs e)
    {
        _selectedProductIds.Clear();

        if (_gridView.SelectedItems != null)
        {
            foreach (var item in _gridView.SelectedItems.OfType<ProductGridItem>())
            {
                _selectedProductIds.Add(item.Product.Id);
            }
        }

        // Single selection event
        if (_gridView.SelectedItem is ProductGridItem selectedItem)
        {
            _selectedProduct = selectedItem.Product;
            ProductSelected?.Invoke(this, new ProductSelectedEventArgs(_selectedProduct));
        }
        else
        {
            _selectedProduct = null;
        }

        // Multi-selection event
        if (_selectedProductIds.Count > 1)
        {
            var selectedProducts = _currentPageProducts
                .Where(p => _selectedProductIds.Contains(p.Id))
                .ToList();
            MultipleProductsSelected?.Invoke(this, new ProductsSelectedEventArgs(selectedProducts));
        }
    }

    /// <summary>
    /// Navigate to previous page
    /// </summary>
    private void GoToPreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            UpdateCurrentPage();
            UpdatePaginationControls();
        }
    }

    /// <summary>
    /// Navigate to next page
    /// </summary>
    private void GoToNextPage()
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            UpdateCurrentPage();
            UpdatePaginationControls();
        }
    }

    /// <summary>
    /// Update pagination button states
    /// </summary>
    private void UpdatePaginationControls()
    {
        _prevPageButton.Enabled = _currentPage > 1;
        _nextPageButton.Enabled = _currentPage < _totalPages;
        _pageInfoLabel.Text = _totalPages > 0 
            ? $"Page {_currentPage} of {_totalPages} ({_allProducts.Count} products)"
            : "No products";
    }

    /// <summary>
    /// Show or hide empty state
    /// </summary>
    private void ShowEmptyState(bool show)
    {
        _emptyStateLabel.Visible = show;
        _gridView.Visible = !show;
        _paginationPanel.Visible = !show;
    }

    /// <summary>
    /// Clear selection
    /// </summary>
    public void ClearSelection()
    {
        _gridView.UnselectAll();
        _selectedProductIds.Clear();
        _selectedProduct = null;
    }

    /// <summary>
    /// Refresh grid (e.g., after favourite/tag changes)
    /// </summary>
    public void Refresh()
    {
        UpdateCurrentPage();
    }

    /// <summary>
    /// Set items per page
    /// </summary>
    public void SetItemsPerPage(int itemsPerPage)
    {
        _itemsPerPage = Math.Max(10, Math.Min(200, itemsPerPage));
        _totalPages = (int)Math.Ceiling((double)_allProducts.Count / _itemsPerPage);
        _currentPage = 1;
        UpdateCurrentPage();
        UpdatePaginationControls();
    }
}

/// <summary>
/// Grid item representing a product row
/// </summary>
public class ProductGridItem
{
    public Product Product { get; }

    public ProductGridItem(Product product, bool isFavourite)
    {
        Product = product;
        FavouriteIcon = isFavourite ? "★" : "☆";
        ThumbnailPlaceholder = "🖼️"; // Placeholder until images load
        Name = product.ProductName;
        SKU = product.Sku ?? "";
        Range = product.Range ?? "";
        Category = product.Category ?? "";
    }

    public string FavouriteIcon { get; }
    public string ThumbnailPlaceholder { get; }
    public string Name { get; }
    public string SKU { get; }
    public string Range { get; }
    public string Category { get; }
}

/// <summary>
/// Event args for single product selection
/// </summary>
public class ProductSelectedEventArgs : EventArgs
{
    public Product Product { get; }

    public ProductSelectedEventArgs(Product product)
    {
        Product = product;
    }
}

/// <summary>
/// Event args for multiple product selection
/// </summary>
public class ProductsSelectedEventArgs : EventArgs
{
    public List<Product> Products { get; }

    public ProductsSelectedEventArgs(List<Product> products)
    {
        Products = products;
    }
}
