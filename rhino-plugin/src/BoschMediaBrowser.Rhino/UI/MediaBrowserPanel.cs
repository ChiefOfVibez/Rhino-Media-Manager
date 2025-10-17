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
using System.Threading.Tasks;

namespace BoschMediaBrowser.Rhino.UI;

/// <summary>
/// Main dockable panel for the Bosch Media Browser with rich UI
/// </summary>
[System.Runtime.InteropServices.Guid("A3B5C7D9-1E2F-4A5B-8C9D-0E1F2A3B4C5D")]
public class MediaBrowserPanel : Panel
{
    // Static instance for access from commands
    public static MediaBrowserPanel? Instance { get; private set; }

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
    private TabPage? _favouritesTabPage;
    private TabPage? _collectionsTabPage;
    private TabPage? _sidebarFavouritesTabPage;
    private TabPage? _sidebarCollectionsTabPage;
    private Button? _refreshButton;
    private Button? _exportButton;
    private Button? _settingsButton;
    private Button? _viewModeButton;
    private Button? _multiSelectButton;
    private Button? _batchInsertButton;
    private Button? _updateLinkedButton;
    
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
        RhinoApp.WriteLine("=== MediaBrowserPanel: Constructor (EMPTY) ===");
        Instance = this; // Set static instance for command access
        LogToFile("=== MediaBrowserPanel: Constructor (EMPTY) ===");
        
        // Set fixed size (non-resizable)
        Size = new Size(940, 725);
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
                
                // Share UserDataService with plugin instance (for commands to access)
                if (BoschMediaBrowserPlugin.Instance != null)
                {
                    BoschMediaBrowserPlugin.Instance.UserDataService = _userDataService;
                    BoschMediaBrowserPlugin.Instance.CollectionsChanged += OnPluginCollectionsChanged;
                    BoschMediaBrowserPlugin.Instance.ProductInsertRequested += OnPluginProductInsertRequested;
                }
                
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
        
        _batchInsertButton = new Button
        {
            Text = "Insert Selected (0)",
            ToolTip = "Insert all selected products",
            Visible = false
        };
        _batchInsertButton.Click += OnBatchInsertButtonClicked;
        
        _updateLinkedButton = new Button 
        { 
            Text = "🔗 Update Linked",
            ToolTip = "Update all linked blocks from source files"
        };
        _updateLinkedButton.Click += OnUpdateLinkedClicked;
        
        _refreshButton = new Button { Text = "⟳ Refresh" };
        _refreshButton.Click += OnRefreshClicked;
        
