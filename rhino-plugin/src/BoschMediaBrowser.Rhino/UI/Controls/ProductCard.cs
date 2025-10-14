using System;
using Eto.Drawing;
using Eto.Forms;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;

namespace BoschMediaBrowser.Rhino.UI.Controls
{
    /// <summary>
    /// Individual product card panel with thumbnail, name, SKU, and action buttons
    /// Matches prototype design from screenshots
    /// </summary>
    public class ProductCard : Panel
    {
        private readonly Product _product;
        private readonly ThumbnailService _thumbnailService;
        private readonly UserDataService _userDataService;
        
        private ImageView _thumbnailImage;
        private Label _nameLabel;
        private Label _skuLabel;
        private Panel _actionButtonsPanel;
        private Button _previewButton;
        private Button _downloadButton;
        private Button _favoriteButton;
        private Label _newBadge;
        private CheckBox _selectCheckbox;
        
        private bool _isFavorite;
        private bool _multiSelectMode;
        
        // Events
        public event EventHandler<ProductCardEventArgs>? CardClicked;
        public event EventHandler<ProductCardEventArgs>? PreviewClicked;
        public event EventHandler<ProductCardEventArgs>? DownloadClicked;
        public event EventHandler<ProductCardEventArgs>? FavoriteToggled;
        
        public Product Product => _product;
        public bool IsSelected { get; private set; }
        public bool IsChecked => _selectCheckbox?.Checked ?? false;
        
        public void SetMultiSelectMode(bool enabled)
        {
            _multiSelectMode = enabled;
            if (_selectCheckbox != null)
            {
                _selectCheckbox.Visible = enabled;
            }
        }
        
        public ProductCard(
            Product product, 
            ThumbnailService thumbnailService,
            UserDataService userDataService)
        {
            _product = product;
            _thumbnailService = thumbnailService;
            _userDataService = userDataService;
            
            _isFavorite = _userDataService.IsFavourite(product.Id);
            
            global::Rhino.RhinoApp.WriteLine($"ProductCard: Creating card for '{product.ProductName}'");
            
            BuildUI();
            LoadThumbnailAsync();
        }
        
        private void BuildUI()
        {
            // Card container with border and padding
            var cardContainer = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Padding = new Padding(12),
                Spacing = 8,
                BackgroundColor = Colors.White
            };
            
            // Thumbnail container (fixed size)
            var thumbnailContainer = new Panel
            {
                Width = 180,
                Height = 180,
                BackgroundColor = Color.FromArgb(250, 250, 250) // Very light gray placeholder
            };
            
            _thumbnailImage = new ImageView
            {
                Width = 180,
                Height = 180
            };
            
            thumbnailContainer.Content = _thumbnailImage;
            
            // Action buttons overlay (hidden by default, shown on hover)
            _actionButtonsPanel = CreateActionButtonsPanel();
            _actionButtonsPanel.Visible = false;
            
            // Thumbnail + buttons stack (overlay buttons in top-right corner)
            var thumbnailStack = new PixelLayout();
            thumbnailStack.Add(thumbnailContainer, 0, 0);
            thumbnailStack.Add(_actionButtonsPanel, 100, 5); // Position in top-right corner
            
            // Product name
            _nameLabel = new Label
            {
                Text = _product.ProductName,
                Font = new Font(SystemFont.Bold, 12),
                TextColor = Color.FromArgb(26, 29, 33), // Dark gray
                Wrap = WrapMode.Word,
                Height = 36
            };
            
            // SKU label
            _skuLabel = new Label
            {
                Text = _product.Sku ?? "",
                Font = new Font(SystemFont.Default, 10),
                TextColor = Color.FromArgb(107, 114, 128), // Medium gray
            };
            
            // "New" badge (optional)
            _newBadge = new Label
            {
                Text = " new ",
                Font = new Font(SystemFont.Bold, 10),
                BackgroundColor = Color.FromArgb(37, 99, 235), // Blue (darker)
                TextColor = Colors.White,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Height = 24,
                Visible = IsNewProduct()
            };
            
            // Multi-select checkbox (hidden by default)
            _selectCheckbox = new CheckBox
            {
                Visible = false,
                ToolTip = "Select for batch operations"
            };
            
            // Add all elements to card
            cardContainer.Items.Add(new StackLayoutItem(_selectCheckbox, false));
            cardContainer.Items.Add(new StackLayoutItem(thumbnailStack, false));
            
            if (_newBadge.Visible)
            {
                cardContainer.Items.Add(new StackLayoutItem(_newBadge, false));
            }
            
            cardContainer.Items.Add(new StackLayoutItem(_nameLabel, false));
            cardContainer.Items.Add(new StackLayoutItem(_skuLabel, false));
            
            // Main card panel with fixed size
            var cardPanel = new Panel
            {
                Width = 204,
                Height = 280,
                BackgroundColor = Colors.White,
                Content = cardContainer
            };
            
            // Wrap in drawable for border
            var drawable = new Drawable
            {
                Content = cardPanel
            };
            
