namespace BoschMediaBrowser.Core.Models;

/// <summary>
/// User settings and preferences
/// </summary>
public class Settings
{
    /// <summary>
    /// Base path to product database on network
    /// </summary>
    public string BaseServerPath { get; set; } = @"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__";

    /// <summary>
    /// Path to public collections folder
    /// </summary>
    public string PublicCollectionsPath { get; set; } = string.Empty;

    /// <summary>
    /// Default to linked insert (vs embedded)
    /// </summary>
    public bool LinkedInsertDefault { get; set; } = true;

    /// <summary>
    /// Default grid spacing in millimeters
    /// </summary>
    public double GridSpacing { get; set; } = 1200.0;

    /// <summary>
    /// Thumbnail size in pixels (128, 192, 256)
    /// </summary>
    public int ThumbnailSize { get; set; } = 192;

    /// <summary>
    /// Path to local thumbnail cache
    /// </summary>
    public string ThumbnailCachePath { get; set; } = string.Empty;

    /// <summary>
    /// Last used filter state
    /// </summary>
    public Filters LastUsedFilters { get; set; } = new();
}

/// <summary>
/// Filter state for product browsing
/// </summary>
public class Filters
{
    public string SearchText { get; set; } = string.Empty;
    public List<string> Ranges { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> HolderVariants { get; set; } = new();
    public List<string> TagsInclude { get; set; } = new();
    public string SortBy { get; set; } = "Name";
    public string SortDir { get; set; } = "asc";
}