        _exportButton = new Button 
        { 
            Text = "📊 Export List",
            ToolTip = "Export stocking list from current document"
        };
        _exportButton.Click += OnExportStockingList;
        
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
                _batchInsertButton,
                _updateLinkedButton,
                _refreshButton,
                _exportButton,
                _settingsButton
            }
        };

        // Tabs
        _tabs = new TabControl();
        
        // Browse Tab with rich UI
        var browseTab = new TabPage { Text = "📁 Browse" };
        browseTab.Content = CreateBrowseTab();
        
        // Favourites Tab - Using FavouritesView
        _favouritesTabPage = new TabPage { Text = "⭐ Favourites" };
        _favouritesTabPage.Content = CreateFavouritesTab();
        
        // Collections Tab - Using CollectionsView
        _collectionsTabPage = new TabPage { Text = "📂 Collections" };
        _collectionsTabPage.Content = CreateCollectionsTab();

        _tabs.Pages.Add(browseTab);
        _tabs.Pages.Add(_favouritesTabPage);
        _tabs.Pages.Add(_collectionsTabPage);

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
        
        var mainLayout = new TableLayout
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
        
        Content = mainLayout;
        
        // Apply theme colors if set
        ApplyThemeColors();
    }
    
    private void ApplyThemeColors()
    {
        if (_settingsService == null) return;
        
        var settings = _settingsService.GetSettings();
        
        // Get all theme colors
        Color? mainBgColor = settings.ThemeBackgroundColor.HasValue 
            ? Color.FromArgb(settings.ThemeBackgroundColor.Value) 
            : (Color?)null;
            
        Color? mainTextColor = settings.ThemeTextColor.HasValue 
            ? Color.FromArgb(settings.ThemeTextColor.Value) 
            : (Color?)null;
            
        Color? sidebarBgColor = settings.ThemeSidebarBackground.HasValue 
            ? Color.FromArgb(settings.ThemeSidebarBackground.Value) 
            : (Color?)null;
            
        Color? sidebarTextColor = settings.ThemeSidebarText.HasValue
            ? Color.FromArgb(settings.ThemeSidebarText.Value)
            : (Color?)null;
        
        Color? cardBgColor = settings.ThemeCardBackground.HasValue 
            ? Color.FromArgb(settings.ThemeCardBackground.Value) 
            : (Color?)null;
            
        Color? cardTextColor = settings.ThemeCardText.HasValue 
            ? Color.FromArgb(settings.ThemeCardText.Value) 
            : (Color?)null;
        
        Color? buttonBgColor = settings.ThemeButtonBackground.HasValue
            ? Color.FromArgb(settings.ThemeButtonBackground.Value)
            : (Color?)null;
        
        Color? buttonTextColor = settings.ThemeButtonText.HasValue
            ? Color.FromArgb(settings.ThemeButtonText.Value)
            : (Color?)null;
        
        Color? listRowBgColor = settings.ThemeListRowBackground.HasValue
            ? Color.FromArgb(settings.ThemeListRowBackground.Value)
            : (Color?)null;
        
        Color? listRowTextColor = settings.ThemeListRowText.HasValue
            ? Color.FromArgb(settings.ThemeListRowText.Value)
            : (Color?)null;

        // Apply main panel background
        if (mainBgColor.HasValue)
        {
            BackgroundColor = mainBgColor.Value;
        }
        
        // Apply to status label
        if (mainTextColor.HasValue && _statusLabel != null)
        {
            _statusLabel.TextColor = mainTextColor.Value;
        }
        
        // Apply to tabs (main product area)
        if (_tabs != null && mainBgColor.HasValue)
        {
            _tabs.BackgroundColor = mainBgColor.Value;
        }
        
        // Apply sidebar background to category list
        if (_categoryList != null && sidebarBgColor.HasValue)
        {
            _categoryList.BackgroundColor = sidebarBgColor.Value;
            if (sidebarTextColor.HasValue) ApplyTextColorRecursive(_categoryList, sidebarTextColor.Value);
        }
        
        // Apply card colors to product card grid
        if (_productCardGrid != null)
        {
            if (cardBgColor.HasValue) 
            {
                _productCardGrid.BackgroundColor = cardBgColor.Value;
            }
            // Card text color will be applied to individual cards
        }
        
        // Apply card colors to product list view
        if (_productListView != null)
        {
            if (cardBgColor.HasValue) 
            {
                _productListView.BackgroundColor = cardBgColor.Value;
            }
            if (cardTextColor.HasValue)
            {
                ApplyTextColorRecursive(_productListView, cardTextColor.Value);
            }
            _productListView.ApplyTheme(cardBgColor, cardTextColor, buttonBgColor, buttonTextColor, listRowBgColor, listRowTextColor);
        }
        
        ApplyToolbarTheme(buttonBgColor, buttonTextColor);
        ApplySidebarTabTheme(sidebarBgColor, sidebarTextColor, buttonBgColor, buttonTextColor, listRowBgColor, listRowTextColor);
    }
    
    private void ApplyToolbarTheme(Color? buttonBgColor, Color? buttonTextColor)
    {
        var buttons = new[] { _viewModeButton, _multiSelectButton, _batchInsertButton, _updateLinkedButton, _refreshButton, _exportButton, _settingsButton };
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            btn.BackgroundColor = buttonBgColor ?? Colors.Transparent;
            btn.TextColor = buttonTextColor ?? Colors.Black;
        }
    }
    
    private void ApplySidebarTabTheme(Color? sidebarBackground, Color? sidebarText, Color? buttonBg, Color? buttonText, Color? listRowBg, Color? listRowText)
    {
        if (_favouritesTabPage?.Content is FavouritesViewCompact favouritesView)
        {
            favouritesView.ApplyTheme(sidebarBackground, sidebarText, buttonBg, buttonText);
        }
        if (_collectionsTabPage?.Content is CollectionsViewCompact collectionsView)
        {
            collectionsView.ApplyTheme(sidebarBackground, sidebarText, buttonBg, buttonText);
        }
        if (_sidebarFavouritesTabPage?.Content is FavouritesViewCompact sidebarFavView)
        {
            sidebarFavView.ApplyTheme(sidebarBackground, sidebarText, buttonBg, buttonText);
        }
        if (_sidebarCollectionsTabPage?.Content is CollectionsViewCompact sidebarCollView)
        {
            sidebarCollView.ApplyTheme(sidebarBackground, sidebarText, buttonBg, buttonText);
        }
    }
    
    private void ApplyTextColorRecursive(Control control, Color textColor)
    {
        if (control is Label label)
        {
            label.TextColor = textColor;
        }
        else if (control is Container container)
        {
            foreach (var child in container.Controls)
            {
                ApplyTextColorRecursive(child, textColor);
            }
        }
    }
    
    private Control CreateLeftSidebar()
    {
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

        var collectionsTab = new TabPage { Text = "⭐ Collections" };
        collectionsTab.Content = CreateCollectionsTabContent();

        TabPage filtersTab;
        if (_searchService != null)
        {
            _filtersBar = new FiltersBar(_searchService);
            _filtersBar.FiltersChanged += OnFiltersChanged;
            filtersTab = new TabPage
            {
                Text = "🔧 Filters",
                Content = new Panel
                {
                    Content = _filtersBar,
                    Padding = 5
                }
            };
        }
        else
        {
            filtersTab = new TabPage
            {
                Text = "🔧 Filters",
                Content = new Label { Text = "Services not initialized" }
            };
        }

        sidebarTabs.Pages.Add(browseTab);
        sidebarTabs.Pages.Add(collectionsTab);
        sidebarTabs.Pages.Add(filtersTab);

        return sidebarTabs;
    }

    private Control CreateCollectionsTabContent()
    {
        // Combine favourites and collections in one tab
        var collectionsTabs = new TabControl();
        
        _sidebarFavouritesTabPage = new TabPage { Text = "Favourites" };
        _sidebarFavouritesTabPage.Content = CreateFavouritesTab();
        
        _sidebarCollectionsTabPage = new TabPage { Text = "Collections" };
        _sidebarCollectionsTabPage.Content = CreateCollectionsTab();
        
        collectionsTabs.Pages.Add(_sidebarFavouritesTabPage);
        collectionsTabs.Pages.Add(_sidebarCollectionsTabPage);
        
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
        _productCardGrid.AddToCollectionRequested += OnAddToCollectionRequested;
        
        _productListView = new ProductListView(_thumbnailService, _userDataService);
        _productListView.ProductSelected += OnProductSelectedInGrid;
        _productListView.ProductPreview += OnProductPreviewRequested;
        _productListView.ProductInsert += OnProductInsertRequested;
        _productListView.AddToCollectionRequested += OnAddToCollectionRequested;
        _productListView.BatchInsert += OnBatchInsertRequested;
        _productListView.SelectionChanged += OnListViewSelectionChanged;
        
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
        _productCardGrid.AddToCollectionRequested += OnAddToCollectionRequested;

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
        // Use compact FavouritesView that acts as a filter
        if (_dataService == null || _userDataService == null)
        {
            return new Label { Text = "Loading..." };
        }
        
        var favouritesView = new FavouritesViewCompact(_dataService, _userDataService);
        favouritesView.FilterRequested += OnFilterRequested;
        return favouritesView;
    }
    
    private Control CreateCollectionsTab()
    {
        // Use compact CollectionsView that acts as a filter
        RhinoApp.WriteLine("CreateCollectionsTab: ENTRY");
        
        if (_dataService == null || _userDataService == null)
        {
            RhinoApp.WriteLine("CreateCollectionsTab: Services not ready");
            return new Label { Text = "Loading..." };
        }
        
        RhinoApp.WriteLine("CreateCollectionsTab: Creating compact CollectionsView...");
        try
        {
            var collectionsView = new CollectionsViewCompact(_dataService, _userDataService);
            collectionsView.FilterRequested += OnFilterRequested;
            collectionsView.CollectionInsertRequested += OnCollectionInsertRequested;
            collectionsView.RemoveItemsRequested += OnRemoveItemsRequested;
            RhinoApp.WriteLine("CreateCollectionsTab: CollectionsView created successfully");
            return collectionsView;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"CreateCollectionsTab ERROR: {ex.Message}");
            RhinoApp.WriteLine($"Stack: {ex.StackTrace}");
            return new Label { Text = $"Error: {ex.Message}" };
        }
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

            // Initialize DataService with base path from settings and logger for debugging
            _dataService = new DataService(settings.BaseServerPath, RhinoApp.WriteLine);
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
                    
                    // Refresh Favourites and Collections tabs now that services are ready
                    RhinoApp.WriteLine("=== REFRESHING FAVOURITES TAB ===");
                    LogToFile("InitializeAsync UI Update: Refreshing Favourites tab...");
                    if (_favouritesTabPage != null)
                    {
                        _favouritesTabPage.Content = CreateFavouritesTab();
                        RhinoApp.WriteLine("Favourites tab refreshed successfully");
                        LogToFile("InitializeAsync UI Update: Favourites tab refreshed");
                    }
                    else
                    {
                        RhinoApp.WriteLine("WARNING: _favouritesTabPage is null!");
                    }
                    
                    RhinoApp.WriteLine("=== REFRESHING COLLECTIONS TAB ===");
                    LogToFile("InitializeAsync UI Update: Refreshing Collections tab...");
                    if (_collectionsTabPage != null)
                    {
                        _collectionsTabPage.Content = CreateCollectionsTab();
                        RhinoApp.WriteLine("Collections tab refreshed successfully");
                        LogToFile("InitializeAsync UI Update: Collections tab refreshed");
                    }
                    else
                    {
                        RhinoApp.WriteLine("WARNING: _collectionsTabPage is null!");
                    }
                    
                    // Refresh sidebar Favourites and Collections tabs
                    RhinoApp.WriteLine("=== REFRESHING SIDEBAR FAVOURITES TAB ===");
                    if (_sidebarFavouritesTabPage != null)
                    {
                        _sidebarFavouritesTabPage.Content = CreateFavouritesTab();
                        RhinoApp.WriteLine("Sidebar Favourites tab refreshed successfully");
                    }
                    
                    RhinoApp.WriteLine("=== REFRESHING SIDEBAR COLLECTIONS TAB ===");
                    if (_sidebarCollectionsTabPage != null)
                    {
                        _sidebarCollectionsTabPage.Content = CreateCollectionsTab();
                        RhinoApp.WriteLine("Sidebar Collections tab refreshed successfully");
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
    
    /// <summary>
    /// Public method to refresh collections (callable from commands)
    /// </summary>
    public void RefreshCollections()
    {
        RhinoApp.WriteLine("MediaBrowserPanel: RefreshCollections called");
        
        // Reload user data first
        _userDataService?.Load();
        
        // Refresh sidebar collections tab if it exists
        if (_sidebarCollectionsTabPage != null)
        {
            Application.Instance.AsyncInvoke(() =>
            {
                _sidebarCollectionsTabPage.Content = CreateCollectionsTab();
                RhinoApp.WriteLine("Collections tab refreshed successfully");
            });
        }
    }
    
    /// <summary>
    /// Handle collections changed event from plugin (triggered by commands)
    /// </summary>
    private void OnPluginCollectionsChanged(object? sender, EventArgs e)
    {
        RefreshCollections();
    }
    
    /// <summary>
    /// Handle Add to Collection request from product card
    /// </summary>
    private void OnAddToCollectionRequested(object? sender, ProductCardEventArgs e)
    {
        if (_userDataService == null) return;
        
        RhinoApp.WriteLine($"Add to Collection requested for: {e.Product.ProductName}");
        
        // Get all collections
        var collections = _userDataService.GetCollections().ToList();
        
        if (collections.Count == 0)
        {
            MessageBox.Show(
                "No collections exist. Create a collection first from the Collections tab.",
                "No Collections",
                MessageBoxType.Information
            );
            return;
        }
        
        // Show collection picker dialog
        var dialog = new Dialog<string>
        {
            Title = $"Add '{e.Product.ProductName}' to Collection",
            Padding = new Padding(10),
            MinimumSize = new Size(350, 200)
        };
        
        var listBox = new ListBox();
        foreach (var collection in collections)
        {
            listBox.Items.Add(new ListItem
            {
                Text = $"{collection.Name} ({collection.Items.Count} items)",
                Key = collection.Id
            });
        }
        
        var layout = new DynamicLayout();
        layout.AddRow(new Label { Text = "Select Collection:" });
        layout.AddRow(new Scrollable { Content = listBox, Height = 150 });
        layout.AddRow(null); // spacer
        
        var okButton = new Button { Text = "Add" };
        okButton.Click += (s, args) =>
        {
            if (listBox.SelectedIndex >= 0)
            {
                dialog.Close(listBox.SelectedKey);
            }
        };
        
        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Click += (s, args) => dialog.Close(null);
        
        layout.AddRow(null, okButton, cancelButton);
        
        dialog.Content = layout;
        dialog.DefaultButton = okButton;
        dialog.AbortButton = cancelButton;
        
        var selectedCollectionId = dialog.ShowModal(this);
        
        if (!string.IsNullOrEmpty(selectedCollectionId))
        {
            // Add product to collection on background thread
            Task.Run(() =>
            {
                try
                {
                    var collection = _userDataService.GetCollectionById(selectedCollectionId);
                    if (collection != null)
                    {
                        // Check if already exists
                        if (collection.Items.Any(item => item.ProductId == e.Product.Id))
                        {
                            RhinoApp.WriteLine($"Product '{e.Product.ProductName}' already in collection '{collection.Name}'");
                            Application.Instance.AsyncInvoke(() =>
                            {
                                MessageBox.Show($"'{e.Product.ProductName}' is already in '{collection.Name}'", "Already Added", MessageBoxType.Information);
                            });
                            return;
                        }
                        
                        // Add as CollectionItem with holder/packaging settings
                        var newItem = new CollectionItem
                        {
                            ProductId = e.Product.Id,
                            HolderVariantKey = e.SelectedHolderVariant,
                            IncludePackaging = e.IncludePackaging,
                            AddedAt = DateTime.UtcNow
                        };
                        
                        collection.Items.Add(newItem);
                        
                        // Also update legacy ProductIds for backwards compatibility
                        if (!collection.ProductIds.Contains(e.Product.Id))
                        {
                            collection.ProductIds.Add(e.Product.Id);
                        }
                        
                        _userDataService.UpdateCollection(collection);
                        RhinoApp.WriteLine($"✓ Added '{e.Product.ProductName}' to collection '{collection.Name}' with settings: Holder={e.SelectedHolderVariant ?? "none"}, Pkg={e.IncludePackaging}");
                        
                        // Refresh collections tab
                        Application.Instance.AsyncInvoke(() =>
                        {
                            if (_sidebarCollectionsTabPage != null)
                            {
                                _sidebarCollectionsTabPage.Content = CreateCollectionsTab();
                            }
                            UpdateStatus($"Added to collection: {collection.Name}");
                        });
                    }
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"✗ Error adding to collection: {ex.Message}");
                }
            });
        }
    }
    
    /// <summary>
    /// Handle remove items request from collections view
    /// </summary>
    private void OnRemoveItemsRequested(object? sender, CollectionsViewCompact.RemoveItemsEventArgs e)
    {
        if (_userDataService == null || _productListView == null) return;
        
        var collection = e.Collection;
        
        // Check if we're in multi-select mode with selections
        if (_multiSelectMode && _productListView.SelectedCount > 0)
        {
            // Get selected product IDs
            var selectedItems = _productListView.GetSelectedItems();
            var selectedProductIds = selectedItems.Select(item => item.product.Id).ToList();
            
            var result = MessageBox.Show(
                $"Remove {selectedProductIds.Count} product(s) from '{collection.Name}'?",
                "Remove from Collection",
                MessageBoxButtons.YesNo
            );
            
            if (result == DialogResult.Yes)
            {
                RemoveProductsFromCollection(selectedProductIds, collection);
            }
        }
        else
        {
            // No selection - inform user to use multi-select
            MessageBox.Show(
                $"To remove items from '{collection.Name}':\n\n" +
                "1. Enable Multi-Select mode\n" +
                "2. Check the products you want to remove\n" +
                "3. Click 'Remove Item' button",
                "Remove Items",
                MessageBoxType.Information
            );
        }
    }
    
    /// <summary>
    /// Remove products from collection
    /// </summary>
    private void RemoveProductsFromCollection(List<string> productIds, Collection collection)
    {
        Task.Run(() =>
        {
            try
            {
                int removedCount = 0;
                
                foreach (var productId in productIds)
                {
                    // Remove from Items list
                    var itemToRemove = collection.Items.FirstOrDefault(item => item.ProductId == productId);
                    if (itemToRemove != null)
                    {
                        collection.Items.Remove(itemToRemove);
                        removedCount++;
                    }
                    
                    // Also remove from legacy ProductIds
                    collection.ProductIds.Remove(productId);
                }
                
                _userDataService.UpdateCollection(collection);
                RhinoApp.WriteLine($"✓ Removed {removedCount} product(s) from collection '{collection.Name}'");
                
                // Refresh UI
                Application.Instance.AsyncInvoke(() =>
                {
                    if (_sidebarCollectionsTabPage != null)
                    {
                        _sidebarCollectionsTabPage.Content = CreateCollectionsTab();
                    }
                    
                    // Refresh the current view to remove the products
                    _filteredProducts = _allProducts
                        .Where(p => collection.Items.Any(item => item.ProductId == p.Id))
                        .ToList();
                    
                    LoadProductsIntoCurrentView();
                    UpdateStatus($"Removed {removedCount} product(s) from collection: {collection.Name}");
                    
                    // Clear selection
                    if (_productListView != null)
                    {
                        _productListView.ClearSelections();
                    }
                });
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"✗ Error removing from collection: {ex.Message}");
            }
        });
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
    
    /// <summary>
    /// Handle filter request from Favourites/Collections views
    /// </summary>
    private void OnFilterRequested(object? sender, FavouritesViewCompact.FilterEventArgs e)
    {
        RhinoApp.WriteLine($"=== FILTER REQUESTED: {e.FilterName} ({e.ProductIds.Count} products) ===");
        
        // Filter products by IDs
        _filteredProducts = _allProducts
            .Where(p => e.ProductIds.Contains(p.Id))
            .ToList();
        
        // Reload products into current view
        LoadProductsIntoCurrentView();
        
        UpdateStatus($"Showing {_filteredProducts.Count} products from {e.FilterName}");
    }
    
    /// <summary>
    /// Handle filter request from Collections view (same signature)
    /// </summary>
    private void OnFilterRequested(object? sender, CollectionsViewCompact.FilterEventArgs e)
    {
        RhinoApp.WriteLine($"=== FILTER REQUESTED: {e.FilterName} ({e.ProductIds.Count} products) ===");
        
        // Filter products by IDs
        _filteredProducts = _allProducts
            .Where(p => e.ProductIds.Contains(p.Id))
            .ToList();
        
        // Reload products into current view
        LoadProductsIntoCurrentView();
        
        UpdateStatus($"Showing {_filteredProducts.Count} products from {e.FilterName}");
    }
    
    /// <summary>
    /// Handle collection batch insert request
    /// </summary>
    private void OnCollectionInsertRequested(object? sender, CollectionsViewCompact.CollectionInsertEventArgs e)
    {
        RhinoApp.WriteLine($"=== COLLECTION INSERT REQUESTED: {e.Collection.Name} ({e.Collection.Items.Count} items) ===");
        
        // Get insertion origin point (user clicks or auto at 0,0,0)
        var collectionOrigin = global::Rhino.Geometry.Point3d.Origin;
        
        // TODO: Prompt user for insertion point
        // For now, auto-insert at origin
        RhinoApp.WriteLine($"Collection will insert at origin: {collectionOrigin}");
        
        // Build insert list - separate lists for each block type
        var toolInserts = new List<(Product product, Holder? holder, global::Rhino.Geometry.Point3d insertPoint, global::Rhino.Geometry.Transform rotation)>();
        var holderInserts = new List<(string blockName, global::Rhino.Geometry.Point3d insertPoint, global::Rhino.Geometry.Transform rotation)>();
        var packagingInserts = new List<(Product product, global::Rhino.Geometry.Point3d insertPoint, global::Rhino.Geometry.Transform rotation)>();
        
        foreach (var collectionItem in e.Collection.Items)
        {
            var blockType = collectionItem.BlockType ?? "Tool"; // Default to Tool for legacy collections
            
            // Parse transforms with rotation (format: "x,y,z,rx,ry,rz;x,y,z,rx,ry,rz;...")
            var transformsWithRotation = ParseTransformsWithRotation(collectionItem.Transforms);
            
            RhinoApp.WriteLine($"  • {blockType}: {collectionItem.ProductId}");
            RhinoApp.WriteLine($"    - InstanceCount: {collectionItem.InstanceCount}");
            RhinoApp.WriteLine($"    - Transforms: {transformsWithRotation.Count}");
            
            if (blockType == "Tool")
            {
                // Tool blocks: Look up product and insert with holder/packaging
                var product = _allProducts.FirstOrDefault(p => p.Id == collectionItem.ProductId);
                if (product == null)
                {
                    RhinoApp.WriteLine($"    ⚠️ Product not found: {collectionItem.ProductId}");
                    continue;
                }
                
                var holder = ResolveHolderFromKey(product, collectionItem.HolderVariantKey);
                
                foreach (var (pos, rot) in transformsWithRotation)
                {
                    var absolutePos = new global::Rhino.Geometry.Point3d(
                        collectionOrigin.X + pos.X,
                        collectionOrigin.Y + pos.Y,
                        collectionOrigin.Z + pos.Z
                    );
                    toolInserts.Add((product, holder, absolutePos, rot));
                }
            }
            else if (blockType == "Holder")
            {
                // Holder blocks: Insert directly at stored positions
                var blockName = collectionItem.ProductId; // Block definition name
                
                foreach (var (pos, rot) in transformsWithRotation)
                {
                    var absolutePos = new global::Rhino.Geometry.Point3d(
                        collectionOrigin.X + pos.X,
                        collectionOrigin.Y + pos.Y,
                        collectionOrigin.Z + pos.Z
                    );
                    holderInserts.Add((blockName, absolutePos, rot));
                }
            }
            else if (blockType == "Packaging")
            {
                // Packaging blocks: Need to find product and load packaging file
                // The ProductId for packaging is just the product name (e.g. "GBL 18V-750")
                var productNameFromBlock = collectionItem.ProductId;
                
                // Try to find the actual product to get packaging file path
                var product = _allProducts.FirstOrDefault(p => p.ProductName == productNameFromBlock);
                if (product != null && product.Packaging != null && !string.IsNullOrEmpty(product.Packaging.FullPath))
                {
                    foreach (var (pos, rot) in transformsWithRotation)
                    {
                        var absolutePos = new global::Rhino.Geometry.Point3d(
                            collectionOrigin.X + pos.X,
                            collectionOrigin.Y + pos.Y,
                            collectionOrigin.Z + pos.Z
                        );
                        RhinoApp.WriteLine($"    - Packaging at ({absolutePos.X:F0}, {absolutePos.Y:F0}, {absolutePos.Z:F0}) with rotation");
                        packagingInserts.Add((product, absolutePos, rot)); // Store product instead of block name
                    }
                }
                else
                {
                    RhinoApp.WriteLine($"    ⚠️ Product or packaging file not found for: {productNameFromBlock}");
                }
            }
        }
        
        var totalInstances = toolInserts.Count + holderInserts.Count + packagingInserts.Count;
        
        if (totalInstances == 0)
        {
            RhinoApp.WriteLine("No blocks to insert");
            UpdateStatus("No blocks to insert");
            return;
        }
        
        RhinoApp.WriteLine($"Inserting {totalInstances} total blocks ({toolInserts.Count} tools, {holderInserts.Count} holders, {packagingInserts.Count} packaging)...");
        UpdateStatus($"Inserting collection: {e.Collection.Name}...");
        
        // Insert all instances on background thread with progress
        Task.Run(() =>
        {
            int inserted = 0;
            
            // Insert tools ONLY (no auto-generation of holders/packaging)
            foreach (var (product, holder, insertPoint, rotation) in toolInserts)
            {
                try
                {
                    Application.Instance.Invoke(() =>
                    {
                        // Insert tool WITHOUT auto-generating holder/packaging
                        // We'll insert them separately from the stored collection data
                        InsertToolOnlyWithRotation(product, holder, insertPoint, rotation);
                    });
                    
                    inserted++;
                    Application.Instance.AsyncInvoke(() =>
                    {
                        UpdateStatus($"Inserting... ({inserted}/{totalInstances})");
                    });
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"Error inserting tool {product.ProductName}: {ex.Message}");
                }
            }
            
            // Insert holders directly (load files if needed)
            foreach (var (blockName, insertPoint, rotation) in holderInserts)
            {
                try
                {
                    Application.Instance.Invoke(() =>
                    {
                        InsertHolderBlockWithRotation(blockName, insertPoint, rotation);
                    });
                    
                    inserted++;
                    Application.Instance.AsyncInvoke(() =>
                    {
                        UpdateStatus($"Inserting... ({inserted}/{totalInstances})");
                    });
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"Error inserting holder {blockName}: {ex.Message}");
                }
            }
            
            // Insert packaging directly with rotation (load from file if needed)
            foreach (var (product, insertPoint, rotation) in packagingInserts)
            {
                try
                {
                    Application.Instance.Invoke(() =>
                    {
                        InsertPackagingWithRotation(product, insertPoint, rotation);
                    });
                    
                    inserted++;
                    Application.Instance.AsyncInvoke(() =>
                    {
                        UpdateStatus($"Inserting... ({inserted}/{totalInstances})");
                    });
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"Error inserting packaging {product.ProductName}: {ex.Message}");
                }
            }
            
            Application.Instance.AsyncInvoke(() =>
            {
                UpdateStatus($"✓ Collection inserted: {inserted}/{totalInstances} blocks");
                RhinoApp.WriteLine($"✓ Collection '{e.Collection.Name}' inserted: {inserted}/{totalInstances} blocks");
            });
        });
    }
    
    /// <summary>
    /// Parse transform string into list of Point3d positions (legacy, ignores rotation)
    /// Format: "x,y,z,rx,ry,rz;x,y,z,rx,ry,rz;..."
    /// </summary>
    private List<global::Rhino.Geometry.Point3d> ParseTransforms(string? transformsString)
    {
        var positions = new List<global::Rhino.Geometry.Point3d>();
        
        if (string.IsNullOrEmpty(transformsString))
            return positions;
        
        try
        {
            var transformEntries = transformsString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var entry in transformEntries)
            {
                var values = entry.Split(',');
                if (values.Length >= 3)
                {
                    if (double.TryParse(values[0], out var x) &&
                        double.TryParse(values[1], out var y) &&
                        double.TryParse(values[2], out var z))
                    {
                        positions.Add(new global::Rhino.Geometry.Point3d(x, y, z));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Error parsing transforms: {ex.Message}");
        }
        
        return positions;
    }
    
    /// <summary>
    /// Parse transform string into list of (position, rotation) tuples
    /// Format: "x,y,z,rx,ry,rz;x,y,z,rx,ry,rz;..."
    /// </summary>
    private List<(global::Rhino.Geometry.Point3d position, global::Rhino.Geometry.Transform rotation)> ParseTransformsWithRotation(string? transformsString)
    {
        var transforms = new List<(global::Rhino.Geometry.Point3d, global::Rhino.Geometry.Transform)>();
        
        if (string.IsNullOrEmpty(transformsString))
            return transforms;
        
        try
        {
            var transformEntries = transformsString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var entry in transformEntries)
            {
                var values = entry.Split(',');
                if (values.Length >= 6)
                {
                    if (double.TryParse(values[0], out var x) &&
                        double.TryParse(values[1], out var y) &&
                        double.TryParse(values[2], out var z) &&
                        double.TryParse(values[3], out var rotX) &&
                        double.TryParse(values[4], out var rotY) &&
                        double.TryParse(values[5], out var rotZ))
                    {
                        var position = new global::Rhino.Geometry.Point3d(x, y, z);
                        
                        // Create rotation transform from Euler angles (degrees)
                        var rotation = global::Rhino.Geometry.Transform.Identity;
                        
                        if (rotX != 0 || rotY != 0 || rotZ != 0)
                        {
                            // Convert degrees to radians
                            var radX = global::Rhino.RhinoMath.ToRadians(rotX);
                            var radY = global::Rhino.RhinoMath.ToRadians(rotY);
                            var radZ = global::Rhino.RhinoMath.ToRadians(rotZ);
                            
                            // Compose rotation: Rz * Ry * Rx (ZYX Euler convention)
                            rotation = global::Rhino.Geometry.Transform.Rotation(radZ, global::Rhino.Geometry.Vector3d.ZAxis, global::Rhino.Geometry.Point3d.Origin) *
                                      global::Rhino.Geometry.Transform.Rotation(radY, global::Rhino.Geometry.Vector3d.YAxis, global::Rhino.Geometry.Point3d.Origin) *
                                      global::Rhino.Geometry.Transform.Rotation(radX, global::Rhino.Geometry.Vector3d.XAxis, global::Rhino.Geometry.Point3d.Origin);
                        }
                        
                        transforms.Add((position, rotation));
                    }
                }
                else if (values.Length >= 3)
                {
                    // Fallback: only position, no rotation
                    if (double.TryParse(values[0], out var x) &&
                        double.TryParse(values[1], out var y) &&
                        double.TryParse(values[2], out var z))
                    {
                        transforms.Add((new global::Rhino.Geometry.Point3d(x, y, z), global::Rhino.Geometry.Transform.Identity));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Error parsing transforms with rotation: {ex.Message}");
        }
        
        return transforms;
    }
    
    /// <summary>
    /// Insert a block instance directly by block definition name with position and rotation
    /// </summary>
    private void InsertBlockInstanceDirect(string blockDefName, global::Rhino.Geometry.Point3d insertPoint, global::Rhino.Geometry.Transform rotation)
    {
        var doc = global::Rhino.RhinoDoc.ActiveDoc;
        if (doc == null) return;
        
        // Find block definition by name
        var blockDef = doc.InstanceDefinitions.Find(blockDefName);
        if (blockDef == null)
        {
            RhinoApp.WriteLine($"⚠️ Block definition not found: {blockDefName}");
            return;
        }
        
        // Create transform: translation + rotation
        var transform = global::Rhino.Geometry.Transform.Translation(insertPoint.X, insertPoint.Y, insertPoint.Z) * rotation;
        
        // Insert block instance
        var objId = doc.Objects.AddInstanceObject(blockDef.Index, transform);
        
        if (objId != Guid.Empty)
        {
            RhinoApp.WriteLine($"✓ Inserted block '{blockDefName}' at ({insertPoint.X:F0}, {insertPoint.Y:F0}, {insertPoint.Z:F0})");
        }
        else
        {
            RhinoApp.WriteLine($"✗ Failed to insert block '{blockDefName}'");
        }
    }
    
    /// <summary>
    /// Insert tool only with rotation (no holder, no packaging) for collection restoration
    /// </summary>
    private void InsertToolOnlyWithRotation(Product product, Holder? holder, global::Rhino.Geometry.Point3d insertPoint, global::Rhino.Geometry.Transform rotation)
    {
        // Find the correct 3DM file based on holder selection
        var toolMeshPath = Find3DMFile(product, holder);
        
        if (string.IsNullOrEmpty(toolMeshPath))
        {
            RhinoApp.WriteLine($"✗ Tool mesh file not found for: {product.ProductName}");
            return;
        }
        
        var doc = global::Rhino.RhinoDoc.ActiveDoc;
        if (doc == null) return;
        
        // Load the tool file and create block definition if needed
        var fileName = Path.GetFileNameWithoutExtension(toolMeshPath);
        var holderSuffix = holder != null ? $"_{holder.Variant}_{holder.Color}" : "";
        var blockName = fileName + holderSuffix;
        
        var blockDef = doc.InstanceDefinitions.Find(blockName);
        
        if (blockDef == null)
        {
            // Check if holder transform needs to be applied to the geometry
            var targetHolder = holder?.Variant ?? product.ReferenceHolder ?? "Tego";
            var needsHolderTransform = product.HolderTransforms != null && 
                                      product.HolderTransforms.ContainsKey(targetHolder) && 
                                      holder != null;
            
            if (needsHolderTransform)
            {
                // Load tool file and apply holder transform to geometry
                LoadFileAsBlockDefinitionWithTransform(toolMeshPath, blockName, product, holder);
            }
            else
            {
                // Load tool file as-is
                LoadFileAsBlockDefinition(toolMeshPath, blockName);
            }
            
            blockDef = doc.InstanceDefinitions.Find(blockName);
        }
        
        if (blockDef != null)
        {
            // Create transform: translation + rotation from stored data
            // The stored rotation is the final world rotation of the instance
            var transform = global::Rhino.Geometry.Transform.Translation(insertPoint.X, insertPoint.Y, insertPoint.Z) * rotation;
            
            // Insert block instance with stored rotation
            var objId = doc.Objects.AddInstanceObject(blockDef.Index, transform);
            
            if (objId != Guid.Empty)
            {
                // Attach metadata
                var newObj = doc.Objects.FindId(objId);
                if (newObj != null)
                {
                    newObj.Attributes.SetUserString("BMB_BlockType", "Tool");
                    newObj.Attributes.SetUserString("BMB_ProductId", product.Id);
                    if (holder != null)
                    {
                        newObj.Attributes.SetUserString("BMB_HolderVariant", $"{holder.Variant}_{holder.Color}");
                    }
                    newObj.CommitChanges();
                }
                
                RhinoApp.WriteLine($"✓ Inserted tool: {product.ProductName} (collection restore with rotation)");
            }
            else
            {
                RhinoApp.WriteLine($"✗ Failed to insert tool: {product.ProductName}");
            }
        }
        else
        {
            RhinoApp.WriteLine($"✗ Block definition not found after loading: {blockName}");
        }
    }
    
    /// <summary>
    /// Load a 3DM file as a block definition with holder transform applied to geometry
    /// </summary>
    private void LoadFileAsBlockDefinitionWithTransform(string filePath, string blockName, Product product, Holder holder)
    {
        var doc = global::Rhino.RhinoDoc.ActiveDoc;
        if (doc == null) return;
        
        try
        {
            // Check if block definition already exists
            var existingDef = doc.InstanceDefinitions.Find(blockName);
            if (existingDef != null)
            {
                RhinoApp.WriteLine($"Block definition '{blockName}' already exists");
                return;
            }
            
            // Read the 3DM file and extract geometry
            using (var file3dm = global::Rhino.FileIO.File3dm.Read(filePath))
            {
                if (file3dm == null)
                {
                    RhinoApp.WriteLine($"✗ Failed to read file: {filePath}");
                    return;
                }
                
                var geometry = new List<global::Rhino.Geometry.GeometryBase>();
                var attributes = new List<global::Rhino.DocObjects.ObjectAttributes>();
                
                // Calculate holder transform
                var holderTransform = CalculateHolderTransform(product, holder);
                
                foreach (var obj in file3dm.Objects)
                {
                    if (obj.Geometry != null)
                    {
                        var geom = obj.Geometry.Duplicate();
                        
                        // Apply holder transform to geometry
                        geom.Transform(holderTransform);
                        
                        geometry.Add(geom);
                        attributes.Add(obj.Attributes);
                    }
                }
                
                if (geometry.Count > 0)
                {
                    // Create instance definition with transformed geometry
                    var defIndex = doc.InstanceDefinitions.Add(blockName, string.Empty, global::Rhino.Geometry.Point3d.Origin, geometry, attributes);
                    
                    if (defIndex >= 0)
                    {
                        // Convert to linked block
                        var updateType = global::Rhino.DocObjects.InstanceDefinitionUpdateType.Linked;
                        bool success = doc.InstanceDefinitions.ModifySourceArchive(defIndex, filePath, updateType, quiet: true);
                        
                        if (success)
                        {
                            RhinoApp.WriteLine($"✓ Loaded linked block '{blockName}' with holder transform from {Path.GetFileName(filePath)}");
                        }
                        else
                        {
                            RhinoApp.WriteLine($"✓ Loaded block '{blockName}' with holder transform (embedded)");
                        }
                    }
                    else
                    {
                        RhinoApp.WriteLine($"✗ Failed to create block definition");
                    }
                }
                else
                {
                    RhinoApp.WriteLine($"✗ No geometry found in file: {filePath}");
                }
            }
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"✗ Error loading block definition: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Calculate holder transform from product holder transform data
    /// </summary>
    private global::Rhino.Geometry.Transform CalculateHolderTransform(Product product, Holder holder)
    {
        var targetHolder = holder.Variant ?? product.ReferenceHolder ?? "Tego";
        
        if (product.HolderTransforms == null || !product.HolderTransforms.ContainsKey(targetHolder))
        {
            return global::Rhino.Geometry.Transform.Identity;
        }
        
        var holderTransform = product.HolderTransforms[targetHolder];
        
        // Build transform: Scale * Rotation * Translation
        var transform = global::Rhino.Geometry.Transform.Identity;
        
        // Apply scale
        if (holderTransform.Scale != null && holderTransform.Scale.Length == 3)
        {
            var scaleTransform = global::Rhino.Geometry.Transform.Scale(
                global::Rhino.Geometry.Plane.WorldXY,
                holderTransform.Scale[0],
                holderTransform.Scale[1],
                holderTransform.Scale[2]
            );
            transform *= scaleTransform;
        }
        
        // Apply rotation (Euler angles in degrees: X, Y, Z)
        if (holderTransform.Rotation != null && holderTransform.Rotation.Length == 3)
        {
            var radX = global::Rhino.RhinoMath.ToRadians(holderTransform.Rotation[0]);
            var radY = global::Rhino.RhinoMath.ToRadians(holderTransform.Rotation[1]);
            var radZ = global::Rhino.RhinoMath.ToRadians(holderTransform.Rotation[2]);
            
            transform *= global::Rhino.Geometry.Transform.Rotation(radX, global::Rhino.Geometry.Vector3d.XAxis, global::Rhino.Geometry.Point3d.Origin);
            transform *= global::Rhino.Geometry.Transform.Rotation(radY, global::Rhino.Geometry.Vector3d.YAxis, global::Rhino.Geometry.Point3d.Origin);
            transform *= global::Rhino.Geometry.Transform.Rotation(radZ, global::Rhino.Geometry.Vector3d.ZAxis, global::Rhino.Geometry.Point3d.Origin);
        }
        
        // Apply translation
        if (holderTransform.Translation != null && holderTransform.Translation.Length == 3)
        {
            transform *= global::Rhino.Geometry.Transform.Translation(
                holderTransform.Translation[0],
                holderTransform.Translation[1],
                holderTransform.Translation[2]
            );
        }
        
        return transform;
    }
    
    /// <summary>
    /// Insert holder block with rotation, loading from file if needed
    /// </summary>
    private void InsertHolderBlockWithRotation(string blockDefName, global::Rhino.Geometry.Point3d insertPoint, global::Rhino.Geometry.Transform rotation)
    {
        var doc = global::Rhino.RhinoDoc.ActiveDoc;
        if (doc == null) return;
        
        // Check if block definition already exists
        var blockDef = doc.InstanceDefinitions.Find(blockDefName);
        
        if (blockDef == null)
        {
            // Need to find and load the holder file
            // Block name format: "Traverse_RAL9006_NN.ALL.BO07803"
            // Try to find the holder file in the holders directory
            
            var holderFileName = blockDefName + ".3dm";
            var holderPath = FindHolderFile(holderFileName);
            
            if (string.IsNullOrEmpty(holderPath))
            {
                RhinoApp.WriteLine($"⚠️ Holder file not found for block: {blockDefName}");
                return;
            }
            
            // Load holder file without inserting instance (just create block definition)
            LoadFileAsBlockDefinition(holderPath, blockDefName);
            
            // Now try to find the block definition
            blockDef = doc.InstanceDefinitions.Find(blockDefName);
        }
        
        if (blockDef != null)
        {
            // Create transform: translation + rotation
            var transform = global::Rhino.Geometry.Transform.Translation(insertPoint.X, insertPoint.Y, insertPoint.Z) * rotation;
            
            // Insert block instance with rotation
            var objId = doc.Objects.AddInstanceObject(blockDef.Index, transform);
            
            if (objId != Guid.Empty)
            {
                // Attach metadata
                var newObj = doc.Objects.FindId(objId);
                if (newObj != null)
                {
                    newObj.Attributes.SetUserString("BMB_BlockType", "Holder");
                    newObj.CommitChanges();
                }
                
                RhinoApp.WriteLine($"✓ Inserted holder '{blockDefName}' at ({insertPoint.X:F0}, {insertPoint.Y:F0}, {insertPoint.Z:F0})");
            }
            else
            {
                RhinoApp.WriteLine($"✗ Failed to insert holder '{blockDefName}'");
            }
        }
        else
        {
            RhinoApp.WriteLine($"✗ Block definition not found after loading: {blockDefName}");
        }
    }
    
    /// <summary>
    /// Insert packaging block with rotation, loading from file if needed
    /// </summary>
    private void InsertPackagingWithRotation(Product product, global::Rhino.Geometry.Point3d insertPoint, global::Rhino.Geometry.Transform rotation)
    {
        if (product.Packaging == null || string.IsNullOrEmpty(product.Packaging.FullPath))
        {
            RhinoApp.WriteLine($"✗ No packaging file for product: {product.ProductName}");
            return;
        }
        
        var doc = global::Rhino.RhinoDoc.ActiveDoc;
        if (doc == null) return;
        
        var packagingPath = product.Packaging.FullPath;
        if (!File.Exists(packagingPath))
        {
            RhinoApp.WriteLine($"✗ Packaging file not found: {packagingPath}");
            return;
        }
        
        // Determine block name from file
        var fileName = Path.GetFileNameWithoutExtension(packagingPath);
        
        // Check if block definition already exists
        var blockDef = doc.InstanceDefinitions.Find(fileName);
        
        if (blockDef == null)
        {
            // Load packaging file without inserting instance (just create block definition)
            LoadFileAsBlockDefinition(packagingPath, fileName);
            
            // Now find the block definition we just created
            blockDef = doc.InstanceDefinitions.Find(fileName);
        }
        
        if (blockDef != null)
        {
            // Create transform: translation + rotation
            var transform = global::Rhino.Geometry.Transform.Translation(insertPoint.X, insertPoint.Y, insertPoint.Z) * rotation;
            
            // Insert block instance with rotation
            var objId = doc.Objects.AddInstanceObject(blockDef.Index, transform);
            
            if (objId != Guid.Empty)
            {
                // Attach metadata
                var newObj = doc.Objects.FindId(objId);
                if (newObj != null)
                {
                    newObj.Attributes.SetUserString("BMB_BlockType", "Packaging");
                    newObj.CommitChanges();
                }
                
                RhinoApp.WriteLine($"✓ Inserted packaging '{fileName}' at ({insertPoint.X:F0}, {insertPoint.Y:F0}, {insertPoint.Z:F0}) with rotation");
            }
            else
            {
                RhinoApp.WriteLine($"✗ Failed to insert packaging '{fileName}'");
            }
        }
        else
        {
            RhinoApp.WriteLine($"✗ Block definition not found after loading: {fileName}");
        }
    }
    
    /// <summary>
    /// Load a 3DM file as a block definition without inserting an instance
    /// </summary>
    private void LoadFileAsBlockDefinition(string filePath, string blockName)
    {
        var doc = global::Rhino.RhinoDoc.ActiveDoc;
        if (doc == null) return;
        
        try
        {
            // Check if block definition already exists
            var existingDef = doc.InstanceDefinitions.Find(blockName);
            if (existingDef != null)
            {
                RhinoApp.WriteLine($"Block definition '{blockName}' already exists");
                return;
            }
            
            // Read the 3DM file and extract geometry
            using (var file3dm = global::Rhino.FileIO.File3dm.Read(filePath))
            {
                if (file3dm == null)
                {
                    RhinoApp.WriteLine($"✗ Failed to read file: {filePath}");
                    return;
                }
                
                var geometry = new List<global::Rhino.Geometry.GeometryBase>();
                var attributes = new List<global::Rhino.DocObjects.ObjectAttributes>();
                
                foreach (var obj in file3dm.Objects)
                {
                    if (obj.Geometry != null)
                    {
                        geometry.Add(obj.Geometry);
                        attributes.Add(obj.Attributes);
                    }
                }
                
                if (geometry.Count > 0)
                {
                    // Create instance definition
                    var defIndex = doc.InstanceDefinitions.Add(blockName, string.Empty, global::Rhino.Geometry.Point3d.Origin, geometry, attributes);
                    
                    if (defIndex >= 0)
                    {
                        // Convert to linked block
                        var updateType = global::Rhino.DocObjects.InstanceDefinitionUpdateType.Linked;
                        bool success = doc.InstanceDefinitions.ModifySourceArchive(defIndex, filePath, updateType, quiet: true);
                        
                        if (success)
                        {
                            RhinoApp.WriteLine($"✓ Loaded linked block '{blockName}' from {Path.GetFileName(filePath)}");
                        }
                        else
                        {
                            RhinoApp.WriteLine($"⚠️ Loaded block '{blockName}' as embedded (link conversion failed)");
                        }
                    }
                    else
                    {
                        RhinoApp.WriteLine($"✗ Failed to create block definition");
                    }
                }
                else
                {
                    RhinoApp.WriteLine($"✗ No geometry found in file: {filePath}");
                }
            }
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"✗ Error loading block definition: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Find holder file by filename in the standard holders directory
    /// </summary>
    private string? FindHolderFile(string holderFileName)
    {
        // Try standard holders directory structure
        var basePath = _userDataService?.GetUserData()?.Settings.BaseServerPath;
        if (string.IsNullOrEmpty(basePath))
        {
            return null;
        }
        
        var holdersPath = Path.Combine(basePath, "Holders");
        
        if (!Directory.Exists(holdersPath))
        {
            return null;
        }
        
        // Search recursively for the holder file
        var files = Directory.GetFiles(holdersPath, holderFileName, SearchOption.AllDirectories);
        
        if (files.Length > 0)
        {
            RhinoApp.WriteLine($"✓ Found holder file: {files[0]}");
            return files[0];
        }
        
        RhinoApp.WriteLine($"✗ Holder file not found: {holderFileName}");
        return null;
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
    
    private void InsertProductWithHolder(Product product, Holder? holder, bool includePackaging = false, global::Rhino.Geometry.Point3d? autoInsertPoint = null)
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
            
            // Get insertion point
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return;
            
            global::Rhino.Geometry.Point3d insertionPoint;
            
            if (autoInsertPoint.HasValue)
            {
                // Batch insert - use provided point (no user prompt)
                insertionPoint = autoInsertPoint.Value;
                RhinoApp.WriteLine($"Auto-insert at: {insertionPoint}");
            }
            else
            {
                // Single insert - prompt user
                var getPoint = new global::Rhino.Input.Custom.GetPoint();
                getPoint.SetCommandPrompt("Select insertion point for product");
                getPoint.Get();
                if (getPoint.CommandResult() != global::Rhino.Commands.Result.Success)
                {
                    RhinoApp.WriteLine("Insert cancelled by user");
                    return;
                }
                insertionPoint = getPoint.Point();
                RhinoApp.WriteLine($"Insertion point: {insertionPoint}");
            }
            
            // Insert tool mesh WITH holder transform
            // CONCEPT: Tool is positioned for reference holder (Tego).
            //          Transform repositions tool to fit selected holder.
            //          Holder itself inserts WITHOUT transform (stationary).
            RhinoApp.WriteLine($"Inserting tool mesh: {toolMeshPath}");
            if (holder != null)
            {
                RhinoApp.WriteLine($"Tool will be transformed to fit holder: {holder.Variant}");
            }
            InsertFile3dmAtPoint(toolMeshPath, insertionPoint, product, holder, applyHolderTransform: true, isPackaging: false, includePackagingMetadata: includePackaging, blockType: "Tool");
            
            // If holder selected, insert holder 3dm at insertion point WITHOUT transform
            // (holder is stationary, tool moves to fit it)
            if (holder != null)
            {
                // Use holder's full path stored in the holder object
                var holderPath = holder.FullPath;
                
                if (System.IO.File.Exists(holderPath))
                {
                    RhinoApp.WriteLine($"Inserting holder (stationary, no transform): {holderPath}");
                    InsertFile3dmAtPoint(holderPath, insertionPoint, product, null, applyHolderTransform: false, isPackaging: false, includePackagingMetadata: false, blockType: "Holder");
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
                    InsertFile3dmAtPoint(packagingPath, packagingPoint, product, null, applyHolderTransform: false, isPackaging: true, includePackagingMetadata: false, blockType: "Packaging");
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
        var selectedHolder = ResolveHolderFromKey(e.Product, e.SelectedHolderVariant);
        RhinoApp.WriteLine($"Insert requested - Holder: {(selectedHolder != null ? $"{selectedHolder.Variant} - {selectedHolder.Color}" : "None")}, Packaging: {e.IncludePackaging}");
        
        // Set product info for command and run it
        Commands.InsertProductCommand.ProductToInsert = e.Product;
        Commands.InsertProductCommand.HolderToInsert = selectedHolder;
        Commands.InsertProductCommand.IncludePackaging = e.IncludePackaging;
        
        // Run command (GetPoint will run in proper Rhino context)
        RhinoApp.RunScript("InsertBoschProduct", false);
    }
    
    /// <summary>
    /// Handle product insert request from plugin (triggered by command after GetPoint)
    /// </summary>
    private void OnPluginProductInsertRequested(object? sender, ProductInsertEventArgs e)
    {
        RhinoApp.WriteLine($"Plugin insert request received at point: {e.InsertionPoint}");
        InsertProductWithHolder(e.Product, e.Holder, e.IncludePackaging, e.InsertionPoint);
    }
    
    private void OnBatchInsertRequested(object? sender, BatchInsertEventArgs e)
    {
        UpdateStatus($"Batch inserting {e.Items.Count} products...");
        
        // Run batch insert on background thread with progress updates
        Task.Run(() =>
        {
            int insertedCount = 0;
            int index = 0;
            int total = e.Items.Count;
            
            foreach (var (product, holderKey, includePackaging) in e.Items)
            {
                try
                {
                    var selectedHolder = ResolveHolderFromKey(product, holderKey);
                    
                    // Auto-insert at origin with X offset: 1000mm per product
                    var insertPoint = new global::Rhino.Geometry.Point3d(index * 1000, 0, 0);
                    
                    // Run insert on UI thread (required for Rhino document operations)
                    Application.Instance.Invoke(() =>
                    {
                        InsertProductWithHolder(product, selectedHolder, includePackaging, insertPoint);
                    });
                    
                    insertedCount++;
                    index++;
                    
                    // Update progress
                    Application.Instance.AsyncInvoke(() =>
                    {
                        UpdateStatus($"Batch inserting... ({insertedCount}/{total})");
                    });
                    
                    RhinoApp.WriteLine($"Inserted {insertedCount}/{total}: {product.ProductName}");
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"Error inserting {product.ProductName}: {ex.Message}");
                }
            }
            
            // Final status update
            Application.Instance.AsyncInvoke(() =>
            {
                UpdateStatus($"Successfully inserted {insertedCount} of {total} products");
            });
            
            RhinoApp.WriteLine($"✓ Batch insert completed: {insertedCount}/{total} products");
        });
    }
    
    private void OnBatchInsertButtonClicked(object? sender, EventArgs e)
    {
        // Trigger batch insert from list view
        if (_productListView != null && _productListView.SelectedCount > 0)
        {
            // The list view will fire BatchInsert event which we handle above
            _productListView.PerformBatchInsert();
        }
    }
    
    private void OnListViewSelectionChanged(object? sender, EventArgs e)
    {
        // Update toolbar button text and visibility
        if (_batchInsertButton != null && _productListView != null)
        {
            int count = _productListView.SelectedCount;
            _batchInsertButton.Text = $"Insert Selected ({count})";
            _batchInsertButton.Visible = _multiSelectMode && count > 0;
        }
    }
    
    private Holder? ResolveHolderFromKey(Product product, string? holderKey)
    {
        if (string.IsNullOrEmpty(holderKey) || product.Holders == null)
            return null;
        
        // Parse key: "Traverse_RAL9006" -> Variant="Traverse", Color="RAL9006"
        var parts = holderKey.Split('_', 2);
        if (parts.Length == 2)
        {
            var variant = parts[0];
            var color = parts[1];
            
            var holder = product.Holders.FirstOrDefault(h => 
                h.Variant?.Equals(variant, StringComparison.OrdinalIgnoreCase) == true &&
                h.Color?.Equals(color, StringComparison.OrdinalIgnoreCase) == true);
            
            if (holder != null)
            {
                RhinoApp.WriteLine($"✓ Found holder: Variant={holder.Variant}, Color={holder.Color}");
            }
            return holder;
        }
        else
        {
            // Fallback: try just variant (backward compatibility)
            return product.Holders.FirstOrDefault(h => 
                h.Variant?.Equals(holderKey, StringComparison.OrdinalIgnoreCase) == true);
        }
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
    
    /// <summary>
    /// Ensures layer hierarchy exists and returns the target layer index
    /// Example: "Tools and Holders::PRO::Garden::1.Tools and Holders" or "...::2.Packaging"
    /// </summary>
    private int EnsureLayerHierarchy(Product product, bool isPackaging)
    {
        var doc = RhinoDoc.ActiveDoc;
        if (doc == null) return 0;
        
        // Build path: "Tools and Holders" > Range (DIY/PRO) > Category > Sublayer
        var pathSegments = new List<string>();
        
        // Top level: "Tools and Holders" (or topCategory)
        pathSegments.Add(product.TopCategory ?? "Tools and Holders");
        
        // Range level: "PRO" or "DIY"
        if (!string.IsNullOrEmpty(product.Range))
        {
            pathSegments.Add(product.Range);
        }
        
        // Category level: e.g., "Garden"
        if (!string.IsNullOrEmpty(product.Category))
        {
            pathSegments.Add(product.Category);
        }
        
        // Sublayer: "1.Tools and Holders" or "2.Packaging"
        pathSegments.Add(isPackaging ? "2.Packaging" : "1.Tools and Holders");
        
        // Create layers recursively
        int parentIndex = -1;
        foreach (var segment in pathSegments)
        {
            var layer = new global::Rhino.DocObjects.Layer();
            layer.Name = segment;
            layer.ParentLayerId = parentIndex >= 0 ? doc.Layers[parentIndex].Id : Guid.Empty;
            
            // Check if layer exists
            var existingIndex = doc.Layers.FindByFullPath(string.Join("::", pathSegments.Take(pathSegments.IndexOf(segment) + 1)), -1);
            if (existingIndex < 0)
            {
                // Create new layer
                parentIndex = doc.Layers.Add(layer);
                RhinoApp.WriteLine($"✓ Created layer: {segment}");
            }
            else
            {
                parentIndex = existingIndex;
            }
        }
        
        return parentIndex;
    }
    
    private void InsertFile3dmAtPoint(string file3dmPath, global::Rhino.Geometry.Point3d insertPoint, Product product, Holder? holder, bool applyHolderTransform = true, bool isPackaging = false, bool includePackagingMetadata = false, string blockType = "Tool")
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
            
            // Ensure layer hierarchy exists and get target layer
            int targetLayerIndex = EnsureLayerHierarchy(product, isPackaging);
            
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
                // Always read file and create embedded block first
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
                        defIndex = instanceDefs.Add(defName, string.Empty, global::Rhino.Geometry.Point3d.Origin, geometry, attributes);
                        RhinoApp.WriteLine($"✓ Created instance definition '{defName}' with {geometry.Count} objects");
                        
                        // Check if user wants linked blocks
                        var isLinked = settings?.InsertBlockType == "Linked";
                        if (isLinked)
                        {
                            try
                            {
                                // Convert to linked block by setting source archive
                                // Use Linked (not LinkedAndEmbedded) to create true linked block
                                var updateType = global::Rhino.DocObjects.InstanceDefinitionUpdateType.Linked;
                                bool success = instanceDefs.ModifySourceArchive(defIndex, file3dmPath, updateType, quiet: true);
                                if (success)
                                {
                                    RhinoApp.WriteLine($"  ✓ Converted to LINKED block (source: {file3dmPath})");
                                }
                                else
                                {
                                    RhinoApp.WriteLine($"  ⚠ Failed to link block to source file, will remain embedded");
                                }
                            }
                            catch (Exception linkEx)
                            {
                                RhinoApp.WriteLine($"  ⚠ Link block error: {linkEx.Message}");
                            }
                        }
                        else
                        {
                            RhinoApp.WriteLine($"  Block type: EMBEDDED (geometry stored in document)");
                        }
                    }
                    else
                    {
                        RhinoApp.WriteLine($"✗ ERROR: No geometry found in file");
                        return;
                    }
                    
                    // Now safe to dispose
                    file.Dispose();
                }
                else
                {
                    RhinoApp.WriteLine($"✗ ERROR: Failed to read 3dm file");
                    return;
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
                                        
                                        // Add metadata to each object (BMB = Bosch Media Browser)
                                        // ONLY attach ProductId to Tool blocks, NOT to Holder or Packaging blocks
                                        var newObj = doc.Objects.FindId(newId);
                                        if (newObj != null && blockType == "Tool")
                                        {
                                            // Core product info
                                            newObj.Attributes.SetUserString("BMB_ProductId", product.Id);
                                            newObj.Attributes.SetUserString("BMB_ProductName", product.ProductName);
                                            newObj.Attributes.SetUserString("BMB_ProductSKU", product.Sku ?? "");
                                            newObj.Attributes.SetUserString("BMB_ProductFile", file3dmPath);
                                            
                                            // Holder configuration
                                            if (holder != null)
                                            {
                                                var holderKey = $"{holder.Variant}_{holder.Color}";
                                                newObj.Attributes.SetUserString("BMB_HolderVariantKey", holderKey);
                                                newObj.Attributes.SetUserString("BMB_HolderVariant", holder.Variant);
                                                newObj.Attributes.SetUserString("BMB_HolderColor", holder.Color);
                                                newObj.Attributes.SetUserString("BMB_HolderCod", holder.CodArticol);
                                            }
                                            else
                                            {
                                                newObj.Attributes.SetUserString("BMB_HolderVariantKey", "");
                                            }
                                            
                                            // Packaging flag
                                            newObj.Attributes.SetUserString("BMB_IncludePackaging", includePackagingMetadata.ToString());
                                            
                                            // Insertion timestamp
                                            newObj.Attributes.SetUserString("BMB_InsertedAt", DateTime.UtcNow.ToString("o"));
                                            
                                            // Block type marker
                                            newObj.Attributes.SetUserString("BMB_BlockType", blockType);
                                            
                                            newObj.CommitChanges();
                                        }
                                        else if (newObj != null)
                                        {
                                            // For Holder/Packaging blocks, only mark the type
                                            newObj.Attributes.SetUserString("BMB_BlockType", blockType);
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
                    
                    // Set layer in attributes
                    var attributes = new global::Rhino.DocObjects.ObjectAttributes();
                    attributes.LayerIndex = targetLayerIndex;
                    
                    var objId = doc.Objects.AddInstanceObject(defIndex, transform, attributes);
                    
                    if (objId != Guid.Empty)
                    {
                        // Store product metadata in object user data (BMB = Bosch Media Browser)
                        // ONLY attach ProductId to Tool blocks, NOT to Holder or Packaging blocks
                        var obj = doc.Objects.FindId(objId);
                        if (obj != null && blockType == "Tool")
                        {
                            // Core product info
                            obj.Attributes.SetUserString("BMB_ProductId", product.Id);
                            obj.Attributes.SetUserString("BMB_ProductName", product.ProductName);
                            obj.Attributes.SetUserString("BMB_ProductSKU", product.Sku ?? "");
                            obj.Attributes.SetUserString("BMB_ProductFile", file3dmPath);
                            
                            // Holder configuration (if applicable)
                            if (holder != null)
                            {
                                var holderKey = $"{holder.Variant}_{holder.Color}"; // e.g., "Tego_RAL9006"
                                obj.Attributes.SetUserString("BMB_HolderVariantKey", holderKey);
                                obj.Attributes.SetUserString("BMB_HolderVariant", holder.Variant);
                                obj.Attributes.SetUserString("BMB_HolderColor", holder.Color);
                                obj.Attributes.SetUserString("BMB_HolderCod", holder.CodArticol);
                            }
                            else
                            {
                                obj.Attributes.SetUserString("BMB_HolderVariantKey", ""); // No holder
                            }
                            
                            // Packaging flag
                            obj.Attributes.SetUserString("BMB_IncludePackaging", includePackagingMetadata.ToString());
                            
                            // Insertion timestamp
                            obj.Attributes.SetUserString("BMB_InsertedAt", DateTime.UtcNow.ToString("o")); // ISO 8601
                            
                            // Block type marker
                            obj.Attributes.SetUserString("BMB_BlockType", blockType); // "Tool", "Holder", or "Packaging"
                            
                            obj.CommitChanges();
                            
                            RhinoApp.WriteLine($"✓ Metadata attached: ProductId={product.Id}, Holder={holder?.Variant ?? "none"}, Pkg={includePackagingMetadata}");
                        }
                        else if (obj != null)
                        {
                            // For Holder/Packaging blocks, only mark the type
                            obj.Attributes.SetUserString("BMB_BlockType", blockType);
                            obj.CommitChanges();
                            RhinoApp.WriteLine($"✓ Block type marked: {blockType} (no ProductId attached)");
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
        
        // Left side: Preview tabs with multiple images
        var meshPreviewImage = new ImageView { Size = new Size(380, 380) };
        var graficaPreviewImage = new ImageView { Size = new Size(380, 380) };
        var packagingPreviewImage = new ImageView { Size = new Size(380, 380) };
        var holderPreviewImage = new ImageView { Size = new Size(380, 380) };
        
        // Load preview images
        LoadPreviewImage(product.Previews?.MeshPreview?.FullPath, meshPreviewImage);
        LoadPreviewImage(product.Previews?.GraficaPreview?.FullPath, graficaPreviewImage);
        LoadPreviewImage(product.Packaging?.PreviewPath, packagingPreviewImage);
        
        // Create preview tabs
        var previewTabs = new TabControl();
        previewTabs.Pages.Add(new TabPage 
        { 
            Text = "Mesh", 
            Content = CreateCenteredImagePanel(meshPreviewImage) 
        });
        previewTabs.Pages.Add(new TabPage 
        { 
            Text = "Grafica", 
            Content = CreateCenteredImagePanel(graficaPreviewImage) 
        });
        previewTabs.Pages.Add(new TabPage 
        { 
            Text = "Packaging", 
            Content = CreateCenteredImagePanel(packagingPreviewImage) 
        });
        previewTabs.Pages.Add(new TabPage 
        { 
            Text = "Holder", 
            Content = CreateCenteredImagePanel(holderPreviewImage) 
        });
        
        var thumbnailContainer = new Panel
        {
            Width = 420,
            Padding = 10,
            Content = previewTabs
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
            
            // Create dropdown list for holder selection
            var holderDropdown = new DropDown();
            holderDropdown.Items.Add("No Holder");
            
            var holderOptions = new List<Holder?> { null }; // null = no holder
            holderOptions.AddRange(product.Holders);
            
            foreach (var holder in product.Holders)
            {
                holderDropdown.Items.Add($"{holder.Variant} - {holder.Color} ({holder.CodArticol})");
            }
            
            // Default to first holder (usually Tego)
            holderDropdown.SelectedIndex = 1;
            selectedHolder = holderOptions[1];
            
            // Load initial holder preview
            if (selectedHolder != null)
            {
                LoadPreviewImage(selectedHolder.Preview, holderPreviewImage);
            }
            
            // Update selection when dropdown changes
            holderDropdown.SelectedIndexChanged += (s, e) =>
            {
                var index = holderDropdown.SelectedIndex;
                selectedHolder = holderOptions[index];
                
                if (selectedHolder == null)
                {
                    holderPreviewImage.Image = null;
                }
                else
                {
                    LoadPreviewImage(selectedHolder.Preview, holderPreviewImage);
                }
            };
            
            detailsStack.Items.Add(holderDropdown);
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
        
        var addToCollectionBtn = new Button
        {
            Text = "📁 Add to Collection",
            Width = 150
        };
        addToCollectionBtn.Click += (s, e) =>
        {
            // Don't close dialog - let user continue
            OnAddToCollectionRequested(this, new ProductCardEventArgs(product));
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
            Items = { insertBtn, addToCollectionBtn, closeBtn }
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
        
        // Update batch insert button visibility
        if (_batchInsertButton != null)
        {
            if (!_multiSelectMode)
            {
                _batchInsertButton.Visible = false;
            }
            // else visibility will be controlled by selection count
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

    private void OnUpdateLinkedClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_updateLinkedButton != null)
            {
                _updateLinkedButton.Enabled = false;
                _updateLinkedButton.Text = "Updating...";
            }

            UpdateStatus("Updating all linked blocks...");
            
            // Run the BlockManager update command
            RhinoApp.RunScript("_-BlockManager _Update _All _Enter", false);
            
            UpdateStatus("All linked blocks updated successfully");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error updating blocks: {ex.Message}");
        }
        finally
        {
            if (_updateLinkedButton != null)
            {
                _updateLinkedButton.Enabled = true;
                _updateLinkedButton.Text = "🔗 Update Linked";
            }
        }
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
    
    private void OnExportStockingList(object? sender, EventArgs e)
    {
        try
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                MessageBox.Show("No active Rhino document", "Export Failed", MessageBoxType.Warning);
                return;
            }
            
            UpdateStatus("Analyzing block instances...");
            
            // Dictionary to count blocks: key = block name, value = count
            var toolCounts = new Dictionary<string, int>();
            var holderCounts = new Dictionary<string, int>();
            var packagingCounts = new Dictionary<string, int>();
            
            // Scan all block instances in document
            foreach (var obj in doc.Objects)
            {
                if (obj.ObjectType == global::Rhino.DocObjects.ObjectType.InstanceReference)
                {
                    var instanceObj = obj as global::Rhino.DocObjects.InstanceObject;
                    if (instanceObj != null)
                    {
                        var defName = instanceObj.InstanceDefinition.Name;
                        
                        // Categorize by block name patterns
                        // Holder pattern: {Variant}_{Color}_{CodArticol}
                        // Example: Tego_RAL7043_BO.161.9LL8600
                        
                        if (defName.Contains("packaging", StringComparison.OrdinalIgnoreCase))
                        {
                            // Packaging block
                            if (!packagingCounts.ContainsKey(defName))
                                packagingCounts[defName] = 0;
                            packagingCounts[defName]++;
                        }
                        else if ((defName.StartsWith("Tego_", StringComparison.OrdinalIgnoreCase) || 
                                  defName.StartsWith("Traverse_", StringComparison.OrdinalIgnoreCase)) &&
                                 defName.Contains("RAL", StringComparison.OrdinalIgnoreCase))
                        {
                            // Holder block: Starts with variant name + contains RAL color code
                            if (!holderCounts.ContainsKey(defName))
                                holderCounts[defName] = 0;
                            holderCounts[defName]++;
                        }
                        else
                        {
                            // Tool block (everything else)
                            if (!toolCounts.ContainsKey(defName))
                                toolCounts[defName] = 0;
                            toolCounts[defName]++;
                        }
                    }
                }
            }
            
            // Prompt for save location
            var saveDialog = new SaveFileDialog
            {
                Title = "Export Stocking List",
                Filters = { new FileFilter("CSV Files", ".csv") },
                FileName = $"StockingList_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            
            if (saveDialog.ShowDialog(this) == DialogResult.Ok)
            {
                var csvPath = saveDialog.FileName;
                
                // Write CSV
                using (var writer = new System.IO.StreamWriter(csvPath))
                {
                    // Header
                    writer.WriteLine("Category,Item Name,Quantity");
                    writer.WriteLine();
                    
                    // Tools section
                    writer.WriteLine("=== TOOLS ===,,");
                    foreach (var kvp in toolCounts.OrderBy(x => x.Key))
                    {
                        writer.WriteLine($"Tool,\"{kvp.Key}\",{kvp.Value}");
                    }
                    writer.WriteLine($",TOTAL TOOLS,{toolCounts.Values.Sum()}");
                    writer.WriteLine();
                    
                    // Holders section
                    writer.WriteLine("=== HOLDERS ===,,");
                    foreach (var kvp in holderCounts.OrderBy(x => x.Key))
                    {
                        writer.WriteLine($"Holder,\"{kvp.Key}\",{kvp.Value}");
                    }
                    writer.WriteLine($",TOTAL HOLDERS,{holderCounts.Values.Sum()}");
                    writer.WriteLine();
                    
                    // Packaging section
                    writer.WriteLine("=== PACKAGING ===,,");
                    foreach (var kvp in packagingCounts.OrderBy(x => x.Key))
                    {
                        writer.WriteLine($"Packaging,\"{kvp.Key}\",{kvp.Value}");
                    }
                    writer.WriteLine($",TOTAL PACKAGING,{packagingCounts.Values.Sum()}");
                    writer.WriteLine();
                    
                    // Summary
                    writer.WriteLine("=== SUMMARY ===,,");
                    writer.WriteLine($"Total Tools,{toolCounts.Count},{toolCounts.Values.Sum()}");
                    writer.WriteLine($"Total Holder Types,{holderCounts.Count},{holderCounts.Values.Sum()}");
                    writer.WriteLine($"Total Packaging Types,{packagingCounts.Count},{packagingCounts.Values.Sum()}");
                }
                
                UpdateStatus($"Exported stocking list: {System.IO.Path.GetFileName(csvPath)}");
                MessageBox.Show($"Stocking list exported successfully!\n\n{csvPath}", "Export Complete", MessageBoxType.Information);
                
                // Open the file
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(csvPath) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Export failed: {ex.Message}");
            MessageBox.Show($"Failed to export stocking list:\n\n{ex.Message}", "Export Error", MessageBoxType.Error);
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
            // Apply theme colors immediately
            ApplyThemeColors();
            
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

    /// <summary>
    /// Load preview image into ImageView
    /// </summary>
    private void LoadPreviewImage(string? imagePath, ImageView imageView)
    {
        try
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                imageView.Image = new Bitmap(imagePath);
            }
            else
            {
                imageView.Image = null;
            }
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Error loading preview image: {ex.Message}");
            imageView.Image = null;
        }
    }

    /// <summary>
    /// Create a centered panel for preview images
    /// </summary>
    private Control CreateCenteredImagePanel(ImageView imageView)
    {
        return new Scrollable
        {
            Content = new StackLayout
            {
                Padding = 10,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Items = { imageView }
            },
            Border = BorderType.None,
            BackgroundColor = Color.FromArgb(245, 245, 245)
        };
    }
}
