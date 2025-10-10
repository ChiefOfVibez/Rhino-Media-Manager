namespace BoschMediaBrowser.Core.Models;

/// <summary>
/// User-defined collection of products for quick recall
/// </summary>
public class Collection
{
    /// <summary>
    /// Unique collection identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Collection display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product IDs in this collection
    /// </summary>
    public List<string> ProductIds { get; set; } = new();

    /// <summary>
    /// When the collection was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? LastModified { get; set; }
}
