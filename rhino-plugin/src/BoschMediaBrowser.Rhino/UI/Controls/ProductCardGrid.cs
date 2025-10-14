using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;

namespace BoschMediaBrowser.Rhino.UI.Controls
{
    /// <summary>
    /// Custom card-based grid layout for products
    /// Avoids Eto.Forms DataStore binding to prevent UI freezing
    /// </summary>
    public class ProductCardGrid : Scrollable
    {
        private readonly ThumbnailService _thumbnailService;
        private readonly UserDataService _userDataService;
        
        private List<Product> _allProducts = new();
        private List<ProductCard> _cards = new();
        private StackLayout _gridContainer; // Changed from TableLayout to StackLayout
        private Label _emptyStateLabel;
        private Panel _paginationPanel;
        
        private int _currentPage = 1;
        private int _itemsPerPage = 12; // Dynamic: columns x rows
        private int _totalPages = 0;
        private int _columnsPerRow = 3; // Default 3 columns (fits 3 cards nicely)
        private const int CardWidth = 204; // Product card width
        private const int CardSpacing = 16; // Spacing between cards
        private const int ContainerPadding = 40; // Left + right padding
        private const int RowsPerPage = 4;
        
        private Button _prevPageButton;
        private Button _nextPageButton;
        private Label _pageInfoLabel;
        
        private bool _multiSelectMode;
        
        // Events
        public event EventHandler<ProductCardEventArgs>? ProductSelected;
        public event EventHandler<ProductCardEventArgs>? ProductPreview;
        public event EventHandler<ProductCardEventArgs>? ProductInsert;
        
        public ProductCardGrid(
            ThumbnailService thumbnailService,
            UserDataService userDataService)
        {
            _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
            _userDataService = userDataService ?? throw new ArgumentNullException(nameof(userDataService));
            
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // Handle resize to recalculate columns
            SizeChanged += OnSizeChanged;
            
            // Main container
            var mainLayout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Padding = new Padding(0)
            };
            
            // Empty state label
            _emptyStateLabel = new Label
            {
                Text = "No products found",
                Font = new Font(SystemFont.Default, 14),
                TextColor = Color.FromArgb(128, 128, 128),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 100
            };
            
            // Grid layout container - use VERTICAL StackLayout (simpler than TableLayout)
            _gridContainer = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Padding = new Padding(20),
                Spacing = 20
            };
            
            // Pagination controls
            _paginationPanel = CreatePaginationPanel();
            
            // Add grid and pagination to main layout
            mainLayout.Items.Add(new StackLayoutItem(_gridContainer, true)); // Grid takes available space
            mainLayout.Items.Add(new StackLayoutItem(_paginationPanel, false)); // Pagination at bottom
            
