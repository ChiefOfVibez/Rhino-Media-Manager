using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoschMediaBrowser.Rhino.UI.Controls;

/// <summary>
/// Displays products in a list view (horizontal rows)
/// Alternative to ProductCardGrid
/// </summary>
public class ProductListView : Panel
{
    private readonly ThumbnailService _thumbnailService;
    private readonly UserDataService _userDataService;
    
    private List<Product> _allProducts = new();
    private StackLayout _listContainer;
    private Label _emptyStateLabel;
    private Panel _paginationPanel;
    private Button _prevPageButton;
    private Button _nextPageButton;
    private Label _pageInfoLabel;
    
    private int _currentPage = 1;
    private const int ITEMS_PER_PAGE = 20;
    
    public bool MultiSelectMode { get; set; }
    
    public event EventHandler<ProductCardEventArgs>? ProductSelected;
    public event EventHandler<ProductCardEventArgs>? ProductPreview;
    public event EventHandler<ProductCardEventArgs>? ProductInsert;
    
    public ProductListView(ThumbnailService thumbnailService, UserDataService userDataService)
    {
        _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
        _userDataService = userDataService ?? throw new ArgumentNullException(nameof(userDataService));
        
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        var mainLayout = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Padding = new Padding(0)
        };
        
        _emptyStateLabel = new Label
        {
            Text = "No products found",
            Font = new Font(SystemFont.Default, 14),
            TextColor = Color.FromArgb(128, 128, 128),
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visible = false
        };
        
