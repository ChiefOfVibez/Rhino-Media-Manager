namespace BoschMediaBrowser.Core.Models;

/// <summary>
/// Aggregates all user-specific data for persistence
/// Stored in %AppData%/BoschMediaBrowser/
/// </summary>
public class UserData
{
    /// <summary>
    /// User's favourite products
    /// </summary>
    public List<Favourite> Favourites { get; set; } = new();

    /// <summary>
    /// User's custom tags on products
    /// </summary>
    public List<Tag> Tags { get; set; } = new();

    /// <summary>
    /// User's simple collections (product groups)
    /// </summary>
    public List<Collection> Collections { get; set; } = new();

    /// <summary>
    /// User's local layout collections (placement patterns)
    /// </summary>
    public List<LayoutCollection> LayoutCollections { get; set; } = new();

    /// <summary>
    /// User settings and preferences
    /// </summary>
    public Settings Settings { get; set; } = new();

    /// <summary>
    /// Last data save timestamp
    /// </summary>
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data format version (for migration support)
    /// </summary>
    public int Version { get; set; } = 1;
}
