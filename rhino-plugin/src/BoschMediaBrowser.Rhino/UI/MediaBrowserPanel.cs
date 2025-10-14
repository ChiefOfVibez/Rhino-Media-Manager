using Eto.Drawing;
using Eto.Forms;
using Rhino;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using BoschMediaBrowser.Rhino.UI.Controls;
using BoschMediaBrowser.Rhino.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoschMediaBrowser.Rhino.UI;

/// <summary>
/// Main dockable panel for the Bosch Media Browser with rich UI
/// </summary>
[System.Runtime.InteropServices.Guid("A3B5C7D9-1E2F-4A5B-8C9D-0E1F2A3B4C5D")]
public class MediaBrowserPanel : Panel
{
    // Services (lazy initialized)
    private DataService? _dataService;
    private SearchService? _searchService;
    private SettingsService? _settingsService;
    private UserDataService? _userDataService;
    private ThumbnailService? _thumbnailService;

    // Custom UI Controls
    private TextBox? _searchBox;
    private Label? _statusLabel;
    private TabControl? _tabs;
    private Button? _refreshButton;
    private Button? _settingsButton;
    private Button? _viewModeButton;
    private Button? _multiSelectButton;
    
    // Rich UI Components
    private CategoryList? _categoryList;
    private ProductCardGrid? _productCardGrid;
    private ProductListView? _productListView;
    private Panel? _productViewContainer; // Container that switches between grid/list
    private FiltersBar? _filtersBar;
    private DetailPane? _detailPane;
    private bool _isGridView = true;
    
    // Data
    private List<Product> _allProducts = new();
    private List<Product> _filteredProducts = new();
    private string _searchText = string.Empty;
    private bool _multiSelectMode = false;
    private bool _isInitialized = false;
    private bool _isInitializing = false;
    private int _onShownCallCount = 0;
    
