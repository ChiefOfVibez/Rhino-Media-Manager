using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using System.Diagnostics;

namespace BoschMediaBrowser.Rhino.UI.Controls;

/// <summary>
/// Detail pane showing selected product information and actions
/// </summary>
public class DetailPane : Panel
{
    private readonly UserDataService _userDataService;
    private readonly SettingsService _settingsService;
    private readonly ThumbnailService _thumbnailService;

    private Product? _currentProduct;

    // UI Controls
    private ImageView _previewImage;
    private Label _productNameLabel;
    private Label _skuLabel;
    private Label _descriptionLabel;
    private Label _categoryLabel;
    private Label _rangeLabel;
    private Button _favouriteButton;
    private Button _openFolderButton;
    private Button _addTagButton;
    private Button _addToCollectionButton;
    private DropDown _holderVariantDropDown;
    private DropDown _holderColorDropDown;
    private CheckBox _noHolderCheckBox;
    private TabControl _previewTabs;
    private ListBox _tagsListBox;
    private Label _emptyStateLabel;

    public event EventHandler<ProductActionEventArgs>? FavouriteToggled;
    public event EventHandler<ProductActionEventArgs>? TagRequested;
    public event EventHandler<ProductActionEventArgs>? CollectionRequested;
    public event EventHandler<HolderSelectionEventArgs>? HolderSelectionChanged;

    public DetailPane(UserDataService userDataService, SettingsService settingsService, ThumbnailService thumbnailService)
    {
        _userDataService = userDataService;
        _settingsService = settingsService;
        _thumbnailService = thumbnailService;

        InitializeControls();
        BuildLayout();
        ShowEmptyState();
    }