        _listContainer = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Padding = new Padding(10),
            Spacing = 0 // No spacing - items have their own borders
        };
        
        _paginationPanel = CreatePaginationPanel();
        
        // Create scrollable wrapper for list container
        var scrollable = new Scrollable
        {
            Content = _listContainer,
            Border = BorderType.None,
            ExpandContentHeight = false,
            ExpandContentWidth = true
        };
        
        mainLayout.Items.Add(new StackLayoutItem(scrollable, true));
        mainLayout.Items.Add(new StackLayoutItem(_paginationPanel, false));
        
        Content = mainLayout;
    }
    
    private Panel CreatePaginationPanel()
    {
        var layout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Padding = new Padding(0, 16)
        };
        
        _prevPageButton = new Button
        {
            Text = "< Previous",
            Enabled = false,
            Width = 100
        };
        _prevPageButton.Click += (s, e) => GoToPreviousPage();
        
        _pageInfoLabel = new Label
        {
            Text = "Page 1 of 1",
            VerticalAlignment = VerticalAlignment.Center,
            Width = 120,
            TextAlignment = TextAlignment.Center
        };
        
        _nextPageButton = new Button
        {
            Text = "Next >",
            Enabled = false,
            Width = 100
        };
        _nextPageButton.Click += (s, e) => GoToNextPage();
        
        layout.Items.Add(_prevPageButton);
        layout.Items.Add(_pageInfoLabel);
        layout.Items.Add(_nextPageButton);
        
        return new Panel { Content = layout };
    }
    
    public async void LoadProducts(List<Product> products)
    {
        _allProducts = products ?? new List<Product>();
        _currentPage = 1;
        await RenderPage();
    }
    
    private async System.Threading.Tasks.Task RenderPage()
    {
        _listContainer.Items.Clear();
        
        if (_allProducts.Count == 0)
        {
            _emptyStateLabel.Visible = true;
            _listContainer.Visible = false;
            UpdatePagination();
            return;
        }
        
        _emptyStateLabel.Visible = false;
        _listContainer.Visible = true;
        
        var totalPages = (int)Math.Ceiling((double)_allProducts.Count / ITEMS_PER_PAGE);
        var skip = (_currentPage - 1) * ITEMS_PER_PAGE;
        var pageProducts = _allProducts.Skip(skip).Take(ITEMS_PER_PAGE).ToList();
        
        foreach (var product in pageProducts)
        {
            var listItem = CreateListItem(product);
            _listContainer.Items.Add(listItem);
        }
        
        UpdatePagination();
    }
    
    private Control CreateListItem(Product product)
    {
        var isFavorite = _userDataService.IsFavourite(product.Id);
        
        // Container for the entire row
        var rowPanel = new Panel
        {
            Padding = new Padding(10, 8),
            BackgroundColor = Colors.White
        };
        
        // Thumbnail
        var thumbnail = new ImageView
        {
            Size = new Size(80, 80)
        };
        
        // Load thumbnail async
        _ = LoadThumbnailAsync(product, thumbnail);
        
        // Product info
        var nameLabel = new Label
        {
            Text = product.ProductName,
            Font = new Font(SystemFont.Bold, 11),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var skuLabel = new Label
        {
            Text = product.Sku ?? "",
            Font = new Font(SystemFont.Default, 9),
            TextColor = Color.FromArgb(120, 120, 120),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Tags (if any)
        var tagsLabel = new Label
        {
            Text = product.Tags != null && product.Tags.Any() 
                ? string.Join(", ", product.Tags.Select(t => $"#{t}")) 
                : "",
            Font = new Font(SystemFont.Default, 9),
            TextColor = Color.FromArgb(100, 100, 200),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Holder variant dropdown
        var holderDropdown = new DropDown
        {
            Width = 140
        };
        holderDropdown.Items.Add("No Holder");
        // Store full "Variant_Color" keys to uniquely identify holders
        var holderKeys = new List<string> { "" }; // Empty string for "No Holder"
        if (product.Holders != null && product.Holders.Any())
        {
            foreach (var holder in product.Holders)
            {
                holderDropdown.Items.Add($"{holder.Variant} - {holder.Color}");
                holderKeys.Add($"{holder.Variant}_{holder.Color}"); // Unique key
            }
        }
        // Default to first holder if available (usually Tego)
        holderDropdown.SelectedIndex = holderKeys.Count > 1 ? 1 : 0;
        
        // Packaging checkbox
        var packagingCheckbox = new CheckBox
        {
            Text = "Pkg",
            ToolTip = "Include Packaging",
            Checked = false,
            Enabled = product.Packaging != null && !string.IsNullOrEmpty(product.Packaging.PreviewPath)
        };
        
        // Action buttons
        var insertButton = new Button
        {
            Text = "Insert",
            Width = 70,
            Height = 28
        };
        insertButton.Click += (s, e) =>
        {
            var selectedIndex = holderDropdown.SelectedIndex;
            var holderKey = selectedIndex > 0 && selectedIndex < holderKeys.Count 
                ? holderKeys[selectedIndex] 
                : null;
            var includePackaging = packagingCheckbox.Checked == true;
            
            ProductInsert?.Invoke(this, new ProductCardEventArgs(product, holderKey, includePackaging));
        };
        
        var favoriteButton = new Button
        {
            Text = isFavorite ? "❤" : "♡",
            Width = 40,
            Height = 28,
            ToolTip = "Favorite"
        };
        favoriteButton.Click += async (s, e) =>
        {
            if (isFavorite)
            {
                _userDataService.RemoveFavourite(product.Id);
                favoriteButton.Text = "♡";
            }
            else
            {
                await _userDataService.AddFavouriteAsync(product.Id);
                favoriteButton.Text = "❤";
            }
            isFavorite = !isFavorite;
        };
        
        // Checkbox for multi-select
        var checkbox = new CheckBox
        {
            Visible = MultiSelectMode
        };
        
        // Info stack (vertically centered)
        var infoStack = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            VerticalContentAlignment = VerticalAlignment.Center,
            Items = { nameLabel, skuLabel, tagsLabel }
        };
        
        // Create a holder + packaging container
        var holderPackagingLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalContentAlignment = VerticalAlignment.Center,
            Items = { holderDropdown, packagingCheckbox }
        };
        
        // Create action buttons container with icons
        var actionsLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalContentAlignment = VerticalAlignment.Center,
            Items = { insertButton, favoriteButton }
        };
        
        // Layout: [Checkbox] [Thumbnail] [Info] [Holder+Pkg] [Actions]
        var layout = new TableLayout
        {
            Spacing = new Size(12, 0),
            Rows =
            {
                new TableRow(
                    new TableCell(checkbox, false),
                    new TableCell(thumbnail, false),
                    new TableCell(infoStack, true), // Takes remaining space
                    new TableCell(holderPackagingLayout, false),
                    new TableCell(actionsLayout, false)
                )
            }
        };
        
        rowPanel.Content = layout;
        
        // Click to preview
        rowPanel.MouseUp += (s, e) =>
        {
            if (e.Buttons == MouseButtons.Primary)
            {
                ProductPreview?.Invoke(this, new ProductCardEventArgs(product));
            }
        };
        
        // Separator line
        var separator = new Panel
        {
            Height = 1,
            BackgroundColor = Color.FromArgb(230, 230, 230)
        };
        
        var itemContainer = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Items = { rowPanel, separator }
        };
        
        return itemContainer;
    }
    
    private async System.Threading.Tasks.Task LoadThumbnailAsync(Product product, ImageView imageView)
    {
        try
        {
            // First try to use existing preview images from JSON
            string? previewPath = product.Previews?.MeshPreview?.FullPath 
                               ?? product.Previews?.GraficaPreview?.FullPath;
            
            if (!string.IsNullOrEmpty(previewPath) && System.IO.File.Exists(previewPath))
            {
                // Use existing preview image directly
                Application.Instance.AsyncInvoke(() =>
                {
                    try
                    {
                        imageView.Image = new Bitmap(previewPath);
                    }
                    catch
                    {
                        // Failed to load, fallback to thumbnail service
                        _ = LoadFromThumbnailService();
                    }
                });
            }
            else
            {
                await LoadFromThumbnailService();
            }
            
            async System.Threading.Tasks.Task LoadFromThumbnailService()
            {
                var thumbnailPath = await _thumbnailService.GetThumbnailPathAsync(
                    product.Id,
                    ThumbnailSize.Small,
                    System.Threading.CancellationToken.None
                );
                
                if (!string.IsNullOrEmpty(thumbnailPath) && System.IO.File.Exists(thumbnailPath))
                {
                    Application.Instance.AsyncInvoke(() =>
                    {
                        try
                        {
                            imageView.Image = new Bitmap(thumbnailPath);
                        }
                        catch { }
                    });
                }
            }
        }
        catch
        {
            // Thumbnail loading failed, keep placeholder
        }
    }
    
    private void GoToPreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            _ = RenderPage();
        }
    }
    
    private void GoToNextPage()
    {
        var totalPages = (int)Math.Ceiling((double)_allProducts.Count / ITEMS_PER_PAGE);
        if (_currentPage < totalPages)
        {
            _currentPage++;
            _ = RenderPage();
        }
    }
    
    private void UpdatePagination()
    {
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)_allProducts.Count / ITEMS_PER_PAGE));
        
        _pageInfoLabel.Text = $"Page {_currentPage} of {totalPages}";
        _prevPageButton.Enabled = _currentPage > 1;
        _nextPageButton.Enabled = _currentPage < totalPages;
    }
}
