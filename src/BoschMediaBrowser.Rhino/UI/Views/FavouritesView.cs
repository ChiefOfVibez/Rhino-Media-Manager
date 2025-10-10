using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using BoschMediaBrowser.Rhino.UI.Controls;

namespace BoschMediaBrowser.Rhino.UI.Views;

/// <summary>
/// View for browsing and managing favourite products
/// </summary>
public class FavouritesView : Panel
{
    private readonly DataService _dataService;
    private readonly SearchService _searchService;
    private readonly UserDataService _userDataService;
    private readonly ThumbnailService _thumbnailService;

    private ThumbnailGrid _thumbnailGrid;
    private DetailPane _detailPane;
    private Label _headerLabel;
    private Button _removeFromFavouritesButton;
    private Button _clearAllFavouritesButton;

    public event EventHandler<ProductSelectedEventArgs>? ProductSelected;

    public FavouritesView(
        DataService dataService,
        SearchService searchService,
        UserDataService userDataService,
        ThumbnailService thumbnailService)
    {
        _dataService = dataService;
        _searchService = searchService;
        _userDataService = userDataService;
        _thumbnailService = thumbnailService;

        InitializeControls();
        BuildLayout();
        LoadFavourites();
    }

    private void InitializeControls()
    {
        // Header
        _headerLabel = new Label
        {
            Text = "Favourite Products",
            Font = SystemFonts.Bold(16)
        };

        // Action buttons
        _removeFromFavouritesButton = new Button
        {
            Text = "Remove from Favourites",
            Enabled = false
        };
        _removeFromFavouritesButton.Click += OnRemoveFromFavourites;

        _clearAllFavouritesButton = new Button
        {
            Text = "Clear All Favourites"
        };
        _clearAllFavouritesButton.Click += OnClearAllFavourites;

        // Thumbnail grid (reuse control)
        _thumbnailGrid = new ThumbnailGrid(_thumbnailService, _userDataService);
        _thumbnailGrid.ProductSelected += OnProductSelected;

        // Detail pane (reuse control)
        _detailPane = new DetailPane(_userDataService, null!, _thumbnailService);
        _detailPane.FavouriteToggled += OnFavouriteToggled;
    }

    private void BuildLayout()
    {
        // Toolbar
        var toolbar = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Padding = new Padding(10),
            Items =
            {
                new StackLayoutItem(_headerLabel, true),
                _removeFromFavouritesButton,
                _clearAllFavouritesButton
            }
        };

        // Split panel: Grid on left, detail on right
        var splitPanel = new Splitter
        {
            Position = 600,
            Panel1 = _thumbnailGrid,
            Panel2 = _detailPane
        };

        Content = new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Items =
            {
                new StackLayoutItem(toolbar, false),
                new StackLayoutItem(splitPanel, true)
            }
        };
    }

    /// <summary>
    /// Load favourite products
    /// </summary>
    public void LoadFavourites()
    {
        var allProducts = _dataService.GetProducts();
        var favourites = _userDataService.GetAllFavourites();
        
        var favouriteIds = new HashSet<string>(favourites.Select(f => f.ProductId));
        var favouriteProducts = allProducts.Where(p => favouriteIds.Contains(p.Id)).ToList();

        _thumbnailGrid.LoadProducts(favouriteProducts);
        
        _headerLabel.Text = $"Favourite Products ({favouriteProducts.Count})";
    }

    /// <summary>
    /// Handle product selection
    /// </summary>
    private void OnProductSelected(object? sender, ProductSelectedEventArgs e)
    {
        _detailPane.LoadProduct(e.Product);
        _removeFromFavouritesButton.Enabled = true;
        ProductSelected?.Invoke(this, e);
    }

    /// <summary>
    /// Remove selected product from favourites
    /// </summary>
    private void OnRemoveFromFavourites(object? sender, EventArgs e)
    {
        var selectedProduct = _thumbnailGrid.SelectedProduct;
        if (selectedProduct == null) return;

        _userDataService.RemoveFavourite(selectedProduct.Id);
        LoadFavourites(); // Refresh list
        _removeFromFavouritesButton.Enabled = false;
    }

    /// <summary>
    /// Clear all favourites with confirmation
    /// </summary>
    private void OnClearAllFavourites(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to remove all favourites?",
            "Confirm Clear All",
            MessageBoxButtons.YesNo,
            MessageBoxType.Question
        );

        if (result == DialogResult.Yes)
        {
            var allFavourites = _userDataService.GetAllFavourites();
            foreach (var favourite in allFavourites)
            {
                _userDataService.RemoveFavourite(favourite.ProductId);
            }
            
            LoadFavourites(); // Refresh list
        }
    }

    /// <summary>
    /// Handle favourite toggled from detail pane
    /// </summary>
    private void OnFavouriteToggled(object? sender, ProductActionEventArgs e)
    {
        // Refresh the list
        LoadFavourites();
        _detailPane.Refresh();
    }

    /// <summary>
    /// Refresh view (call when favourites change externally)
    /// </summary>
    public void Refresh()
    {
        LoadFavourites();
    }
}
