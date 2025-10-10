namespace BoschMediaBrowser.Core.Models;

/// <summary>
/// User-defined tag attached to a product
/// </summary>
public class Tag
{
    /// <summary>
    /// Product identifier this tag is attached to
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Tag name/label
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the tag was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