            // This control is already a Scrollable, so set mainLayout as content
            Content = mainLayout;
            Border = BorderType.None;
        }
        
        private void OnSizeChanged(object? sender, EventArgs e)
        {
            // Recalculate columns per row based on available width
            var availableWidth = Width;
            if (availableWidth <= 0) return;
            
            // Calculate how many columns fit
            // 3 columns: >= 684px (3×204 + 2×16 spacing + 40 padding)
            // 2 columns: 440-683px (2×204 + 1×16 spacing + 40 padding)
            // 1 column: < 440px
            int newColumns;
            if (availableWidth >= 684)
                newColumns = 3;
            else if (availableWidth >= 440)
                newColumns = 2;
            else
                newColumns = 1;
            
            // If columns changed, recalculate layout
            if (newColumns != _columnsPerRow)
            {
                _columnsPerRow = newColumns;
                _itemsPerPage = _columnsPerRow * RowsPerPage;
                
                global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Resize detected, Width={availableWidth}, Columns={_columnsPerRow}, ItemsPerPage={_itemsPerPage}");
                
                // Re-render current page with new column count
                if (_allProducts.Any())
                {
                    _ = RenderCurrentPageAsync();
                }
            }
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
            
            return new Panel 
            { 
                Content = layout
            };
        }
        
        /// <summary>
        /// Load products into grid - fully async, non-blocking
        /// </summary>
        public async void LoadProducts(List<Product> products)
        {
            _allProducts = products ?? new List<Product>();
            _currentPage = 1;
            _totalPages = (int)Math.Ceiling((double)_allProducts.Count / _itemsPerPage);
            
            global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Loading {_allProducts.Count} products");
            
            ShowEmptyState(_allProducts.Count == 0);
            
            if (_allProducts.Count > 0)
            {
                await RenderCurrentPageAsync();
            }
            
            UpdatePaginationControls();
        }
        
        private async Task RenderCurrentPageAsync()
        {
            // Calculate visible products for current page
            var startIndex = (_currentPage - 1) * _itemsPerPage;
            var currentPageProducts = _allProducts
                .Skip(startIndex)
                .Take(_itemsPerPage)
                .ToList();
            
            global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Rendering page {_currentPage}, {currentPageProducts.Count} products");
            
            // Clear existing cards
            _cards.Clear();
            _gridContainer.Items.Clear();
            
            global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Cleared container, _gridContainer.Items.Count = {_gridContainer.Items.Count}");
            
            // Build card grid (3 columns per row using horizontal StackLayouts)
            var rows = (int)Math.Ceiling((double)currentPageProducts.Count / _columnsPerRow);
            
            global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Will create {rows} rows for {currentPageProducts.Count} products");
            
            for (int rowIdx = 0; rowIdx < rows; rowIdx++)
            {
                // Create horizontal row
                var rowLayout = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 16
                };
                
                for (int colIdx = 0; colIdx < _columnsPerRow; colIdx++)
                {
                    var productIdx = rowIdx * _columnsPerRow + colIdx;
                    
                    if (productIdx < currentPageProducts.Count)
                    {
                        var product = currentPageProducts[productIdx];
                        var card = CreateProductCard(product);
                        _cards.Add(card);
                        
                        rowLayout.Items.Add(new StackLayoutItem(card, false));
                        global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Added card for '{product.ProductName}' at row {rowIdx}, col {colIdx}");
                    }
                    else
                    {
                        // Empty spacer for alignment
                        rowLayout.Items.Add(new StackLayoutItem(new Panel { Width = 204, Height = 1 }, false));
                    }
                }
                
                _gridContainer.Items.Add(new StackLayoutItem(rowLayout, false));
                global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Added row {rowIdx}, _gridContainer.Items.Count = {_gridContainer.Items.Count}");
            }
            
            global::Rhino.RhinoApp.WriteLine($"ProductCardGrid: Render complete, {_cards.Count} cards created, {_gridContainer.Items.Count} rows in container");
            
            // Force layout update
            await Task.Run(() =>
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    _gridContainer.Invalidate();
                });
            });
        }
        
        private ProductCard CreateProductCard(Product product)
        {
            var card = new ProductCard(product, _thumbnailService, _userDataService);
            
            // Wire up events
            card.CardClicked += (s, e) =>
            {
                // Card click opens preview modal (like prototype)
                ProductPreview?.Invoke(this, e);
                
                // Also select the card for visual feedback
                foreach (var c in _cards)
                {
                    c.SetSelected(false);
                }
                card.SetSelected(true);
                ProductSelected?.Invoke(this, e);
            };
            
            card.PreviewClicked += (s, e) =>
            {
                // Preview button removed from cards, but keep event for compatibility
                ProductPreview?.Invoke(this, e);
            };
            
            card.DownloadClicked += (s, e) =>
            {
                ProductInsert?.Invoke(this, e);
            };
            
            card.FavoriteToggled += (s, e) =>
            {
                // Could refresh favorites view here if needed
            };
            
            return card;
        }
        
        private void ShowEmptyState(bool show)
        {
            if (show)
            {
                _gridContainer.Items.Clear();
                _gridContainer.Items.Add(new StackLayoutItem(_emptyStateLabel, true));
                _paginationPanel.Visible = false;
            }
            else
            {
                _paginationPanel.Visible = true;
            }
        }
        
        private async void GoToPreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await RenderCurrentPageAsync();
                UpdatePaginationControls();
            }
        }
        
        private async void GoToNextPage()
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await RenderCurrentPageAsync();
                UpdatePaginationControls();
            }
        }
        
        private void UpdatePaginationControls()
        {
            _prevPageButton.Enabled = _currentPage > 1;
            _nextPageButton.Enabled = _currentPage < _totalPages;
            _pageInfoLabel.Text = $"Page {_currentPage} of {_totalPages}";
            
            _paginationPanel.Visible = _totalPages > 1;
        }
        
        /// <summary>
        /// Get currently selected product (if any)
        /// </summary>
        public Product? GetSelectedProduct()
        {
            var selectedCard = _cards.FirstOrDefault(c => c.IsSelected);
            return selectedCard?.Product;
        }
        
        /// <summary>
        /// Clear selection
        /// </summary>
        public void ClearSelection()
        {
            foreach (var card in _cards)
            {
                card.SetSelected(false);
            }
        }
        
        /// <summary>
        /// Refresh the current page (e.g., after favorite toggle)
        /// </summary>
        public async void Refresh()
        {
            await RenderCurrentPageAsync();
        }
        
        /// <summary>
        /// Enable/disable multi-select mode (shows checkboxes on cards)
        /// </summary>
        public void SetMultiSelectMode(bool enabled)
        {
            _multiSelectMode = enabled;
            foreach (var card in _cards)
            {
                card.SetMultiSelectMode(enabled);
            }
        }
        
        /// <summary>
        /// Get all checked products (in multi-select mode)
        /// </summary>
        public List<Product> GetCheckedProducts()
        {
            return _cards
                .Where(c => c.IsChecked)
                .Select(c => c.Product)
                .ToList();
        }
    }
}