            drawable.Paint += (sender, e) =>
            {
                var borderColor = IsSelected 
                    ? Color.FromArgb(37, 99, 235) // Blue when selected
                    : Color.FromArgb(229, 231, 235); // Light gray border
                    
                var borderWidth = IsSelected ? 2f : 1f;
                
                // Draw border
                e.Graphics.DrawRectangle(borderColor, 0, 0, drawable.Width - borderWidth, drawable.Height - borderWidth);
                
                // Draw subtle shadow when selected
                if (IsSelected)
                {
                    e.Graphics.FillRectangle(Color.FromArgb(10, 0, 0, 0), 2, 2, drawable.Width - 4, drawable.Height - 4);
                }
            };
            
            // Card click handler
            drawable.MouseDown += (s, e) =>
            {
                CardClicked?.Invoke(this, new ProductCardEventArgs(_product));
            };
            
            // Hover effect - show action buttons
            drawable.MouseEnter += (s, e) =>
            {
                _actionButtonsPanel.Visible = true;
            };
            
            drawable.MouseLeave += (s, e) =>
            {
                _actionButtonsPanel.Visible = false;
            };
            
            Content = drawable;
        }
        
        private Panel CreateActionButtonsPanel()
        {
            // Action buttons overlay (top-right corner)
            var buttonsLayout = new StackLayout
            {
                Spacing = 4,
                Padding = new Padding(8),
                BackgroundColor = Color.FromArgb(200, 0, 0, 0) // Semi-transparent black
            };
            
            // Download/Insert button
            _downloadButton = new Button
            {
                Text = "⬇",
                Width = 32,
                Height = 32,
                ToolTip = "Insert"
            };
            _downloadButton.Click += (s, e) =>
            {
                DownloadClicked?.Invoke(this, new ProductCardEventArgs(_product));
            };
            
            // Favorite button (heart icon)
            _favoriteButton = new Button
            {
                Text = _isFavorite ? "❤" : "♡",
                Width = 32,
                Height = 32,
                ToolTip = "Favorite"
            };
            _favoriteButton.Click += (s, e) =>
            {
                ToggleFavorite();
            };
            
            // Preview button removed - card click opens preview modal instead
            buttonsLayout.Items.Add(_downloadButton);
            buttonsLayout.Items.Add(_favoriteButton);
            
            var panel = new Panel
            {
                Width = 80,  // Wide enough for 2 buttons
                Height = 40,
                Content = buttonsLayout
            };
            
            return panel;
        }
        
        private async void LoadThumbnailAsync()
        {
            try
            {
                // First try to use existing preview images from JSON
                string? previewPath = _product.Previews?.MeshPreview?.FullPath 
                                   ?? _product.Previews?.GraficaPreview?.FullPath;
                
                if (!string.IsNullOrEmpty(previewPath) && System.IO.File.Exists(previewPath))
                {
                    // Use existing preview image directly
                    Application.Instance.AsyncInvoke(() =>
                    {
                        try
                        {
                            _thumbnailImage.Image = new Bitmap(previewPath);
                        }
                        catch
                        {
                            // Failed to load, try thumbnail service
                            LoadFromThumbnailService();
                        }
                    });
                }
                else
                {
                    // No preview available, use thumbnail service
                    LoadFromThumbnailService();
                }
                
                async void LoadFromThumbnailService()
                {
                    var thumbnailPath = await _thumbnailService.GetThumbnailPathAsync(
                        _product.Id,
                        ThumbnailSize.Small,
                        System.Threading.CancellationToken.None
                    );
                    
                    if (!string.IsNullOrEmpty(thumbnailPath) && System.IO.File.Exists(thumbnailPath))
                    {
                        Application.Instance.AsyncInvoke(() =>
                        {
                            try
                            {
                                _thumbnailImage.Image = new Bitmap(thumbnailPath);
                            }
                            catch { }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash UI
                System.Diagnostics.Debug.WriteLine($"Error loading thumbnail for {_product.Id}: {ex.Message}");
            }
        }
        
        private bool IsNewProduct()
        {
            // Product is "new" if created within last 30 days
            if (_product.Metadata?.CreatedDate != null)
            {
                var daysSinceCreation = (DateTime.Now - _product.Metadata.CreatedDate.Value).TotalDays;
                return daysSinceCreation <= 30;
            }
            return false;
        }
        
        private async void ToggleFavorite()
        {
            _isFavorite = !_isFavorite;
            _favoriteButton.Text = _isFavorite ? "❤" : "♡";
            
            try
            {
                if (_isFavorite)
                {
                    await _userDataService.AddFavouriteAsync(_product.Id);
                }
                else
                {
                    _userDataService.RemoveFavourite(_product.Id);
                }
                
                FavoriteToggled?.Invoke(this, new ProductCardEventArgs(_product));
            }
            catch (Exception ex)
            {
                global::Rhino.RhinoApp.WriteLine($"ProductCard: Error toggling favorite: {ex.Message}");
                // Revert on error
                _isFavorite = !_isFavorite;
                _favoriteButton.Text = _isFavorite ? "❤" : "♡";
            }
        }
        
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            Invalidate(); // Trigger repaint to show selection border
        }
    }
    
    /// <summary>
    /// Event args for product card events
    /// </summary>
    public class ProductCardEventArgs : EventArgs
    {
        public Product Product { get; }
        public string? SelectedHolderVariant { get; set; }
        public bool IncludePackaging { get; set; }
        
        public ProductCardEventArgs(Product product, string? selectedHolderVariant = null, bool includePackaging = false)
        {
            Product = product;
            SelectedHolderVariant = selectedHolderVariant;
            IncludePackaging = includePackaging;
        }
    }
}