    private void InitializeControls()
    {
        // Preview image
        _previewImage = new ImageView
        {
            Size = new Size(300, 300),
            Image = null // Will load thumbnail
        };

        // Product info labels
        _productNameLabel = new Label
        {
            Font = SystemFonts.Bold(16),
            Wrap = WrapMode.Word
        };

        _skuLabel = new Label
        {
            Font = SystemFonts.Default(10),
            TextColor = Colors.Gray
        };

        _descriptionLabel = new Label
        {
            Wrap = WrapMode.Word,
            Height = 80
        };

        _categoryLabel = new Label();
        _rangeLabel = new Label();

        // Action buttons
        _favouriteButton = new Button
        {
            Text = "☆ Add to Favourites",
            Width = 150
        };
        _favouriteButton.Click += OnFavouriteClicked;

        _openFolderButton = new Button
        {
            Text = "📁 Open Folder",
            Width = 150
        };
        _openFolderButton.Click += OnOpenFolderClicked;

        _addTagButton = new Button
        {
            Text = "🏷️ Add Tag",
            Width = 150
        };
        _addTagButton.Click += OnAddTagClicked;

        _addToCollectionButton = new Button
        {
            Text = "➕ Add to Collection",
            Width = 150
        };
        _addToCollectionButton.Click += OnAddToCollectionClicked;

        // Holder selection
        _holderVariantDropDown = new DropDown { Width = 200 };
        _holderVariantDropDown.SelectedIndexChanged += OnHolderSelectionChanged;

        _holderColorDropDown = new DropDown { Width = 200, Enabled = false };
        _holderColorDropDown.SelectedIndexChanged += OnHolderSelectionChanged;

        _noHolderCheckBox = new CheckBox { Text = "No holder (product only)" };
        _noHolderCheckBox.CheckedChanged += OnHolderSelectionChanged;

        // Preview tabs
        _previewTabs = new TabControl();
        _previewTabs.Pages.Add(new TabPage { Text = "Mesh Preview", Content = CreatePreviewTabContent("Mesh") });
        _previewTabs.Pages.Add(new TabPage { Text = "Grafica Preview", Content = CreatePreviewTabContent("Grafica") });
        _previewTabs.Pages.Add(new TabPage { Text = "Packaging Preview", Content = CreatePreviewTabContent("Packaging") });

        // Tags list
        _tagsListBox = new ListBox { Height = 80 };

        // Empty state
        _emptyStateLabel = new Label
        {
            Text = "Select a product to view details",
            Font = SystemFonts.Bold(14),
            TextColor = Colors.Gray,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private Control CreatePreviewTabContent(string previewType)
    {
        return new Label
        {
            Text = $"{previewType} preview will load here",
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    private void BuildLayout()
    {
        var scrollable = new Scrollable
        {
            Content = new StackLayout
            {
                Padding = new Padding(10),
                Spacing = 10,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    // Preview image
                    new StackLayoutItem(_previewImage, HorizontalAlignment.Center),

                    // Product info
                    new GroupBox
                    {
                        Text = "Product Information",
                        Content = new StackLayout
                        {
                            Spacing = 5,
                            Padding = new Padding(10),
                            Items =
                            {
                                _productNameLabel,
                                _skuLabel,
                                new Label { Text = " " }, // Spacer
                                _descriptionLabel,
                                new Label { Text = " " }, // Spacer
                                new StackLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Spacing = 10,
                                    Items =
                                    {
                                        new Label { Text = "Category:" },
                                        _categoryLabel
                                    }
                                },
                                new StackLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Spacing = 10,
                                    Items =
                                    {
                                        new Label { Text = "Range:" },
                                        _rangeLabel
                                    }
                                }
                            }
                        }
                    },

                    // Actions
                    new GroupBox
                    {
                        Text = "Actions",
                        Content = new StackLayout
                        {
                            Spacing = 5,
                            Padding = new Padding(10),
                            Items =
                            {
                                _favouriteButton,
                                _addTagButton,
                                _addToCollectionButton,
                                _openFolderButton
                            }
                        }
                    },

                    // Holder selection
                    new GroupBox
                    {
                        Text = "Holder Selection",
                        Content = new StackLayout
                        {
                            Spacing = 5,
                            Padding = new Padding(10),
                            Items =
                            {
                                new Label { Text = "Holder Variant:" },
                                _holderVariantDropDown,
                                new Label { Text = "Color:" },
                                _holderColorDropDown,
                                _noHolderCheckBox
                            }
                        }
                    },

                    // Tags
                    new GroupBox
                    {
                        Text = "Tags",
                        Content = new StackLayout
                        {
                            Spacing = 5,
                            Padding = new Padding(10),
                            Items =
                            {
                                _tagsListBox
                            }
                        }
                    },

                    // Preview tabs
                    new GroupBox
                    {
                        Text = "Previews",
                        Content = _previewTabs,
                        Height = 250
                    }
                }
            }
        };

        Content = scrollable;
    }

    /// <summary>
    /// Load and display product details
    /// </summary>
    public async void LoadProduct(Product product)
    {
        _currentProduct = product;
        ShowEmptyState(false);

        // Update product info
        _productNameLabel.Text = product.ProductName;
        _skuLabel.Text = $"SKU: {product.Sku ?? "N/A"}";
        _descriptionLabel.Text = product.Description ?? "No description available";
        _categoryLabel.Text = product.Category ?? "Uncategorized";
        _rangeLabel.Text = product.Range ?? "Unknown";

        // Update favourite button
        var isFavourite = _userDataService.GetAllFavourites().Any(f => f.ProductId == product.Id);
        _favouriteButton.Text = isFavourite ? "★ Remove from Favourites" : "☆ Add to Favourites";

        // Load holders
        LoadHolders(product);

        // Load tags
        LoadTags(product);

        // Load preview image
        await LoadPreviewImageAsync(product);
    }

    /// <summary>
    /// Load holder options
    /// </summary>
    private void LoadHolders(Product product)
    {
        _holderVariantDropDown.Items.Clear();
        _holderColorDropDown.Items.Clear();
        _holderColorDropDown.Enabled = false;

        if (product.Holders == null || product.Holders.Count == 0)
        {
            _holderVariantDropDown.Items.Add("No holders available");
            _holderVariantDropDown.SelectedIndex = 0;
            _holderVariantDropDown.Enabled = false;
            _noHolderCheckBox.Checked = true;
            return;
        }

        _holderVariantDropDown.Enabled = true;
        _holderVariantDropDown.Items.Add("Select holder...");
        
        foreach (var holder in product.Holders)
        {
            _holderVariantDropDown.Items.Add(holder.Variant ?? "Unknown");
        }

        _holderVariantDropDown.SelectedIndex = 0;
    }

    /// <summary>
    /// Load colors for selected holder
    /// </summary>
    private void LoadColorsForHolder(Holder holder)
    {
        _holderColorDropDown.Items.Clear();
        
        if (string.IsNullOrEmpty(holder.Color))
        {
            _holderColorDropDown.Items.Add("No color specified");
            _holderColorDropDown.SelectedIndex = 0;
            _holderColorDropDown.Enabled = false;
            return;
        }

        _holderColorDropDown.Enabled = true;
        _holderColorDropDown.Items.Add(holder.Color);
        _holderColorDropDown.SelectedIndex = 0;
    }

    /// <summary>
    /// Load product tags
    /// </summary>
    private void LoadTags(Product product)
    {
        _tagsListBox.Items.Clear();
        
        var tags = _userDataService.GetTagsForProduct(product.Id);
        if (tags.Count == 0)
        {
            _tagsListBox.Items.Add("(No tags)");
        }
        else
        {
            foreach (var tag in tags)
            {
                _tagsListBox.Items.Add(tag);
            }
        }
    }

    /// <summary>
    /// Load preview image asynchronously
    /// </summary>
    private async Task LoadPreviewImageAsync(Product product)
    {
        try
        {
            var thumbnailPath = _thumbnailService.GetThumbnailPath(
                product,
                PreviewType.Mesh
            );

            if (!string.IsNullOrEmpty(thumbnailPath) && File.Exists(thumbnailPath))
            {
                _previewImage.Image = new Bitmap(thumbnailPath);
            }
            else
            {
                _previewImage.Image = null; // Show placeholder
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Preview load error: {ex.Message}");
            _previewImage.Image = null;
        }
    }

    /// <summary>
    /// Handle favourite button click
    /// </summary>
    private void OnFavouriteClicked(object? sender, EventArgs e)
    {
        if (_currentProduct == null) return;

        FavouriteToggled?.Invoke(this, new ProductActionEventArgs(_currentProduct));
        
        // Refresh button text
        if (_currentProduct != null)
        {
            LoadProduct(_currentProduct);
        }
    }

    /// <summary>
    /// Handle open folder button click
    /// </summary>
    private void OnOpenFolderClicked(object? sender, EventArgs e)
    {
        if (_currentProduct == null) return;

        try
        {
            var folderPath = Path.GetDirectoryName(_currentProduct.FolderPath);
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                Process.Start("explorer.exe", folderPath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open folder: {ex.Message}", MessageBoxType.Error);
        }
    }

    /// <summary>
    /// Handle add tag button click
    /// </summary>
    private void OnAddTagClicked(object? sender, EventArgs e)
    {
        if (_currentProduct == null) return;
        TagRequested?.Invoke(this, new ProductActionEventArgs(_currentProduct));
    }

    /// <summary>
    /// Handle add to collection button click
    /// </summary>
    private void OnAddToCollectionClicked(object? sender, EventArgs e)
    {
        if (_currentProduct == null) return;
        CollectionRequested?.Invoke(this, new ProductActionEventArgs(_currentProduct));
    }

    /// <summary>
    /// Handle holder selection changes
    /// </summary>
    private void OnHolderSelectionChanged(object? sender, EventArgs e)
    {
        if (_currentProduct == null) return;

        // If "no holder" is checked, disable dropdowns
        if (_noHolderCheckBox.Checked == true)
        {
            _holderVariantDropDown.Enabled = false;
            _holderColorDropDown.Enabled = false;
            
            HolderSelectionChanged?.Invoke(this, new HolderSelectionEventArgs(null, null));
            return;
        }

        _holderVariantDropDown.Enabled = true;

        // Load colors when holder variant selected
        if (sender == _holderVariantDropDown && _holderVariantDropDown.SelectedIndex > 0)
        {
            var selectedVariant = _holderVariantDropDown.SelectedValue?.ToString();
            var holder = _currentProduct.Holders?.FirstOrDefault(h => h.Variant == selectedVariant);
            
            if (holder != null)
            {
                LoadColorsForHolder(holder);
            }
        }

        // Fire event when both selections made
        if (_holderVariantDropDown.SelectedIndex > 0)
        {
            var variant = _holderVariantDropDown.SelectedValue?.ToString();
            var color = _holderColorDropDown.SelectedIndex > 0 
                ? _holderColorDropDown.SelectedValue?.ToString() 
                : null;

            HolderSelectionChanged?.Invoke(this, new HolderSelectionEventArgs(variant, color));
        }
    }

    /// <summary>
    /// Show or hide empty state
    /// </summary>
    private void ShowEmptyState(bool show = true)
    {
        if (show)
        {
            Content = _emptyStateLabel;
        }
    }

    /// <summary>
    /// Refresh current product (e.g., after tag/favourite changes)
    /// </summary>
    public void Refresh()
    {
        if (_currentProduct != null)
        {
            LoadProduct(_currentProduct);
        }
    }
}

/// <summary>
/// Event args for product actions
/// </summary>
public class ProductActionEventArgs : EventArgs
{
    public Product Product { get; }

    public ProductActionEventArgs(Product product)
    {
        Product = product;
    }
}

/// <summary>
/// Event args for holder selection
/// </summary>
public class HolderSelectionEventArgs : EventArgs
{
    public string? SelectedVariant { get; }
    public string? SelectedColor { get; }

    public HolderSelectionEventArgs(string? variant, string? color)
    {
        SelectedVariant = variant;
        SelectedColor = color;
    }
}
