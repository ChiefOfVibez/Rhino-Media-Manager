using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using BoschMediaBrowser.Rhino.UI.Controls;

namespace BoschMediaBrowser.Rhino.UI.Views;

/// <summary>
/// View for managing product collections
/// </summary>
public class CollectionsView : Panel
{
    private readonly DataService _dataService;
    private readonly UserDataService _userDataService;
    private readonly ThumbnailService _thumbnailService;

    private ListBox _collectionsListBox;
    private ThumbnailGrid _thumbnailGrid;
    private DetailPane _detailPane;
    private Button _createCollectionButton;
    private Button _renameCollectionButton;
    private Button _deleteCollectionButton;
    private Button _removeProductButton;
    private Label _collectionNameLabel;

    private Collection? _selectedCollection;

    public event EventHandler<ProductSelectedEventArgs>? ProductSelected;

    public CollectionsView(
        DataService dataService,
        UserDataService userDataService,
        ThumbnailService thumbnailService)
    {
        _dataService = dataService;
        _userDataService = userDataService;
        _thumbnailService = thumbnailService;

        InitializeControls();
        BuildLayout();
        LoadCollections();
    }

    private void InitializeControls()
    {
        // Collections list
        _collectionsListBox = new ListBox { Width = 200 };
        _collectionsListBox.SelectedIndexChanged += OnCollectionSelected;

        // Collection management buttons
        _createCollectionButton = new Button { Text = "➕ New Collection" };
        _createCollectionButton.Click += OnCreateCollection;

        _renameCollectionButton = new Button { Text = "✏️ Rename", Enabled = false };
        _renameCollectionButton.Click += OnRenameCollection;

        _deleteCollectionButton = new Button { Text = "🗑️ Delete", Enabled = false };
        _deleteCollectionButton.Click += OnDeleteCollection;

        // Product management
        _removeProductButton = new Button { Text = "Remove from Collection", Enabled = false };
        _removeProductButton.Click += OnRemoveProduct;

        _collectionNameLabel = new Label
        {
            Text = "No collection selected",
            Font = SystemFonts.Bold(14)
        };

        // Thumbnail grid for products in collection
        _thumbnailGrid = new ThumbnailGrid(_thumbnailService, _userDataService);
        _thumbnailGrid.ProductSelected += OnProductSelected;

        // Detail pane
        _detailPane = new DetailPane(_userDataService, null!, _thumbnailService);
    }

