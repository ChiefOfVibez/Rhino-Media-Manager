using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;
using BoschMediaBrowser.Core.Services;

namespace BoschMediaBrowser.Rhino.UI.Controls;

/// <summary>
/// Filter controls for products (ranges, categories, holders, tags)
/// </summary>
public class FiltersBar : GroupBox
{
    private readonly SearchService _searchService;
    private List<Product> _allProducts = new();

    // Filter controls
    private CheckBox _diyCheckBox;
    private CheckBox _proCheckBox;
    private DropDown _categoryDropDown;
    private DropDown _holderDropDown;
    private ListBox _tagListBox;
    private Button _clearFiltersButton;

    public event EventHandler<FiltersChangedEventArgs>? FiltersChanged;

    public Filters CurrentFilters { get; private set; } = new();

    public FiltersBar(SearchService searchService)
    {
        _searchService = searchService;
        
        Text = "Filters";
        Padding = new Padding(10);

        InitializeControls();
        BuildLayout();
    }

    private void InitializeControls()
    {
        // Range checkboxes
        _diyCheckBox = new CheckBox { Text = "DIY" };
        _diyCheckBox.CheckedChanged += (s, e) => OnFiltersChanged();

        _proCheckBox = new CheckBox { Text = "PRO" };
        _proCheckBox.CheckedChanged += (s, e) => OnFiltersChanged();

        // Category dropdown
        _categoryDropDown = new DropDown();
        _categoryDropDown.SelectedIndexChanged += (s, e) => OnFiltersChanged();

        // Holder dropdown
        _holderDropDown = new DropDown();
        _holderDropDown.SelectedIndexChanged += (s, e) => OnFiltersChanged();

        // Tags list (single-select for now - Eto.Forms ListBox doesn't support multi-select easily)
        _tagListBox = new ListBox { Height = 100 };
        _tagListBox.SelectedIndexChanged += (s, e) => OnFiltersChanged();

        // Clear filters button
        _clearFiltersButton = new Button { Text = "Clear All Filters" };
        _clearFiltersButton.Click += (s, e) => ClearFilters();
    }

    private void BuildLayout()
    {
        var layout = new StackLayout
        {
            Spacing = 10,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Items =
            {
                // Range section
                new StackLayoutItem(new Label { Text = "Range:", Font = SystemFonts.Bold() }),
                new StackLayoutItem(_diyCheckBox),
                new StackLayoutItem(_proCheckBox),

                // Category section
                new StackLayoutItem(new Label { Text = "Category:", Font = SystemFonts.Bold() }),
                new StackLayoutItem(_categoryDropDown, true),

                // Holder section
                new StackLayoutItem(new Label { Text = "Holder Variant:", Font = SystemFonts.Bold() }),
                new StackLayoutItem(_holderDropDown, true),

                // Tags section
                new StackLayoutItem(new Label { Text = "Tags:", Font = SystemFonts.Bold() }),
                new StackLayoutItem(_tagListBox),

                // Clear button
                new StackLayoutItem(_clearFiltersButton)
            }
        };

        Content = layout;
    }

    /// <summary>
    /// Load products and populate filter options
    /// </summary>
    public void LoadProducts(List<Product> products)
    {
        _allProducts = products;
        PopulateFilterOptions();
    }

    /// <summary>
    /// Populate dropdowns with unique values from products
    /// </summary>
    private void PopulateFilterOptions()
    {
        // Categories
        var categories = _searchService.GetUniqueCategories(_allProducts);
        _categoryDropDown.Items.Clear();
        _categoryDropDown.Items.Add("All Categories");
        foreach (var category in categories.OrderBy(c => c))
        {
            _categoryDropDown.Items.Add(category);
        }
        _categoryDropDown.SelectedIndex = 0;

        // Holders
        var holders = _searchService.GetUniqueHolderVariants(_allProducts);
        _holderDropDown.Items.Clear();
        _holderDropDown.Items.Add("All Holders");
        foreach (var holder in holders.OrderBy(h => h))
        {
            _holderDropDown.Items.Add(holder);
        }
        _holderDropDown.SelectedIndex = 0;

        // Tags (multi-select)
        _tagListBox.Items.Clear();
        // Tags will be populated from UserDataService when available
        // For now, placeholder
        _tagListBox.Items.Add("(No tags defined)");
    }

    /// <summary>
    /// Get current filter state
    /// </summary>
    private Filters GetCurrentFilters()
    {
        var filters = new Filters();

        // Ranges
        if (_diyCheckBox.Checked == true)
            filters.Ranges.Add("DIY");
        if (_proCheckBox.Checked == true)
            filters.Ranges.Add("PRO");

        // Category
        if (_categoryDropDown.SelectedIndex > 0)
        {
            filters.Categories.Add(_categoryDropDown.SelectedValue?.ToString() ?? "");
        }

        // Holder
        if (_holderDropDown.SelectedIndex > 0)
        {
            filters.HolderVariants.Add(_holderDropDown.SelectedValue?.ToString() ?? "");
        }

        // Tags
        if (_tagListBox.SelectedIndex >= 0 && _tagListBox.SelectedValue != null)
        {
            filters.TagsInclude.Add(_tagListBox.SelectedValue.ToString() ?? "");
        }

        return filters;
    }

    /// <summary>
    /// Clear all filters
    /// </summary>
    public void ClearFilters()
    {
        _diyCheckBox.Checked = false;
        _proCheckBox.Checked = false;
        _categoryDropDown.SelectedIndex = 0;
        _holderDropDown.SelectedIndex = 0;
        _tagListBox.SelectedIndex = -1;

        CurrentFilters = new Filters();
        FiltersChanged?.Invoke(this, new FiltersChangedEventArgs(CurrentFilters));
    }

    /// <summary>
    /// Handle filter changes
    /// </summary>
    private void OnFiltersChanged()
    {
        CurrentFilters = GetCurrentFilters();
        FiltersChanged?.Invoke(this, new FiltersChangedEventArgs(CurrentFilters));
    }

    /// <summary>
    /// Update tag list from user data
    /// </summary>
    public void UpdateTags(List<string> tags)
    {
        _tagListBox.Items.Clear();
        if (tags.Count == 0)
        {
            _tagListBox.Items.Add("(No tags defined)");
        }
        else
        {
            foreach (var tag in tags.OrderBy(t => t))
            {
                _tagListBox.Items.Add(tag);
            }
        }
    }
}

/// <summary>
/// Event args for filter changes
/// </summary>
public class FiltersChangedEventArgs : EventArgs
{
    public Filters Filters { get; }

    public FiltersChangedEventArgs(Filters filters)
    {
        Filters = filters;
    }
}
