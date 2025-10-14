namespace BoschMediaBrowser.Core.Models;

/// <summary>
/// Represents a single Bosch product in the media browser.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier (derived from folderPath + productName)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Product display name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// SKU/article code (optional)
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Article code
    /// </summary>
    public string? CodArticol { get; set; }

    /// <summary>
    /// Product range: DIY or PRO
    /// </summary>
    public string Range { get; set; } = string.Empty;

    /// <summary>
    /// Product category (e.g., Garden, Drilling)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional subcategory
    /// </summary>
    public string? Subcategory { get; set; }

    /// <summary>
    /// Top-level category (e.g., "Tools and Holders", "POS", "Graphics")
    /// Derived from folder structure
    /// </summary>
    public string? TopCategory { get; set; }

    /// <summary>
    /// Full category path (e.g., "Tools and Holders>DIY>Garden>Drills")
    /// </summary>
    public string? CategoryPath { get; set; }

    /// <summary>
    /// Category path segments as array
    /// </summary>
    public List<string> PathSegments { get; set; } = new();

    /// <summary>
    /// Absolute path to product folder on network
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Tags for filtering and search
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Internal notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Holders available for this product
    /// </summary>
    public List<Holder> Holders { get; set; } = new();

    /// <summary>
    /// Transform data for each holder variant (relative to reference mesh)
    /// Key: Holder variant name (e.g., "Tego", "Traverse")
    /// Value: Transform data (translation, rotation, scale)
    /// </summary>
    public Dictionary<string, HolderTransform>? HolderTransforms { get; set; }

    /// <summary>
    /// Reference holder variant (the mesh position that all transforms are relative to)
    /// Typically "Tego" is the reference position
    /// </summary>
    public string? ReferenceHolder { get; set; }

    /// <summary>
    /// Packaging information
    /// </summary>
    public Packaging? Packaging { get; set; }

    /// <summary>
    /// Preview image references
    /// </summary>
    public PreviewRefs Previews { get; set; } = new();

    /// <summary>
    /// Metadata timestamps
    /// </summary>
    public ProductMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Holder variant for a product
/// </summary>
public class Holder
{
    public string Variant { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? CodArticol { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string? Preview { get; set; }
}

/// <summary>
/// Transform data for a specific holder variant
/// Stores translation, rotation, and scale relative to reference mesh
/// </summary>
public class HolderTransform
{
    /// <summary>
    /// Translation in X, Y, Z (millimeters or Rhino units)
    /// </summary>
    public double[] Translation { get; set; } = new double[] { 0, 0, 0 };

    /// <summary>
    /// Rotation in degrees (X, Y, Z axis rotations)
    /// </summary>
    public double[] Rotation { get; set; } = new double[] { 0, 0, 0 };

    /// <summary>
    /// Scale factors (X, Y, Z) - typically [1, 1, 1]
    /// </summary>
    public double[] Scale { get; set; } = new double[] { 1, 1, 1 };

    /// <summary>
    /// Optional notes about this transform
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Packaging dimensions and preview
/// </summary>
public class Packaging
{
    public string? FileName { get; set; }
    public string? FullPath { get; set; }
    public double? Length { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? Preview { get; set; }
    public string? PreviewPath { get; set; }
}

/// <summary>
/// Preview image references
/// </summary>
public class PreviewRefs
{
    /// <summary>
    /// Primary product preview image filename (for backwards compatibility)
    /// </summary>
    public string? Product { get; set; }
    
    /// <summary>
    /// Alternate product preview image filename (for backwards compatibility)
    /// </summary>
    public string? ProductAlternate { get; set; }
    
    public PreviewImage? Mesh3d { get; set; }
    public PreviewImage? MeshPreview { get; set; }
    public PreviewImage? Grafica3d { get; set; }
    public PreviewImage? GraficaPreview { get; set; }
    public PreviewImage? ProxyMesh { get; set; }
}

/// <summary>
/// Single preview image/file reference
/// </summary>
public class PreviewImage
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Product metadata
/// </summary>
public class ProductMetadata
{
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModified { get; set; }
}