    private void BuildLayout()
    {
        // Left sidebar: Collections list
        var collectionsSidebar = new StackLayout
        {
            Spacing = 5,
            Padding = new Padding(10),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Items =
            {
                new Label { Text = "Collections", Font = SystemFonts.Bold() },
                new StackLayoutItem(_collectionsListBox, true),
                _createCollectionButton,
                _renameCollectionButton,
                _deleteCollectionButton
            }
        };

        // Right content: Products in selected collection
        var productsContent = new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Items =
            {
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Padding = new Padding(10),
                    Spacing = 10,
                    Items =
                    {
                        new StackLayoutItem(_collectionNameLabel, true),
                        _removeProductButton
                    }
                },
                new StackLayoutItem(new Splitter
                {
                    Position = 600,
                    Panel1 = _thumbnailGrid,
                    Panel2 = _detailPane
                }, true)
            }
        };

        // Main split: Collections list on left, products on right
        Content = new Splitter
        {
            Position = 220,
            FixedPanel = SplitterFixedPanel.Panel1,
            Panel1 = collectionsSidebar,
            Panel2 = productsContent
        };
    }

    /// <summary>
    /// Load all collections
    /// </summary>
    public void LoadCollections()
    {
        _collectionsListBox.Items.Clear();
        
        var collections = _userDataService.GetCollections();
        
        if (collections.Count == 0)
        {
            _collectionsListBox.Items.Add("(No collections)");
        }
        else
        {
            foreach (var collection in collections.OrderBy(c => c.Name))
            {
                _collectionsListBox.Items.Add($"{collection.Name} ({collection.ProductIds.Count})");
            }
        }
    }

    /// <summary>
    /// Handle collection selection
    /// </summary>
    private void OnCollectionSelected(object? sender, EventArgs e)
    {
        if (_collectionsListBox.SelectedIndex < 0)
        {
            _selectedCollection = null;
            _renameCollectionButton.Enabled = false;
            _deleteCollectionButton.Enabled = false;
            _thumbnailGrid.LoadProducts(new List<Product>());
            _collectionNameLabel.Text = "No collection selected";
            return;
        }

        var collections = _userDataService.GetCollections().OrderBy(c => c.Name).ToList();
        if (_collectionsListBox.SelectedIndex >= collections.Count) return;

        _selectedCollection = collections[_collectionsListBox.SelectedIndex];
        _renameCollectionButton.Enabled = true;
        _deleteCollectionButton.Enabled = true;

        LoadCollectionProducts(_selectedCollection);
    }

    /// <summary>
    /// Load products in selected collection
    /// </summary>
    private void LoadCollectionProducts(Collection collection)
    {
        _collectionNameLabel.Text = $"{collection.Name} ({collection.ProductIds.Count} products)";

        var allProducts = _dataService.GetProducts();
        var collectionProducts = allProducts
            .Where(p => collection.ProductIds.Contains(p.Id))
            .ToList();

        _thumbnailGrid.LoadProducts(collectionProducts);
    }

    /// <summary>
    /// Create new collection
    /// </summary>
    private void OnCreateCollection(object? sender, EventArgs e)
    {
        var dialog = new Dialog<string?>();
        dialog.Title = "Create Collection";
        dialog.MinimumSize = new Size(400, 150);

        var nameTextBox = new TextBox { PlaceholderText = "Collection name" };
        var descTextBox = new TextBox { PlaceholderText = "Description (optional)" };

        var createButton = new Button { Text = "Create" };
        createButton.Click += (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Collection name is required", MessageBoxType.Error);
                return;
            }
            dialog.Close(nameTextBox.Text);
        };

        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Click += (s, args) => dialog.Close(null);

        dialog.Content = new StackLayout
        {
            Padding = new Padding(10),
            Spacing = 10,
            Items =
            {
                new Label { Text = "Collection Name:" },
                nameTextBox,
                new Label { Text = "Description:" },
                descTextBox
            }
        };

        dialog.PositiveButtons.Add(createButton);
        dialog.NegativeButtons.Add(cancelButton);
        dialog.DefaultButton = createButton;
        dialog.AbortButton = cancelButton;

        var result = dialog.ShowModal();
        
        if (!string.IsNullOrEmpty(result))
        {
            _userDataService.CreateCollection(result, descTextBox.Text);
            LoadCollections();
        }
    }

    /// <summary>
    /// Rename selected collection
    /// </summary>
    private void OnRenameCollection(object? sender, EventArgs e)
    {
        if (_selectedCollection == null) return;

        var dialog = new Dialog<string?>();
        dialog.Title = "Rename Collection";
        dialog.MinimumSize = new Size(400, 120);

        var nameTextBox = new TextBox { Text = _selectedCollection.Name };

        var renameButton = new Button { Text = "Rename" };
        renameButton.Click += (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Collection name is required", MessageBoxType.Error);
                return;
            }
            dialog.Close(nameTextBox.Text);
        };

        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Click += (s, args) => dialog.Close(null);

        dialog.Content = new StackLayout
        {
            Padding = new Padding(10),
            Spacing = 10,
            Items =
            {
                new Label { Text = "New Name:" },
                nameTextBox
            }
        };

        dialog.PositiveButtons.Add(renameButton);
        dialog.NegativeButtons.Add(cancelButton);
        dialog.DefaultButton = renameButton;

        var result = dialog.ShowModal();
        
        if (!string.IsNullOrEmpty(result))
        {
            _selectedCollection.Name = result;
            _userDataService.UpdateCollection(_selectedCollection);
            LoadCollections();
        }
    }

    /// <summary>
    /// Delete selected collection
    /// </summary>
    private void OnDeleteCollection(object? sender, EventArgs e)
    {
        if (_selectedCollection == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the collection '{_selectedCollection.Name}'?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxType.Question
        );

        if (result == DialogResult.Yes)
        {
            _userDataService.DeleteCollection(_selectedCollection.Id);
            LoadCollections();
            _selectedCollection = null;
            _thumbnailGrid.LoadProducts(new List<Product>());
        }
    }

    /// <summary>
    /// Remove selected product from collection
    /// </summary>
    private void OnRemoveProduct(object? sender, EventArgs e)
    {
        if (_selectedCollection == null) return;
        
        var selectedProduct = _thumbnailGrid.SelectedProduct;
        if (selectedProduct == null) return;

        _userDataService.RemoveProductFromCollection(_selectedCollection.Id, selectedProduct.Id);
        LoadCollectionProducts(_selectedCollection);
        _removeProductButton.Enabled = false;
    }

    /// <summary>
    /// Handle product selection
    /// </summary>
    private void OnProductSelected(object? sender, ProductSelectedEventArgs e)
    {
        _detailPane.LoadProduct(e.Product);
        _removeProductButton.Enabled = true;
        ProductSelected?.Invoke(this, e);
    }

    /// <summary>
    /// Add product to selected collection (called externally)
    /// </summary>
    public void AddProductToCurrentCollection(Product product)
    {
        if (_selectedCollection == null)
        {
            MessageBox.Show("Please select a collection first", MessageBoxType.Information);
            return;
        }

        _userDataService.AddProductToCollection(_selectedCollection.Id, product.Id);
        LoadCollectionProducts(_selectedCollection);
    }

    /// <summary>
    /// Refresh view
    /// </summary>
    public void Refresh()
    {
        LoadCollections();
        if (_selectedCollection != null)
        {
            LoadCollectionProducts(_selectedCollection);
        }
    }
}
