namespace BoschMediaBrowser.Core.Models;

/// <summary>
/// User-specific favourite marker for a product
/// </summary>
public class Favourite
{
    /// <summary>
    /// Product identifier marked as favourite
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// When the product was favourited
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
