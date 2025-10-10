using Eto.Drawing;
using Eto.Forms;
using Rhino;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;
using BoschMediaBrowser.Rhino.UI.Controls;
using BoschMediaBrowser.Rhino.UI.Views;

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
    private CategoryTree? _categoryTree;
    private ThumbnailGrid? _thumbnailGrid;
    private FiltersBar? _filtersBar;
    private DetailPane? _detailPane;
    
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
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var statusBar = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Padding = 8,
            Spacing = 8,
            BackgroundColor = Colors.Black,
            Items = 
            { 
                _statusLabel
            }
        };

        // Main layout
        Content = new TableLayout
        {
            Padding = 0,
            Spacing = new Size(0, 0),
            Rows =
            {
                toolbar,
                new TableRow(_tabs) { ScaleHeight = true },
                statusBar
            }
        };
    }

    private Control CreateBrowseTab()
    {
        if (_searchService == null || _thumbnailService == null || _userDataService == null || _settingsService == null)
        {
            return new Label { Text = "Services not initialized" };
        }
        
        // Left sidebar: Category Tree (250px wide)
        _categoryTree = new CategoryTree(_searchService);
        _categoryTree.CategorySelected += OnCategorySelected;
        
        var categoryPanel = new Panel
        {
            Content = _categoryTree,
            MinimumSize = new Size(200, 400),
            Size = new Size(250, -1),
            Padding = 5
        };

        // Center: Filters Bar + Thumbnail Grid
        _filtersBar = new FiltersBar(_searchService);
        _filtersBar.FiltersChanged += OnFiltersChanged;
        
        _thumbnailGrid = new ThumbnailGrid(_thumbnailService, _userDataService);
        _thumbnailGrid.ProductSelected += OnProductSelectedInGrid;
        _thumbnailGrid.MultipleProductsSelected += OnMultipleProductsSelected;

        var centerLayout = new TableLayout
        {
            Spacing = new Size(0, 5),
            Rows =
            {
                _filtersBar,
                new TableRow(_thumbnailGrid) { ScaleHeight = true }
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
            var count = await _dataService.LoadProductsAsync();
            LogToFile($"InitializeAsync: Products loaded, count={count}");
            
            LogToFile("InitializeAsync: Getting products list...");
            _allProducts = _dataService.GetProducts();
            _filteredProducts = _allProducts;
            LogToFile($"InitializeAsync: Product lists assigned, _allProducts.Count={_allProducts.Count}");

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
                    LogToFile($"InitializeAsync UI Update: Loading {_allProducts.Count} products into CategoryTree...");
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    
                    if (_categoryTree != null)
                    {
                        _categoryTree.LoadProducts(_allProducts);
                        sw.Stop();
                        LogToFile($"InitializeAsync UI Update: CategoryTree.LoadProducts() took {sw.ElapsedMilliseconds}ms");
                    }
                    
                    LogToFile($"InitializeAsync UI Update: Loading {_filteredProducts.Count} products into ThumbnailGrid...");
                    sw.Restart();
                    
                    if (_thumbnailGrid != null)
                    {
                        _thumbnailGrid.LoadProducts(_filteredProducts);
                        sw.Stop();
                        LogToFile($"InitializeAsync UI Update: ThumbnailGrid.LoadProducts() took {sw.ElapsedMilliseconds}ms");
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
        
        // Update thumbnail grid
        if (_thumbnailGrid != null)
        {
            _thumbnailGrid.LoadProducts(_filteredProducts);
        }
        
        UpdateStatus($"Showing {_filteredProducts.Count} of {_allProducts.Count} products");
    }
    
    private void OnCategorySelected(object? sender, CategorySelectedEventArgs e)
    {
        // Filter products by selected category
        if (string.IsNullOrEmpty(e.Category.Path))
        {
            // "All Products" selected
            _filteredProducts = _allProducts;
        }
        else
        {
            // Filter by category path
            _filteredProducts = _allProducts
                .Where(p => p.CategoryPath?.StartsWith(e.Category.Path, StringComparison.OrdinalIgnoreCase) ?? false)
                .ToList();
        }
        
        if (_thumbnailGrid != null)
        {
            _thumbnailGrid.LoadProducts(_filteredProducts);
        }
        
        UpdateStatus($"Category: {e.Category.Name} ({_filteredProducts.Count} products)");
    }
    
    private void OnFiltersChanged(object? sender, FiltersChangedEventArgs e)
    {
        if (_searchService == null) return;
        
        // Apply filters to products
        _filteredProducts = _searchService.ApplyFilters(_allProducts, e.Filters).ToList();
        
        if (_thumbnailGrid != null)
        {
            _thumbnailGrid.LoadProducts(_filteredProducts);
        }
        
        UpdateStatus($"Filtered: {_filteredProducts.Count} products");
    }
    
    private void OnProductSelectedInGrid(object? sender, ProductSelectedEventArgs e)
    {
        // Update detail pane with selected product
        if (_detailPane != null && e.Product != null)
        {
            _detailPane.LoadProduct(e.Product);
        }
        
        UpdateStatus($"Selected: {e.Product?.ProductName ?? "None"}");
    }
    
    private void OnMultipleProductsSelected(object? sender, ProductsSelectedEventArgs e)
    {
        UpdateStatus($"Selected {e.Products.Count} products for batch operation");
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
    
    private void OnMultiSelectToggled(object? sender, EventArgs e)
    {
        _multiSelectMode = !_multiSelectMode;
        
        if (_multiSelectButton != null)
        {
            _multiSelectButton.Text = _multiSelectMode ? "✓ Multi-Select" : "Multi-Select";
        }
        
        // TODO: Enable multi-select mode in ThumbnailGrid
        UpdateStatus(_multiSelectMode ? "Multi-select mode enabled" : "Multi-select mode disabled");
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
        UpdateStatus("Settings dialog not yet implemented");
        // TODO: Open settings dialog
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
