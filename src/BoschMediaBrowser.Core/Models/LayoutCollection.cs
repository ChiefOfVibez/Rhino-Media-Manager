namespace BoschMediaBrowser.Core.Models;

/// <summary>
/// Represents a saved layout from a Rhino scene with placement transforms
/// Can be shared (Public scope) or user-specific (Local scope)
/// </summary>
public class LayoutCollection
{
    /// <summary>
    /// Unique layout collection identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Layout collection display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Scope: Public (shared) or Local (user-specific)
    /// </summary>
    public CollectionScope Scope { get; set; } = CollectionScope.Local;

    /// <summary>
    /// Items in the layout with their transforms
    /// </summary>
    public List<LayoutItem> Items { get; set; } = new();

    /// <summary>
    /// When the layout collection was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Single item in a layout collection with placement transform
/// </summary>
public class LayoutItem
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Selected holder variant (e.g., "Type 1_TEGO_RAL9006") or "NONE"
    /// </summary>
    public string HolderVariant { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include the holder in the assembly
    /// </summary>
    public bool IncludeHolder { get; set; } = true;

    /// <summary>
    /// Placement transform (position and rotation)
    /// </summary>
    public Transform Transform { get; set; } = new();
}

/// <summary>
/// Transform data for object placement (position and rotation)
/// </summary>
public class Transform
{
    /// <summary>
    /// X position in millimeters
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y position in millimeters
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Z position in millimeters
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// Rotation around X axis in degrees
    /// </summary>
    public double Rx { get; set; }

    /// <summary>
    /// Rotation around Y axis in degrees
    /// </summary>
    public double Ry { get; set; }

    /// <summary>
    /// Rotation around Z axis in degrees
    /// </summary>
    public double Rz { get; set; }
}

/// <summary>
/// Collection scope enumeration
/// </summary>
public enum CollectionScope
{
    /// <summary>
    /// Local to user (stored in %AppData%)
    /// </summary>
    Local,

    /// <summary>
    /// Public/shared (stored in network _public-collections folder)
    /// </summary>
    Public
}