    // File logging for crash debugging
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "BoschMediaBrowser_Crash.log");
    
    private static void LogToFile(string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            File.AppendAllText(LogFilePath, $"[{timestamp}] {message}\n");
        }
        catch
        {
            // Ignore logging errors
        }
    }

    public MediaBrowserPanel()
    {
        // ABSOLUTELY MINIMAL CONSTRUCTOR - DO NOTHING
        // All initialization happens in OnShown
        LogToFile("=== MediaBrowserPanel: Constructor (EMPTY) ===");
        RhinoApp.WriteLine("=== MediaBrowserPanel: Constructor (EMPTY) ===");
        
        // Set preferred default size (wider to fit 3 cards comfortably)
        Size = new Size(820, 700);
    }
    
    /// <summary>
    /// Called when the panel is shown
    /// </summary>
    protected override void OnShown(EventArgs e)
    {
        try
        {
            _onShownCallCount++;
            LogToFile("==========================================");
            LogToFile($"MediaBrowserPanel: OnShown ENTRY (Call #{_onShownCallCount})");
            LogToFile("==========================================");
            RhinoApp.WriteLine("==========================================");
            RhinoApp.WriteLine($"MediaBrowserPanel: OnShown ENTRY (Call #{_onShownCallCount})");
            RhinoApp.WriteLine("==========================================");
            
            // If called too many times, something is causing an infinite loop
            if (_onShownCallCount > 5)
            {
                LogToFile($"WARNING: OnShown called {_onShownCallCount} times - possible infinite loop!");
                RhinoApp.WriteLine($"WARNING: OnShown called {_onShownCallCount} times - possible infinite loop!");
                return; // Exit early to break the loop
            }
            
            base.OnShown(e);
            LogToFile("MediaBrowserPanel: base.OnShown completed");
            RhinoApp.WriteLine("MediaBrowserPanel: base.OnShown completed");
            
            // Initialize only when panel is actually shown
            if (!_isInitialized)
            {
                LogToFile("MediaBrowserPanel: Not yet initialized, starting init...");
                RhinoApp.WriteLine("MediaBrowserPanel: Not yet initialized, starting init...");
                
                try
                {
                    // Show loading state
                    LogToFile("MediaBrowserPanel: Step 1 - Creating loading Label...");
                    RhinoApp.WriteLine("MediaBrowserPanel: Step 1 - Creating loading Label...");
                    var loadingLabel = new Label
                    {
                        Text = "Loading Bosch Media Browser...\n\nPlease wait...",
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    LogToFile("MediaBrowserPanel: Step 2 - Setting Content...");
                    RhinoApp.WriteLine("MediaBrowserPanel: Step 2 - Setting Content...");
                    Content = loadingLabel;
                    LogToFile("MediaBrowserPanel: Loading UI created successfully");
                    RhinoApp.WriteLine("MediaBrowserPanel: Loading UI created successfully");
                    
                    // Start async initialization with detailed logging
                    LogToFile("MediaBrowserPanel: Step 3 - Scheduling async initialization...");
                    RhinoApp.WriteLine("MediaBrowserPanel: Step 3 - Scheduling async initialization...");
                    Application.Instance.AsyncInvoke(() =>
                    {
                        try
                        {
                            LogToFile("MediaBrowserPanel: AsyncInvoke callback started");
                            RhinoApp.WriteLine("MediaBrowserPanel: AsyncInvoke callback started");
                            InitializePanel();
                            LogToFile("MediaBrowserPanel: AsyncInvoke callback completed");
                            RhinoApp.WriteLine("MediaBrowserPanel: AsyncInvoke callback completed");
                        }
                        catch (Exception asyncEx)
                        {
                            LogToFile("==========================================");
                            LogToFile("FATAL: AsyncInvoke callback crashed");
                            LogToFile($"ERROR: {asyncEx.Message}");
                            LogToFile($"TYPE: {asyncEx.GetType().FullName}");
                            LogToFile($"STACK: {asyncEx.StackTrace}");
                            LogToFile("==========================================");
                            RhinoApp.WriteLine("==========================================");
                            RhinoApp.WriteLine("FATAL: AsyncInvoke callback crashed");
                            RhinoApp.WriteLine($"ERROR: {asyncEx.Message}");
                            RhinoApp.WriteLine($"TYPE: {asyncEx.GetType().FullName}");
                            RhinoApp.WriteLine($"STACK: {asyncEx.StackTrace}");
                            RhinoApp.WriteLine("==========================================");
                        }
                    });
                    LogToFile("MediaBrowserPanel: Async initialization scheduled");
                    RhinoApp.WriteLine("MediaBrowserPanel: Async initialization scheduled");
                }
                catch (Exception loadingEx)
                {
                    LogToFile("==========================================");
                    LogToFile("FATAL: Error creating loading UI");
                    LogToFile($"ERROR: {loadingEx.Message}");
                    LogToFile($"TYPE: {loadingEx.GetType().FullName}");
                    LogToFile($"STACK: {loadingEx.StackTrace}");
                    RhinoApp.WriteLine("==========================================");
                    RhinoApp.WriteLine("FATAL: Error creating loading UI");
                    RhinoApp.WriteLine($"ERROR: {loadingEx.Message}");
                    RhinoApp.WriteLine($"TYPE: {loadingEx.GetType().FullName}");
                    RhinoApp.WriteLine($"STACK: {loadingEx.StackTrace}");
                    if (loadingEx.InnerException != null)
                    {
                        RhinoApp.WriteLine($"INNER: {loadingEx.InnerException.Message}");
                    }
                    RhinoApp.WriteLine("==========================================");
                    
                    // Try to show error without crashing
                    try
                    {
                        Content = new Label
                        {
                            Text = $"ERROR: {loadingEx.Message}",
                            TextColor = Colors.Red
                        };
                    }
                    catch
                    {
                        // Can't even set content
                        RhinoApp.WriteLine("CRITICAL: Cannot set Content property");
                    }
                }
            }
            else
            {
                LogToFile("MediaBrowserPanel: Already initialized, skipping init");
                RhinoApp.WriteLine("MediaBrowserPanel: Already initialized");
            }
            
            LogToFile($"MediaBrowserPanel: OnShown EXIT (Call #{_onShownCallCount})");
            RhinoApp.WriteLine($"MediaBrowserPanel: OnShown EXIT (Call #{_onShownCallCount})");
        }
        catch (Exception ex)
        {
            LogToFile("==========================================");
            LogToFile("FATAL: MediaBrowserPanel OnShown CRASHED");
            LogToFile($"ERROR: {ex.Message}");
            LogToFile($"TYPE: {ex.GetType().FullName}");
            LogToFile($"STACK: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogToFile($"INNER: {ex.InnerException.Message}");
                LogToFile($"INNER STACK: {ex.InnerException.StackTrace}");
            }
            LogToFile("==========================================");
            
            RhinoApp.WriteLine("==========================================");
            RhinoApp.WriteLine("FATAL: MediaBrowserPanel OnShown CRASHED");
            RhinoApp.WriteLine($"ERROR: {ex.Message}");
            RhinoApp.WriteLine($"TYPE: {ex.GetType().FullName}");
            RhinoApp.WriteLine($"STACK: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                RhinoApp.WriteLine($"INNER: {ex.InnerException.Message}");
                RhinoApp.WriteLine($"INNER STACK: {ex.InnerException.StackTrace}");
            }
            RhinoApp.WriteLine("==========================================");
            
            // Try to call base to not break Rhino
            try
            {
                base.OnShown(e);
            }
            catch
            {
                // Ignore
            }
        }
    }
    
    private void InitializePanel()
    {
        if (_isInitialized) return;
        
        try
        {
            LogToFile("MediaBrowserPanel: Starting deferred initialization...");
            RhinoApp.WriteLine("MediaBrowserPanel: Starting deferred initialization...");
            
            // Initialize services
            try
            {
                LogToFile("MediaBrowserPanel: Creating SearchService...");
                RhinoApp.WriteLine("MediaBrowserPanel: Creating SearchService...");
                _searchService = new SearchService();
                LogToFile("MediaBrowserPanel: SearchService OK");
                RhinoApp.WriteLine("MediaBrowserPanel: SearchService OK");
                
                LogToFile("MediaBrowserPanel: Creating SettingsService...");
                RhinoApp.WriteLine("MediaBrowserPanel: Creating SettingsService...");
                _settingsService = new SettingsService();
                LogToFile("MediaBrowserPanel: SettingsService OK");
                RhinoApp.WriteLine("MediaBrowserPanel: SettingsService OK");
                
                LogToFile("MediaBrowserPanel: Creating UserDataService...");
                RhinoApp.WriteLine("MediaBrowserPanel: Creating UserDataService...");
                _userDataService = new UserDataService();
                LogToFile("MediaBrowserPanel: UserDataService OK");
                RhinoApp.WriteLine("MediaBrowserPanel: UserDataService OK");
                
                LogToFile("MediaBrowserPanel: Creating ThumbnailService...");
                RhinoApp.WriteLine("MediaBrowserPanel: Creating ThumbnailService...");
                var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BoschMediaBrowser", "ThumbnailCache");
                _thumbnailService = new ThumbnailService(cachePath);
                LogToFile("MediaBrowserPanel: ThumbnailService OK");
                RhinoApp.WriteLine("MediaBrowserPanel: ThumbnailService OK");
                
                LogToFile("MediaBrowserPanel: All services initialized successfully");
                RhinoApp.WriteLine("MediaBrowserPanel: All services initialized successfully");
            }
            catch (Exception servicesEx)
            {
                LogToFile($"FATAL ERROR initializing services: {servicesEx.Message}");
                LogToFile($"Stack: {servicesEx.StackTrace}");
                RhinoApp.WriteLine($"FATAL ERROR initializing services: {servicesEx.Message}");
                RhinoApp.WriteLine($"Stack: {servicesEx.StackTrace}");
                throw;
            }

            // Create UI
            try
            {
                LogToFile("MediaBrowserPanel: Building UI...");
                RhinoApp.WriteLine("MediaBrowserPanel: Building UI...");
                BuildUI();
                LogToFile("MediaBrowserPanel: UI built successfully");
                RhinoApp.WriteLine("MediaBrowserPanel: UI built successfully");
            }
            catch (Exception uiEx)
            {
                LogToFile($"FATAL ERROR building UI: {uiEx.Message}");
                LogToFile($"Stack: {uiEx.StackTrace}");
                RhinoApp.WriteLine($"FATAL ERROR building UI: {uiEx.Message}");
                RhinoApp.WriteLine($"Stack: {uiEx.StackTrace}");
                throw;
            }
            
            _isInitialized = true;
            
            LogToFile("MediaBrowserPanel: Updating Content on UI thread...");
            RhinoApp.WriteLine("MediaBrowserPanel: Updating Content on UI thread...");
            
            // Update Content on main thread
            Application.Instance.Invoke(() =>
            {
                try
                {
                    LogToFile("MediaBrowserPanel: Setting Content to built UI...");
                    Content = Content; // Force refresh
                    LogToFile("MediaBrowserPanel: Content updated successfully");
                    RhinoApp.WriteLine("MediaBrowserPanel: Content updated successfully");
                }
                catch (Exception contentEx)
                {
                    LogToFile($"ERROR updating Content: {contentEx.Message}");
                    LogToFile($"Stack: {contentEx.StackTrace}");
                    RhinoApp.WriteLine($"ERROR updating Content: {contentEx.Message}");
                }
            });
            
            // Load data asynchronously
            try
            {
                LogToFile("MediaBrowserPanel: Starting async data load...");
                RhinoApp.WriteLine("MediaBrowserPanel: Starting async data load...");
                _ = InitializeAsync();
            }
            catch (Exception asyncEx)
            {
                LogToFile($"ERROR starting async data load: {asyncEx.Message}");
                LogToFile($"Stack: {asyncEx.StackTrace}");
                RhinoApp.WriteLine($"ERROR starting async data load: {asyncEx.Message}");
                // Don't throw - let UI show without data
            }
            
            LogToFile("=== MediaBrowserPanel: Deferred initialization completed ===");
            RhinoApp.WriteLine("=== MediaBrowserPanel: Deferred initialization completed ===");
        }
        catch (Exception ex)
        {
            LogToFile($"==========================================");
            LogToFile($"FATAL: MediaBrowserPanel InitializePanel CRASHED");
            LogToFile($"ERROR: {ex.Message}");
            LogToFile($"TYPE: {ex.GetType().FullName}");
            LogToFile($"STACK: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogToFile($"INNER: {ex.InnerException.Message}");
                LogToFile($"INNER STACK: {ex.InnerException.StackTrace}");
            }
            LogToFile($"==========================================");
            
            RhinoApp.WriteLine($"==========================================");
            RhinoApp.WriteLine($"FATAL: MediaBrowserPanel InitializePanel CRASHED");
            RhinoApp.WriteLine($"ERROR: {ex.Message}");
            RhinoApp.WriteLine($"TYPE: {ex.GetType().FullName}");
            RhinoApp.WriteLine($"STACK: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                RhinoApp.WriteLine($"INNER: {ex.InnerException.Message}");
                RhinoApp.WriteLine($"INNER STACK: {ex.InnerException.StackTrace}");
            }
            RhinoApp.WriteLine($"==========================================");
            
            Application.Instance.Invoke(() =>
            {
                Content = new Label
                {
                    Text = $"Panel initialization failed:\n\n{ex.Message}\n\nSee command window for details.",
                    TextColor = Colors.Red,
                    Wrap = WrapMode.Word,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
            });
        }
    }

    private void BuildUI()
    {
        // Toolbar with search and view controls
        _searchBox = new TextBox 
        { 
            PlaceholderText = "Search products by name, SKU, or tags..."
        };
        _searchBox.TextChanged += OnSearchTextChanged;
        
        _viewModeButton = new Button 
        { 
            Text = "Grid View",
            ToolTip = "Toggle between Grid and List view"
        };
        _viewModeButton.Click += OnViewModeToggled;
        
        _multiSelectButton = new Button 
        { 
            Text = "Multi-Select",
            ToolTip = "Enable multi-select mode for batch operations"
        };
        _multiSelectButton.Click += OnMultiSelectToggled;
        
        _refreshButton = new Button { Text = "⟳ Refresh" };
        _refreshButton.Click += OnRefreshClicked;
        
        _settingsButton = new Button { Text = "⚙ Settings" };
        _settingsButton.Click += OnSettingsClicked;

        var toolbar = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Padding = 8,
            Spacing = 8,
            Items =
            {
                new StackLayoutItem(_searchBox, true),
                _viewModeButton,
                _multiSelectButton,
                _refreshButton,
                _settingsButton
            }
        };

        // Tabs
        _tabs = new TabControl();
        
        // Browse Tab with rich UI
        var browseTab = new TabPage { Text = "📁 Browse" };
        browseTab.Content = CreateBrowseTab();
        
        // Favourites Tab - Using FavouritesView
        var favouritesTab = new TabPage { Text = "⭐ Favourites" };
        favouritesTab.Content = CreateFavouritesTab();
        
        // Collections Tab - Using CollectionsView
        var collectionsTab = new TabPage { Text = "📂 Collections" };
        collectionsTab.Content = CreateCollectionsTab();

        _tabs.Pages.Add(browseTab);
        _tabs.Pages.Add(favouritesTab);
        _tabs.Pages.Add(collectionsTab);

        // Status bar with product count
        _statusLabel = new Label 
        { 
            Text = "Initializing...",
            VerticalAlignment = VerticalAlignment.Center,
            TextColor = Color.FromArgb(60, 60, 60) // Dark gray for readability
        };
        
        var statusBar = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Padding = 8,
            Spacing = 8,
            BackgroundColor = Color.FromArgb(240, 240, 240), // Light gray background
            Items = 
            { 
                _statusLabel
            }
        };

        // Main layout: 2-column with left sidebar tabs and right product area
        var leftSidebar = CreateLeftSidebar();
        var rightArea = CreateMainProductArea();
        
        var splitter = new Splitter
        {
            Panel1 = leftSidebar,
            Panel2 = rightArea,
            RelativePosition = 0.25, // 25% for sidebar, 75% for products
            FixedPanel = SplitterFixedPanel.None,
            Orientation = Orientation.Horizontal
        };
        
        Content = new TableLayout
        {
            Padding = 0,
            Spacing = new Size(0, 0),
            Rows =
            {
                toolbar,
                new TableRow(splitter) { ScaleHeight = true },
                statusBar
            }
        };
    }
    
    private Control CreateLeftSidebar()
    {
        // Left sidebar with tabs: Browse (categories), Collections (fav+collections), Filters
        var sidebarTabs = new TabControl();
        
        // Browse tab - category tree
        var browseTab = new TabPage { Text = "📁 Browse" };
        _categoryList = new CategoryList();
        _categoryList.CategorySelected += OnCategorySelected;
        browseTab.Content = new Panel
        {
            Content = _categoryList,
            Padding = 5
        };
        
        // Collections tab - favourites + collections combined
        var collectionsTab = new TabPage { Text = "⭐ Collections" };
        collectionsTab.Content = CreateCollectionsTabContent();
        
        // Filters tab
        var filtersTab = new TabPage { Text = "🔧 Filters" };
        _filtersBar = new FiltersBar(_searchService);
        _filtersBar.FiltersChanged += OnFiltersChanged;
        filtersTab.Content = new Panel
        {
            Content = _filtersBar,
            Padding = 5
        };
        
        sidebarTabs.Pages.Add(browseTab);
        sidebarTabs.Pages.Add(collectionsTab);
        sidebarTabs.Pages.Add(filtersTab);
        
        return sidebarTabs;
    }
    
    private Control CreateCollectionsTabContent()
    {
        // Combine favourites and collections in one tab
        var collectionsTabs = new TabControl();
        
        var favTab = new TabPage { Text = "Favourites" };
        favTab.Content = CreateFavouritesTab();
        
        var collTab = new TabPage { Text = "Collections" };
        collTab.Content = CreateCollectionsTab();
        
        collectionsTabs.Pages.Add(favTab);
        collectionsTabs.Pages.Add(collTab);
        
        return new Panel
        {
            Content = collectionsTabs
        };
    }
    
    private Control CreateMainProductArea()
    {
        // Create both grid and list views
        _productCardGrid = new ProductCardGrid(_thumbnailService, _userDataService);
        _productCardGrid.ProductSelected += OnProductSelectedInGrid;
        _productCardGrid.ProductPreview += OnProductPreviewRequested;
        _productCardGrid.ProductInsert += OnProductInsertRequested;
        
        _productListView = new ProductListView(_thumbnailService, _userDataService);
        _productListView.ProductSelected += OnProductSelectedInGrid;
        _productListView.ProductPreview += OnProductPreviewRequested;
        _productListView.ProductInsert += OnProductInsertRequested;
        
        // Container panel that can switch between views
        _productViewContainer = new Panel
        {
            Content = _productCardGrid // Start with grid view
        };
        
        return _productViewContainer;
    }

    private Control CreateBrowseTab()
    {
        if (_searchService == null || _thumbnailService == null || _userDataService == null || _settingsService == null)
        {
            return new Label { Text = "Services not initialized" };
        }
        
        // Left sidebar: Category List (250px wide)
        _categoryList = new CategoryList();
        _categoryList.CategorySelected += OnCategorySelected;
        
        var categoryPanel = new Panel
        {
            Content = _categoryList,
            MinimumSize = new Size(200, 400),
            Size = new Size(250, -1),
            Padding = 5
        };

        // Center: Filters Bar + Product Card Grid
        _filtersBar = new FiltersBar(_searchService);
        _filtersBar.FiltersChanged += OnFiltersChanged;
        
        _productCardGrid = new ProductCardGrid(_thumbnailService, _userDataService);
        _productCardGrid.ProductSelected += OnProductSelectedInGrid;
        _productCardGrid.ProductPreview += OnProductPreviewRequested;
        _productCardGrid.ProductInsert += OnProductInsertRequested;

        var centerLayout = new TableLayout
        {
            Spacing = new Size(0, 5),
            Padding = new Padding(0),
            Rows =
            {
                _filtersBar,
                new TableRow(_productCardGrid) { ScaleHeight = true }
            }
        };

        // Right sidebar: Detail Pane (300px wide, collapsible)
        _detailPane = new DetailPane(_userDataService!, _settingsService!, _thumbnailService!);
        _detailPane.FavouriteToggled += OnFavouriteToggled;
        _detailPane.HolderSelectionChanged += OnHolderSelectionChanged;
        
        var detailPanel = new Panel
        {
            Content = _detailPane,
            MinimumSize = new Size(250, 400),
            Size = new Size(300, -1),
            Padding = 5
        };

        // Add minimum size to center layout
        var centerPanel = new Panel
        {
            Content = centerLayout,
            MinimumSize = new Size(400, 400)
        };

        // Main layout: 3-column with splitters using relative positioning
        var splitter1 = new Splitter
        {
            Panel1 = categoryPanel,
            Panel2 = centerPanel,
            RelativePosition = 0.2,  // 20% for category tree
            FixedPanel = SplitterFixedPanel.None,
            Orientation = Orientation.Horizontal
        };

        var splitter2 = new Splitter
        {
            Panel1 = splitter1,
            Panel2 = detailPanel,
            RelativePosition = 0.75,  // 75% for left side, 25% for details
            FixedPanel = SplitterFixedPanel.None,
            Orientation = Orientation.Horizontal
        };

        return splitter2;
    }
    
    private Control CreateFavouritesTab()
    {
        // Use FavouritesView
        if (_dataService == null || _searchService == null || _userDataService == null || _thumbnailService == null)
        {
            return new Label { Text = "Loading..." };
        }
        var favouritesView = new FavouritesView(_dataService, _searchService, _userDataService, _thumbnailService);
        favouritesView.ProductSelected += OnProductSelectedInGrid;
        return favouritesView;
    }
    
    private Control CreateCollectionsTab()
    {
        // Use CollectionsView
        if (_dataService == null || _userDataService == null || _thumbnailService == null)
        {
            return new Label { Text = "Loading..." };
        }
        var collectionsView = new CollectionsView(_dataService, _userDataService, _thumbnailService);
        collectionsView.ProductSelected += OnProductSelectedInGrid;
        return collectionsView;
    }

    private async Task InitializeAsync()
    {
        try
        {
            LogToFile("InitializeAsync: ENTRY");
            RhinoApp.WriteLine("InitializeAsync: ENTRY");
            
            // Prevent concurrent initialization
            if (_isInitializing)
            {
                LogToFile("InitializeAsync: Already initializing - EXITING");
                RhinoApp.WriteLine("InitializeAsync: Already initializing - EXITING");
                return;
            }
            
            _isInitializing = true;
            LogToFile("InitializeAsync: Set _isInitializing = true");
            
            if (_settingsService == null || _userDataService == null)
            {
                LogToFile("InitializeAsync: Services not initialized - EXITING");
                UpdateStatus("Services not initialized");
                _isInitializing = false;
                return;
            }
            
            LogToFile("InitializeAsync: Loading settings...");
            UpdateStatus("Loading settings...");
            
            // Load settings and user data
            LogToFile("InitializeAsync: Calling _settingsService.LoadAsync()...");
            var settings = await _settingsService.LoadAsync();
            LogToFile($"InitializeAsync: Settings loaded. BaseServerPath={settings.BaseServerPath}");
            
            LogToFile("InitializeAsync: Calling _userDataService.LoadAsync()...");
            await _userDataService.LoadAsync();
            LogToFile("InitializeAsync: UserData loaded");

            UpdateStatus("Loading products...");
            LogToFile("InitializeAsync: Creating DataService...");

            // Initialize DataService with base path from settings
            _dataService = new DataService(settings.BaseServerPath);
            LogToFile("InitializeAsync: DataService created");

            // Load products
            LogToFile("InitializeAsync: Calling LoadProductsAsync()...");
            RhinoApp.WriteLine($"=== LOADING PRODUCTS FROM: {settings.BaseServerPath} ===");
            var count = await _dataService.LoadProductsAsync();
            LogToFile($"InitializeAsync: Products loaded, count={count}");
            RhinoApp.WriteLine($"=== LOADED {count} PRODUCTS ===");
            
            LogToFile("InitializeAsync: Getting products list...");
            _allProducts = _dataService.GetProducts();
            _filteredProducts = _allProducts;
            LogToFile($"InitializeAsync: Product lists assigned, _allProducts.Count={_allProducts.Count}");
            
            // LOG ALL LOADED PRODUCTS FOR DIAGNOSIS
            RhinoApp.WriteLine($"=== PRODUCT LIST ({_allProducts.Count} total) ===");
            foreach (var p in _allProducts)
            {
                RhinoApp.WriteLine($"  • {p.ProductName} (ID: {p.Id})");
                RhinoApp.WriteLine($"    FolderPath: {p.FolderPath}");
                RhinoApp.WriteLine($"    Range: {p.Range}, Category: {p.Category}");
            }
            RhinoApp.WriteLine($"=== END PRODUCT LIST ===");

            // Update UI with loaded data - using AsyncInvoke to prevent deadlock
            LogToFile("InitializeAsync: Scheduling UI update with AsyncInvoke...");
            Application.Instance.AsyncInvoke(() =>
            {
                try
                {
                    LogToFile("InitializeAsync UI Update: Updating status first...");
                    UpdateStatus($"Loaded {count} products");
                    LogToFile("InitializeAsync UI Update: Status updated");
                    
                    // Load products into controls with performance monitoring
                    LogToFile($"InitializeAsync UI Update: Loading {_allProducts.Count} products into CategoryList...");
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    
                    if (_categoryList != null)
                    {
                        _categoryList.LoadProducts(_allProducts);
                        sw.Stop();
                        LogToFile($"InitializeAsync UI Update: CategoryList.LoadProducts() took {sw.ElapsedMilliseconds}ms");
                    }
                    
                    LogToFile($"InitializeAsync UI Update: Loading {_filteredProducts.Count} products into ProductCardGrid...");
                    sw.Restart();
                    
                    if (_productCardGrid != null)
                    {
                        _productCardGrid.LoadProducts(_filteredProducts);
                        sw.Stop();
                        LogToFile($"InitializeAsync UI Update: ProductCardGrid.LoadProducts() took {sw.ElapsedMilliseconds}ms");
                    }
                    
                    // Load products into FiltersBar to populate dropdowns
                    if (_filtersBar != null)
                    {
                        LogToFile("InitializeAsync UI Update: Loading products into FiltersBar...");
                        _filtersBar.LoadProducts(_allProducts);
                        
                        // Extract and populate tags
                        var allTags = _allProducts
                            .SelectMany(p => p.Tags ?? new List<string>())
                            .Distinct()
                            .ToList();
                        _filtersBar.UpdateTags(allTags);
                        LogToFile($"InitializeAsync UI Update: FiltersBar loaded with {allTags.Count} unique tags");
                    }
                    
                    LogToFile("InitializeAsync UI Update: COMPLETED");
                }
                catch (Exception uiEx)
                {
                    LogToFile($"InitializeAsync UI Update ERROR: {uiEx.Message}");
                    LogToFile($"Stack: {uiEx.StackTrace}");
                    RhinoApp.WriteLine($"InitializeAsync UI Update ERROR: {uiEx.Message}");
                }
            });
            LogToFile("InitializeAsync: UI update scheduled (non-blocking)");

            // TEMPORARILY DISABLE file watcher to prevent UI freezing
            LogToFile("InitializeAsync: Skipping file watcher setup (disabled for debugging)");
            RhinoApp.WriteLine("InitializeAsync: File watcher disabled to prevent UI freezing");
            // _dataService.ProductsReloaded += OnProductsReloaded;
            // _dataService.StartWatching();
            
            _isInitializing = false;
            LogToFile("InitializeAsync: Set _isInitializing = false");
            LogToFile("InitializeAsync: COMPLETED SUCCESSFULLY");
            LogToFile("InitializeAsync: Waiting 100ms before final exit...");
            await Task.Delay(100);
            LogToFile("InitializeAsync: FINAL EXIT - Panel should be responsive now");
            RhinoApp.WriteLine("InitializeAsync: COMPLETED SUCCESSFULLY");
        }
        catch (Exception ex)
        {
            _isInitializing = false;
            LogToFile($"==========================================");
            LogToFile($"FATAL: InitializeAsync CRASHED");
            LogToFile($"ERROR: {ex.Message}");
            LogToFile($"TYPE: {ex.GetType().FullName}");
            LogToFile($"STACK: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogToFile($"INNER: {ex.InnerException.Message}");
                LogToFile($"INNER STACK: {ex.InnerException.StackTrace}");
            }
            LogToFile($"==========================================");
            
            UpdateStatus($"Error loading data: {ex.Message}");
            RhinoApp.WriteLine($"Error in InitializeAsync: {ex.Message}");
            RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void OnProductsReloaded(object? sender, EventArgs e)
    {
        if (_dataService == null) return;

        _allProducts = _dataService.GetProducts();
        ApplySearch();
        UpdateStatus($"Reloaded {_allProducts.Count} products");
    }

    private void OnSearchTextChanged(object? sender, EventArgs e)
    {
        _searchText = _searchBox?.Text ?? string.Empty;
        ApplySearch();
    }

    private void ApplySearch()
    {
        if (_allProducts == null || _searchService == null) return;

        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _filteredProducts = _allProducts;
        }
        else
        {
            var filters = new Filters { SearchText = _searchText };
            _filteredProducts = _searchService.ApplyFilters(_allProducts, filters).ToList();
        }
        
        // Update current view (grid or list)
        LoadProductsIntoCurrentView();
        
        UpdateStatus($"Showing {_filteredProducts.Count} of {_allProducts.Count} products");
    }
    
    private void OnCategorySelected(object? sender, CategorySelectedEventArgs e)
    {
        // Category list already filtered products
        _filteredProducts = e.FilteredProducts;
        
        // Apply search filter if present
        if (!string.IsNullOrEmpty(_searchText))
        {
            var filters = new Filters { SearchText = _searchText };
            _filteredProducts = _searchService.ApplyFilters(_filteredProducts, filters).ToList();
        }
        
        // Update current view (grid or list)
        LoadProductsIntoCurrentView();
        
        UpdateStatus($"Category: {e.Category} ({_filteredProducts.Count} products)");
    }
    
    private void LoadProductsIntoCurrentView()
    {
        if (_isGridView && _productCardGrid != null)
        {
            _productCardGrid.LoadProducts(_filteredProducts);
        }
        else if (!_isGridView && _productListView != null)
        {
            _productListView.LoadProducts(_filteredProducts);
        }
    }
    
    private void OnFiltersChanged(object? sender, FiltersChangedEventArgs e)
    {
        if (_searchService == null) return;
        
        // Apply filters to products
        _filteredProducts = _searchService.ApplyFilters(_allProducts, e.Filters).ToList();
        
        // Update current view (grid or list)
        LoadProductsIntoCurrentView();
        
        UpdateStatus($"Filtered: {_filteredProducts.Count} products");
    }
    
    private void OnProductSelectedInGrid(object? sender, ProductCardEventArgs e)
    {
        // Update detail pane with selected product
        if (_detailPane != null && e.Product != null)
        {
            _detailPane.LoadProduct(e.Product);
        }
        
        UpdateStatus($"Selected: {e.Product?.ProductName ?? "None"}");
    }
    
    private void OnProductPreviewRequested(object? sender, ProductCardEventArgs e)
    {
        // Show preview modal dialog
        RhinoApp.WriteLine($"Preview requested for: {e.Product.ProductName}");
        ShowPreviewModal(e.Product);
        UpdateStatus($"Previewing: {e.Product.ProductName}");
    }
    
    private void InsertProductWithHolder(Product product, Holder? holder, bool includePackaging = false)
    {
        try
        {
            RhinoApp.WriteLine($"=== INSERT REQUESTED ===");
            RhinoApp.WriteLine($"Product: {product.ProductName}");
            RhinoApp.WriteLine($"Holder: {(holder != null ? $"{holder.Variant} - {holder.Color}" : "NONE")}");
            RhinoApp.WriteLine($"Packaging: {includePackaging}");
            
            // Find the tool mesh file (holder-specific if holder selected)
            string? toolMeshPath = Find3DMFile(product, holder);
            
            if (string.IsNullOrEmpty(toolMeshPath) || !System.IO.File.Exists(toolMeshPath))
            {
                UpdateStatus($"No tool mesh found for: {product.ProductName}");
                RhinoApp.WriteLine($"ERROR: No tool mesh found for product {product.ProductName}");
                MessageBox.Show($"No tool mesh found for product: {product.ProductName}", "Insert Failed", MessageBoxType.Warning);
                return;
            }
            
            // Get insertion point (user picks or 0,0,0)
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return;
            
            var getPoint = new global::Rhino.Input.Custom.GetPoint();
            getPoint.SetCommandPrompt("Select insertion point for product");
            getPoint.Get();
            if (getPoint.CommandResult() != global::Rhino.Commands.Result.Success)
            {
                RhinoApp.WriteLine("Insert cancelled by user");
                return;
            }
            
            var insertionPoint = getPoint.Point();
            RhinoApp.WriteLine($"Insertion point: {insertionPoint}");
            
            // Insert tool mesh WITH holder transform
            // CONCEPT: Tool is positioned for reference holder (Tego).
            //          Transform repositions tool to fit selected holder.
            //          Holder itself inserts WITHOUT transform (stationary).
            RhinoApp.WriteLine($"Inserting tool mesh: {toolMeshPath}");
            if (holder != null)
            {
                RhinoApp.WriteLine($"Tool will be transformed to fit holder: {holder.Variant}");
            }
            InsertFile3dmAtPoint(toolMeshPath, insertionPoint, product, holder, applyHolderTransform: true);
            
            // If holder selected, insert holder 3dm at insertion point WITHOUT transform
            // (holder is stationary, tool moves to fit it)
            if (holder != null && !string.IsNullOrEmpty(holder.FullPath))
            {
                // Try to resolve UNC path to mapped drive if needed
                var holderPath = ResolveFilePath(holder.FullPath);
                
                if (System.IO.File.Exists(holderPath))
                {
                    RhinoApp.WriteLine($"Inserting holder (stationary, no transform): {holderPath}");
                    InsertFile3dmAtPoint(holderPath, insertionPoint, product, null, applyHolderTransform: false);
                }
                else
                {
                    RhinoApp.WriteLine($"✗ WARNING: Holder file not found: {holderPath}");
                    RhinoApp.WriteLine($"  Original path: {holder.FullPath}");
                    RhinoApp.WriteLine($"  Resolved path: {holderPath}");
                    RhinoApp.WriteLine($"  Check if file exists at resolved location");
                }
            }
            
            // If packaging requested, insert it offset to the side (NO holder transform!)
            if (includePackaging && product.Packaging != null && !string.IsNullOrEmpty(product.Packaging.FullPath))
            {
                var packagingPath = ResolveFilePath(product.Packaging.FullPath);
                
                if (System.IO.File.Exists(packagingPath))
                {
                    // Offset packaging to the right by 500mm
                    var packagingPoint = new global::Rhino.Geometry.Point3d(
                        insertionPoint.X + 500,
                        insertionPoint.Y,
                        insertionPoint.Z
                    );
                    
                    RhinoApp.WriteLine($"Inserting packaging at offset: {packagingPath}");
                    InsertFile3dmAtPoint(packagingPath, packagingPoint, product, null, applyHolderTransform: false);
                }
                else
                {
                    RhinoApp.WriteLine($"✗ WARNING: Packaging file not found: {packagingPath}");
                }
            }
            
            UpdateStatus($"Inserted: {product.ProductName}" + 
                        (holder != null ? $" with {holder.Variant} - {holder.Color}" : "") +
                        (includePackaging ? " + Packaging" : ""));
            RhinoApp.WriteLine($"=== INSERT COMPLETE ===");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error inserting product: {ex.Message}");
            RhinoApp.WriteLine($"ERROR in InsertProductWithHolder: {ex.Message}");
            RhinoApp.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
    
    private void OnProductInsertRequested(object? sender, ProductCardEventArgs e)
    {
        // Use holder key from event args (format: "Variant_Color" from list view dropdown)
        var holderKey = e.SelectedHolderVariant;
        
        // Find the holder object by variant_color key
        Holder? selectedHolder = null;
        if (!string.IsNullOrEmpty(holderKey) && e.Product.Holders != null)
        {
            // Parse key: "Traverse_RAL9006" -> Variant="Traverse", Color="RAL9006"
            var parts = holderKey.Split('_', 2);
            if (parts.Length == 2)
            {
                var variant = parts[0];
                var color = parts[1];
                
                selectedHolder = e.Product.Holders.FirstOrDefault(h => 
                    h.Variant?.Equals(variant, StringComparison.OrdinalIgnoreCase) == true &&
                    h.Color?.Equals(color, StringComparison.OrdinalIgnoreCase) == true);
                
                if (selectedHolder != null)
                {
                    RhinoApp.WriteLine($"✓ Found holder: Variant={selectedHolder.Variant}, Color={selectedHolder.Color}, FullPath={selectedHolder.FullPath}");
                }
                else
                {
                    RhinoApp.WriteLine($"✗ Holder not found in product.Holders for key: {holderKey} (Variant={variant}, Color={color})");
                }
            }
            else
            {
                // Fallback: try just variant (backward compatibility)
                selectedHolder = e.Product.Holders.FirstOrDefault(h => 
                    h.Variant?.Equals(holderKey, StringComparison.OrdinalIgnoreCase) == true);
                    
                if (selectedHolder != null)
                {
                    RhinoApp.WriteLine($"✓ Found holder (variant-only match): Variant={selectedHolder.Variant}, Color={selectedHolder.Color}");
                }
                else
                {
                    RhinoApp.WriteLine($"✗ Holder not found for: {holderKey}");
                }
            }
        }
        
        RhinoApp.WriteLine($"Insert requested - Holder: {(selectedHolder != null ? $"{selectedHolder.Variant} - {selectedHolder.Color}" : "None")}, Packaging: {e.IncludePackaging}");
        
        InsertProductWithHolder(e.Product, selectedHolder, e.IncludePackaging);
    }
    
    private string ResolveFilePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        
        // Convert forward slashes to backslashes for Windows
        path = path.Replace('/', '\\');
        
        // Convert UNC path to mapped drive if applicable
        if (path.StartsWith(@"\\MattHQ-SVDC01\Share\"))
        {
            path = path.Replace(@"\\MattHQ-SVDC01\Share\", "M:\\");
        }
        
        return path;
    }
    
    private void InsertFile3dmAtPoint(string file3dmPath, global::Rhino.Geometry.Point3d insertPoint, Product product, Holder? holder, bool applyHolderTransform = true)
    {
        try
        {
            RhinoApp.WriteLine($"Inserting file: {file3dmPath} at {insertPoint}");
            
            // Insert the file as linked instance
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                UpdateStatus("No active Rhino document");
                return;
            }
            
            // Get insert settings FIRST (needed for linked block creation)
            var settings = _settingsService?.GetSettings();
            
            // Read the file and insert as instance
            var instanceDefs = doc.InstanceDefinitions;
            
            // Create a unique instance definition name based on ACTUAL file name
            // This ensures tool, holder, and packaging each get unique block definitions
            var fileName = System.IO.Path.GetFileNameWithoutExtension(file3dmPath);
            
            // CRITICAL: Block name MUST include the actual filename to avoid collisions!
            // For holder: "Traverse_RAL9006_NN.ALL.BO07803"  
            // For tool: "GBL 18V-750_Mesh_Tego" + "_Traverse_RAL9006"
            // For packaging: "GBL 18V-750_packaging"
            var suffix = holder != null ? $"_{holder.Variant}_{holder.Color}" : "";
            var defName = $"{fileName}{suffix}"; // SIMPLIFIED: Just use filename + holder suffix
            
            // DEBUG: Log block naming details
            RhinoApp.WriteLine($"=== BLOCK NAMING DEBUG ===");
            RhinoApp.WriteLine($"  file3dmPath: {file3dmPath}");
            RhinoApp.WriteLine($"  fileName (from path): {fileName}");
            RhinoApp.WriteLine($"  holder param: {(holder != null ? $"{holder.Variant}-{holder.Color}" : "null")}");
            RhinoApp.WriteLine($"  suffix: '{suffix}'");
            RhinoApp.WriteLine($"  defName (FINAL): {defName}");
            
            // Check if definition already exists
            var existingDef = instanceDefs.Find(defName);
            int defIndex = existingDef != null ? existingDef.Index : -1;
            if (defIndex < 0)
            {
                // Read the 3DM file and create instance definition
                var geometry = new List<global::Rhino.Geometry.GeometryBase>();
                var attributes = new List<global::Rhino.DocObjects.ObjectAttributes>();
                
                // Read file content
                var file = global::Rhino.FileIO.File3dm.Read(file3dmPath);
                if (file != null)
                {
                    // Import materials from source file to current document
                    var materialMap = new Dictionary<int, int>(); // source index -> doc index
                    
                    foreach (var mat in file.AllMaterials)
                    {
                        // Check if material already exists in doc
                        var existingMatIndex = doc.Materials.Find(mat.Name, true);
                        if (existingMatIndex < 0)
                        {
                            // Add new material
                            existingMatIndex = doc.Materials.Add(mat);
                        }
                        materialMap[mat.Index] = existingMatIndex;
                    }
                    
                    foreach (var obj in file.Objects)
                    {
                        if (obj.Geometry != null)
                        {
                            // Duplicate geometry to break reference to file
                            var geomCopy = obj.Geometry.Duplicate();
                            if (geomCopy != null)
                            {
                                geometry.Add(geomCopy);
                                
                                // Duplicate attributes and remap material index
                                var attrCopy = obj.Attributes.Duplicate();
                                
                                // Remap material index if object has a material assigned
                                if (attrCopy.MaterialSource == global::Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject)
                                {
                                    if (materialMap.ContainsKey(attrCopy.MaterialIndex))
                                    {
                                        attrCopy.MaterialIndex = materialMap[attrCopy.MaterialIndex];
                                    }
                                }
                                
                                attributes.Add(attrCopy);
                            }
                        }
                    }
                    
                    // Create instance definition from geometry
                    if (geometry.Count > 0)
                    {
                        // Create embedded block definition
                        defIndex = instanceDefs.Add(defName, string.Empty, global::Rhino.Geometry.Point3d.Origin, geometry, attributes);
                        RhinoApp.WriteLine($"✓ Created instance definition '{defName}' with {geometry.Count} objects");
                        
                        // Make it a linked block if user selected "Linked" in settings
                        if (settings?.InsertBlockType == "Linked")
                        {
                            try
                            {
                                // Convert to linked block by setting source archive
                                var updateType = global::Rhino.DocObjects.InstanceDefinitionUpdateType.LinkedAndEmbedded;
                                bool success = instanceDefs.ModifySourceArchive(defIndex, file3dmPath, updateType, quiet: true);
                                if (success)
                                {
                                    RhinoApp.WriteLine($"  ✓ Converted to LINKED block (source: {file3dmPath})");
                                }
                                else
                                {
                                    RhinoApp.WriteLine($"  ⚠ Failed to link block to source file");
                                }
                            }
                            catch (Exception linkEx)
                            {
                                RhinoApp.WriteLine($"  ⚠ Link block error: {linkEx.Message}");
                            }
                        }
                    }
                    
                    // Now safe to dispose
                    file.Dispose();
                }
            }
            
            if (defIndex >= 0)
            {
                // Settings already fetched at beginning of function
                var insertAsGroup = settings?.InsertAs == "Group";
                
                // Calculate transform (insertion point + holder transform if requested)
                var transform = applyHolderTransform 
                    ? CalculateInsertTransform(product, holder, insertPoint)
                    : global::Rhino.Geometry.Transform.Translation(insertPoint.X, insertPoint.Y, insertPoint.Z);
                
                if (insertAsGroup)
                {
                    // Insert as group - explode the block and group the objects
                    RhinoApp.WriteLine($"Inserting as GROUP (exploding block definition)");
                    
                    // Get the instance definition
                    var idef = doc.InstanceDefinitions[defIndex];
                    if (idef != null)
                    {
                        var explodedIds = new List<Guid>();
                        var defObjects = idef.GetObjects();
                        
                        if (defObjects == null || defObjects.Length == 0)
                        {
                            RhinoApp.WriteLine($"ERROR: Block definition has no objects");
                        }
                        else
                        {
                            RhinoApp.WriteLine($"Block definition contains {defObjects.Length} objects");
                        
                            // Get geometry from definition and add to doc
                            int successCount = 0;
                            foreach (var defObj in defObjects)
                            {
                                var geom = defObj.Geometry?.Duplicate();
                                var attr = defObj.Attributes.Duplicate();
                                
                                if (geom != null)
                                {
                                    // Apply transform (move to insertion point)
                                    geom.Transform(transform);
                                    
                                    // Add to document with attributes (includes material)
                                    var newId = doc.Objects.Add(geom, attr);
                                    if (newId != Guid.Empty)
                                    {
                                        explodedIds.Add(newId);
                                        successCount++;
                                        
                                        // Add metadata to each object
                                        var newObj = doc.Objects.FindId(newId);
                                        if (newObj != null)
                                        {
                                            newObj.Attributes.SetUserString("BoschProductId", product.Id);
                                            newObj.Attributes.SetUserString("BoschProductName", product.ProductName);
                                            newObj.Attributes.SetUserString("BoschProductSKU", product.Sku ?? "");
                                            newObj.Attributes.SetUserString("BoschProductFile", file3dmPath);
                                            if (holder != null)
                                            {
                                                newObj.Attributes.SetUserString("BoschHolderVariant", holder.Variant);
                                                newObj.Attributes.SetUserString("BoschHolderColor", holder.Color);
                                            }
                                            newObj.CommitChanges();
                                        }
                                    }
                                    else
                                    {
                                        RhinoApp.WriteLine($"WARNING: Failed to add object to document (type: {geom.ObjectType})");
                                    }
                                }
                                else
                                {
                                    RhinoApp.WriteLine($"WARNING: Object has no geometry");
                                }
                            }
                            
                            RhinoApp.WriteLine($"Successfully added {successCount} of {defObjects.Length} objects to document");
                            
                            // Create a group with the exploded objects
                            if (explodedIds.Count > 0)
                            {
                                var groupName = holder != null 
                                    ? $"{product.ProductName}_{holder.Variant}_{DateTime.Now.Ticks}"
                                    : $"{product.ProductName}_{DateTime.Now.Ticks}";
                                var groupIndex = doc.Groups.Add(groupName);
                                foreach (var id in explodedIds)
                                {
                                    doc.Groups.AddToGroup(groupIndex, id);
                                }
                                RhinoApp.WriteLine($"✓ Created group '{groupName}' with {explodedIds.Count} objects");
                            }
                            else
                            {
                                RhinoApp.WriteLine($"ERROR: No objects were added to create group");
                            }
                        }
                    }
                    else
                    {
                        RhinoApp.WriteLine($"ERROR: Could not find instance definition at index {defIndex}");
                    }
                }
                else
                {
                    // Insert as block instance (default)
                    RhinoApp.WriteLine($"Inserting as BLOCK INSTANCE");
                    
                    var objId = doc.Objects.AddInstanceObject(defIndex, transform);
                    
                    if (objId != Guid.Empty)
                    {
                        // Store product metadata in object user data
                        var obj = doc.Objects.FindId(objId);
                        if (obj != null)
                        {
                            obj.Attributes.SetUserString("BoschProductId", product.Id);
                            obj.Attributes.SetUserString("BoschProductName", product.ProductName);
                            obj.Attributes.SetUserString("BoschProductSKU", product.Sku ?? "");
                            obj.Attributes.SetUserString("BoschProductFile", file3dmPath);
                            if (holder != null)
                            {
                                obj.Attributes.SetUserString("BoschHolderVariant", holder.Variant);
                                obj.Attributes.SetUserString("BoschHolderColor", holder.Color);
                                obj.Attributes.SetUserString("BoschHolderCod", holder.CodArticol);
                            }
                            obj.CommitChanges();
                        }
                    }
                }
                
                doc.Views.Redraw();
                var holderInfo = holder != null ? $" with {holder.Variant} - {holder.Color}" : "";
                UpdateStatus($"Inserted: {product.ProductName}{holderInfo}");
                RhinoApp.WriteLine($"SUCCESS: Inserted {product.ProductName}{holderInfo} at origin");
            }
            else
            {
                UpdateStatus($"Failed to insert: {product.ProductName}");
                RhinoApp.WriteLine($"ERROR: Failed to create instance definition");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error inserting file: {ex.Message}");
            RhinoApp.WriteLine($"ERROR in InsertFile3dm: {ex.Message}");
            RhinoApp.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
    
    private string? Find3DMFile(Product product, Holder? holder = null)
    {
        RhinoApp.WriteLine($"=== Find3DMFile: Product={product.ProductName}, Holder={holder?.Variant ?? "NONE"} ===");
        
        // STRATEGY 1: Check if transforms are defined - if yes, use reference mesh
        // STRATEGY 2: Fallback to holder-specific mesh files (current behavior)
        
        var targetHolder = holder?.Variant ?? product.ReferenceHolder ?? "Tego";
        
        // Check if product has transform data and holder-specific transform exists
        if (product.HolderTransforms != null && product.HolderTransforms.ContainsKey(targetHolder))
        {
            RhinoApp.WriteLine($"✓ Product has HolderTransforms defined");
            
            // Use reference mesh (typically the one at Tego position)
            var referenceMesh = FindReferenceMesh(product);
            if (referenceMesh != null)
            {
                RhinoApp.WriteLine($"✓ Will use REFERENCE mesh with transform: {referenceMesh}");
                RhinoApp.WriteLine($"  Transform for '{targetHolder}': T={string.Join(",", product.HolderTransforms[targetHolder].Translation)}, R={string.Join(",", product.HolderTransforms[targetHolder].Rotation)}");
                return referenceMesh;
            }
            else
            {
                RhinoApp.WriteLine($"⚠ Transforms defined but reference mesh not found, falling back to file naming");
            }
        }
        else
        {
            RhinoApp.WriteLine($"ℹ No HolderTransforms defined, using file naming strategy");
        }
        
        // FALLBACK: Use holder-specific mesh files (original behavior)
        if (!string.IsNullOrEmpty(product.FolderPath) && System.IO.Directory.Exists(product.FolderPath))
        {
            RhinoApp.WriteLine($"Searching for mesh variant: {targetHolder} in: {product.FolderPath}");
            
            // Try all case variations for holder-specific mesh
            var patterns = new[]
            {
                $"{product.ProductName}_Mesh_{targetHolder}.3dm",
                $"{product.ProductName}_mesh_{targetHolder}.3dm",
                $"{product.ProductName}_MESH_{targetHolder}.3dm",
                $"{product.ProductName}_Mesh_{targetHolder.ToUpper()}.3dm",
                $"{product.ProductName}_mesh_{targetHolder.ToLower()}.3dm"
            };
            
            foreach (var pattern in patterns)
            {
                var holderSpecificMesh = System.IO.Path.Combine(product.FolderPath, pattern);
                if (System.IO.File.Exists(holderSpecificMesh))
                {
                    if (holder == null)
                    {
                        RhinoApp.WriteLine($"✓ Found DEFAULT Tego mesh (no holder selected): {holderSpecificMesh}");
                    }
                    else
                    {
                        RhinoApp.WriteLine($"✓ Found holder-specific mesh for {holder.Variant}: {holderSpecificMesh}");
                    }
                    return holderSpecificMesh;
                }
                else
                {
                    RhinoApp.WriteLine($"  × Not found: {pattern}");
                }
            }
            
            // If Tego mesh not found and no holder was selected, try generic mesh as fallback
            if (holder == null)
            {
                RhinoApp.WriteLine($"WARNING: Tego mesh not found, falling back to generic mesh");
                
                // Try generic mesh patterns
                var genericPatterns = new[]
                {
                    $"{product.ProductName}_mesh.3dm",
                    $"{product.ProductName}_Mesh.3dm"
                };
                
                foreach (var pattern in genericPatterns)
                {
                    var genericMesh = System.IO.Path.Combine(product.FolderPath, pattern);
                    if (System.IO.File.Exists(genericMesh))
                    {
                        RhinoApp.WriteLine($"✓ Found generic tool mesh (fallback): {genericMesh}");
                        return genericMesh;
                    }
                }
            }
            else
            {
                RhinoApp.WriteLine($"WARNING: No mesh found for holder {holder.Variant}");
            }
        }
        
        // Final fallback: Use mesh3d path from JSON
        if (product.Previews?.Mesh3d?.FullPath != null && System.IO.File.Exists(product.Previews.Mesh3d.FullPath))
        {
            RhinoApp.WriteLine($"✓ Using mesh from JSON (last resort): {product.Previews.Mesh3d.FullPath}");
            return product.Previews.Mesh3d.FullPath;
        }
        
        RhinoApp.WriteLine($"✗ ERROR: No tool mesh found for {product.ProductName}" + (holder != null ? $" with holder {holder.Variant}" : " (expected Tego default)"));
        return null;
    }
    
    private string? FindReferenceMesh(Product product)
    {
        // Reference mesh is typically the mesh at the reference holder position
        // Usually stored in mesh3d.fullPath or as {ProductName}_mesh.3dm
        var referenceHolder = product.ReferenceHolder ?? "Tego";
        
        if (!string.IsNullOrEmpty(product.FolderPath) && System.IO.Directory.Exists(product.FolderPath))
        {
            // Try reference holder mesh first
            var patterns = new[]
            {
                $"{product.ProductName}_Mesh_{referenceHolder}.3dm",
                $"{product.ProductName}_mesh_{referenceHolder}.3dm",
                $"{product.ProductName}_mesh.3dm",
                $"{product.ProductName}_Mesh.3dm"
            };
            
            foreach (var pattern in patterns)
            {
                var meshPath = System.IO.Path.Combine(product.FolderPath, pattern);
                if (System.IO.File.Exists(meshPath))
                {
                    return meshPath;
                }
            }
        }
        
        // Fallback to mesh3d from JSON
        if (product.Previews?.Mesh3d?.FullPath != null && System.IO.File.Exists(product.Previews.Mesh3d.FullPath))
        {
            return product.Previews.Mesh3d.FullPath;
        }
        
        return null;
    }
    
    private global::Rhino.Geometry.Transform CalculateInsertTransform(Product product, Holder? holder, global::Rhino.Geometry.Point3d insertPoint)
    {
        // Start with translation to insertion point
        var transform = global::Rhino.Geometry.Transform.Translation(insertPoint.X, insertPoint.Y, insertPoint.Z);
        
        // If holder transforms are defined, apply the holder-specific transform
        // DEBUG: Log what we're receiving
        RhinoApp.WriteLine($"=== CalculateInsertTransform DEBUG ===");
        RhinoApp.WriteLine($"  holder parameter: {(holder != null ? $"Variant={holder.Variant}, Color={holder.Color}" : "NULL")}");
        RhinoApp.WriteLine($"  product.ReferenceHolder: {product.ReferenceHolder ?? "NULL"}");
        
        var holderVariant = holder?.Variant ?? product.ReferenceHolder ?? "Tego";
        
        RhinoApp.WriteLine($"  Resolved holderVariant: '{holderVariant}'");
        RhinoApp.WriteLine($"  product.HolderTransforms exists: {product.HolderTransforms != null}");
        if (product.HolderTransforms != null)
        {
            RhinoApp.WriteLine($"  Available transform keys: {string.Join(", ", product.HolderTransforms.Keys)}");
            RhinoApp.WriteLine($"  Contains key '{holderVariant}': {product.HolderTransforms.ContainsKey(holderVariant)}");
        }
        
        if (product.HolderTransforms != null && product.HolderTransforms.ContainsKey(holderVariant))
        {
            var holderTransform = product.HolderTransforms[holderVariant];
            
            RhinoApp.WriteLine($"✓ Applying holder transform for '{holderVariant}':");
            RhinoApp.WriteLine($"  Translation: [{holderTransform.Translation[0]}, {holderTransform.Translation[1]}, {holderTransform.Translation[2]}]");
            RhinoApp.WriteLine($"  Rotation: [{holderTransform.Rotation[0]}, {holderTransform.Rotation[1]}, {holderTransform.Rotation[2]}] degrees");
            RhinoApp.WriteLine($"  Scale: [{holderTransform.Scale[0]}, {holderTransform.Scale[1]}, {holderTransform.Scale[2]}]");
            
            // Apply transforms in order: Scale -> Rotate -> Translate
            
            // 1. Scale (if not uniform 1,1,1)
            if (holderTransform.Scale[0] != 1.0 || holderTransform.Scale[1] != 1.0 || holderTransform.Scale[2] != 1.0)
            {
                var scaleTransform = global::Rhino.Geometry.Transform.Scale(
                    global::Rhino.Geometry.Plane.WorldXY,
                    holderTransform.Scale[0],
                    holderTransform.Scale[1],
                    holderTransform.Scale[2]
                );
                transform = scaleTransform * transform;
            }
            
            // 2. Rotations (X, Y, Z in degrees)
            // Convert degrees to radians
            var rotX = holderTransform.Rotation[0] * Math.PI / 180.0;
            var rotY = holderTransform.Rotation[1] * Math.PI / 180.0;
            var rotZ = holderTransform.Rotation[2] * Math.PI / 180.0;
            
            if (rotX != 0.0)
            {
                var rotXTransform = global::Rhino.Geometry.Transform.Rotation(rotX, global::Rhino.Geometry.Vector3d.XAxis, global::Rhino.Geometry.Point3d.Origin);
                transform = rotXTransform * transform;
            }
            if (rotY != 0.0)
            {
                var rotYTransform = global::Rhino.Geometry.Transform.Rotation(rotY, global::Rhino.Geometry.Vector3d.YAxis, global::Rhino.Geometry.Point3d.Origin);
                transform = rotYTransform * transform;
            }
            if (rotZ != 0.0)
            {
                var rotZTransform = global::Rhino.Geometry.Transform.Rotation(rotZ, global::Rhino.Geometry.Vector3d.ZAxis, global::Rhino.Geometry.Point3d.Origin);
                transform = rotZTransform * transform;
            }
            
            // 3. Translation (holder-specific offset)
            var holderTranslation = global::Rhino.Geometry.Transform.Translation(
                holderTransform.Translation[0],
                holderTransform.Translation[1],
                holderTransform.Translation[2]
            );
            transform = holderTranslation * transform;
        }
        else
        {
            RhinoApp.WriteLine($"ℹ No holder transform defined for '{holderVariant}', using identity");
        }
        
        return transform;
    }
    
    private void ShowPreviewModal(Product product)
    {
        // Create preview modal dialog
        var dialog = new Dialog<bool>
        {
            Title = product.ProductName,
            MinimumSize = new Size(700, 550),
            Padding = 0
        };
        
        // Left side: Thumbnail (400x400)
        var thumbnailImage = new ImageView
        {
            Width = 400,
            Height = 400
        };
        
        // Try to load thumbnail from existing preview images
        LoadProductThumbnailAsync(product, thumbnailImage, ThumbnailSize.Large);
        
        var thumbnailContainer = new Panel
        {
            Width = 420,
            Padding = 10,
            Content = new Panel
            {
                BackgroundColor = Color.FromArgb(245, 245, 245),
                Content = thumbnailImage
            }
        };
        
        // Right side: Product details
        var detailsStack = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Padding = new Padding(20),
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };
        
        // Product name (header)
        detailsStack.Items.Add(new Label
        {
            Text = product.ProductName,
            Font = new Font(SystemFont.Bold, 16)
        });
        
        // SKU
        detailsStack.Items.Add(new Label
        {
            Text = $"SKU: {product.Sku ?? "N/A"}",
            Font = new Font(SystemFont.Default, 10),
            TextColor = Color.FromArgb(120, 120, 120)
        });
        
        // Separator
        detailsStack.Items.Add(new Panel { Height = 1, BackgroundColor = Color.FromArgb(220, 220, 220) });
        
        // Description
        if (!string.IsNullOrEmpty(product.Description))
        {
            detailsStack.Items.Add(new Label
            {
                Text = product.Description,
                Font = new Font(SystemFont.Default, 11),
                Wrap = WrapMode.Word
            });
        }
        
        // Category
        if (!string.IsNullOrEmpty(product.Category))
        {
            detailsStack.Items.Add(new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Label { Text = "Category:", Font = SystemFonts.Bold() },
                    new Label { Text = product.Category }
                }
            });
        }
        
        // Range
        if (!string.IsNullOrEmpty(product.Range))
        {
            detailsStack.Items.Add(new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new Label { Text = "Range:", Font = SystemFonts.Bold() },
                    new Label { Text = product.Range }
                }
            });
        }
        
        // Tags
        if (product.Tags != null && product.Tags.Any())
        {
            var tagsText = string.Join(", ", product.Tags.Select(t => $"#{t}"));
            detailsStack.Items.Add(new Label
            {
                Text = tagsText,
                Font = new Font(SystemFont.Default, 10),
                TextColor = Color.FromArgb(70, 100, 200),
                Wrap = WrapMode.Word
            });
        }
        
        // Holder selection
        Holder? selectedHolder = null;
        if (product.Holders != null && product.Holders.Any())
        {
            detailsStack.Items.Add(new Panel { Height = 1, BackgroundColor = Color.FromArgb(220, 220, 220) });
            
            detailsStack.Items.Add(new Label
            {
                Text = "Holder Options:",
                Font = SystemFonts.Bold()
            });
            
            var holderLabel = new Label
            {
                Text = "No Holder",
                Font = new Font(SystemFont.Default, 11)
            };
            
            var holderOptions = new List<Holder?> { null }; // null = no holder
            holderOptions.AddRange(product.Holders);
            int currentHolderIndex = 1; // Default to first holder (usually Tego)
            
            void UpdateHolderLabel()
            {
                var holder = holderOptions[currentHolderIndex];
                if (holder == null)
                {
                    holderLabel.Text = "✓ No Holder";
                    selectedHolder = null;
                }
                else
                {
                    holderLabel.Text = $"✓ {holder.Variant} - {holder.Color} ({holder.CodArticol})";
                    selectedHolder = holder;
                }
            }
            
            var prevButton = new Button { Text = "◀ Previous", Width = 90 };
            var nextButton = new Button { Text = "Next ▶", Width = 90 };
            
            prevButton.Click += (s, e) =>
            {
                currentHolderIndex = (currentHolderIndex - 1 + holderOptions.Count) % holderOptions.Count;
                UpdateHolderLabel();
            };
            
            nextButton.Click += (s, e) =>
            {
                currentHolderIndex = (currentHolderIndex + 1) % holderOptions.Count;
                UpdateHolderLabel();
            };
            
            UpdateHolderLabel();
            
            detailsStack.Items.Add(holderLabel);
            detailsStack.Items.Add(new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Items = { prevButton, nextButton }
            });
        }
        
        // Packaging checkbox
        var includePackaging = false;
        if (product.Packaging != null && !string.IsNullOrEmpty(product.Packaging.FullPath))
        {
            var packagingCheckbox = new CheckBox
            {
                Text = "Include Packaging",
                Checked = false
            };
            packagingCheckbox.CheckedChanged += (s, e) =>
            {
                includePackaging = packagingCheckbox.Checked == true;
            };
            
            detailsStack.Items.Add(packagingCheckbox);
        }
        
        // Spacer
        detailsStack.Items.Add(new StackLayoutItem(null, true));
        
        // Buttons at bottom
        var insertBtn = new Button
        {
            Text = "Insert",
            Width = 100
        };
        insertBtn.Click += (s, e) =>
        {
            dialog.Close(true);
            InsertProductWithHolder(product, selectedHolder, includePackaging);
        };
        
        var closeBtn = new Button
        {
            Text = "Close",
            Width = 100
        };
        closeBtn.Click += (s, e) => dialog.Close(false);
        
        var buttonLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalContentAlignment = HorizontalAlignment.Right,
            Items = { insertBtn, closeBtn }
        };
        
        detailsStack.Items.Add(buttonLayout);
        
        // Main layout: Two columns (thumbnail | details)
        var mainLayout = new TableLayout
        {
            Spacing = new Size(0, 0),
            Rows =
            {
                new TableRow(
                    new TableCell(thumbnailContainer, false),
                    new TableCell(detailsStack, true)
                )
            }
        };
        
        dialog.Content = mainLayout;
        dialog.ShowModal(this);
    }
    
    private async void LoadProductThumbnailAsync(Product product, ImageView imageView, ThumbnailSize size)
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
                        LoadFromThumbnailService();
                    }
                });
            }
            else
            {
                LoadFromThumbnailService();
            }
            
            async void LoadFromThumbnailService()
            {
                if (_thumbnailService != null)
                {
                    var thumbPath = await _thumbnailService.GetThumbnailPathAsync(
                        product.Id,
                        size,
                        System.Threading.CancellationToken.None
                    );
                    
                    if (!string.IsNullOrEmpty(thumbPath) && System.IO.File.Exists(thumbPath))
                    {
                        Application.Instance.AsyncInvoke(() =>
                        {
                            try
                            {
                                imageView.Image = new Bitmap(thumbPath);
                            }
                            catch { }
                        });
                    }
                }
            }
        }
        catch
        {
            // Thumbnail loading failed, keep placeholder
        }
    }
    
    private void OnFavouriteToggled(object? sender, ProductActionEventArgs e)
    {
        if (_userDataService == null) return;
        
        // Toggle favourite status
        if (_userDataService.IsFavourite(e.Product.Id))
        {
            _userDataService.RemoveFavourite(e.Product.Id);
            UpdateStatus($"Removed from favourites: {e.Product.ProductName}");
        }
        else
        {
            _userDataService.AddFavouriteAsync(e.Product.Id);
            UpdateStatus($"Added to favourites: {e.Product.ProductName}");
        }
        
        // Save changes
        _ = _userDataService.SaveAsync();
    }
    
    private void OnHolderSelectionChanged(object? sender, HolderSelectionEventArgs e)
    {
        UpdateStatus($"Holder selection: {e.SelectedVariant ?? "None"} / {e.SelectedColor ?? "Default"}");
    }
    
    private void OnViewModeToggled(object? sender, EventArgs e)
    {
        // Toggle between Grid and List view
        _isGridView = !_isGridView;
        
        if (_viewModeButton != null && _productViewContainer != null)
        {
            if (_isGridView)
            {
                // Switch to Grid View
                _viewModeButton.Text = "Grid View";
                _productViewContainer.Content = _productCardGrid;
                
                // Reload products in grid
                if (_productCardGrid != null)
                {
                    _productCardGrid.LoadProducts(_filteredProducts);
                    _productCardGrid.SetMultiSelectMode(_multiSelectMode);
                }
                
                UpdateStatus("Switched to Grid view");
            }
            else
            {
                // Switch to List View
                _viewModeButton.Text = "List View";
                _productViewContainer.Content = _productListView;
                
                // Reload products in list
                if (_productListView != null)
                {
                    _productListView.LoadProducts(_filteredProducts);
                    _productListView.MultiSelectMode = _multiSelectMode;
                }
                
                UpdateStatus("Switched to List view");
            }
        }
    }
    
    private void OnMultiSelectToggled(object? sender, EventArgs e)
    {
        _multiSelectMode = !_multiSelectMode;
        
        if (_multiSelectButton != null)
        {
            _multiSelectButton.Text = _multiSelectMode ? "✓ Multi-Select" : "Multi-Select";
        }
        
        // Apply to current view (grid or list)
        if (_isGridView && _productCardGrid != null)
        {
            _productCardGrid.SetMultiSelectMode(_multiSelectMode);
        }
        else if (!_isGridView && _productListView != null)
        {
            _productListView.MultiSelectMode = _multiSelectMode;
            // Refresh list view to show/hide checkboxes
            _productListView.LoadProducts(_filteredProducts);
        }
        
        UpdateStatus(_multiSelectMode ? "Multi-select mode enabled - Select products for batch operations" : "Multi-select mode disabled");
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_refreshButton != null)
            {
                _refreshButton.Enabled = false;
                _refreshButton.Text = "Refreshing...";
            }

            UpdateStatus("Refreshing product database...");

            if (_dataService != null)
            {
                await _dataService.ReloadAsync();
                _allProducts = _dataService.GetProducts();
                ApplySearch();
            }

            UpdateStatus("Products refreshed successfully");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error refreshing: {ex.Message}");
        }
        finally
        {
            if (_refreshButton != null)
            {
                _refreshButton.Enabled = true;
                _refreshButton.Text = "Refresh";
            }
        }
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        if (_settingsService == null)
        {
            UpdateStatus("Settings service not initialized");
            return;
        }
        
        var dialog = new Dialogs.SettingsDialog(_settingsService);
        var result = dialog.ShowModal(this);
        
        if (result)
        {
            // Settings saved, reload data
            UpdateStatus("Settings saved. Reloading database...");
            _ = Task.Run(async () =>
            {
                if (_dataService != null)
                {
                    await _dataService.ReloadAsync();
                    Application.Instance.AsyncInvoke(() =>
                    {
                        _allProducts = _dataService.GetProducts();
                        ApplySearch();
                        UpdateStatus("Database reloaded successfully");
                    });
                }
            });
        }
    }

    private void UpdateStatus(string message)
    {
        if (_statusLabel != null)
        {
            Application.Instance.Invoke(() =>
            {
                _statusLabel.Text = message;
            });
        }
    }
}
