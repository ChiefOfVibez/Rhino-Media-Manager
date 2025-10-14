using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BoschMediaBrowser.Rhino.Commands;

/// <summary>
/// Command to create a collection from selected Bosch products in Rhino
/// </summary>
public class CreateCollectionCommand : Command
{
    public CreateCollectionCommand()
    {
        Instance = this;
    }

    public static CreateCollectionCommand Instance { get; private set; }

    public override string EnglishName => "CreateBoschCollection";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        try
        {
            // Get selected objects
            var selectedObjs = doc.Objects.GetSelectedObjects(false, false)?.ToArray();
            if (selectedObjs == null || selectedObjs.Length == 0)
            {
                RhinoApp.WriteLine("No objects selected. Please select Bosch products to create a collection.");
                return Result.Cancel;
            }

            // Filter to only Bosch products (those with BoschProductId user data)
            var boschProducts = new List<CollectionItem>();
            
            foreach (var obj in selectedObjs)
            {
                var productId = obj.Attributes.GetUserString("BoschProductId");
                if (!string.IsNullOrEmpty(productId))
                {
                    var productName = obj.Attributes.GetUserString("BoschProductName");
                    var productSKU = obj.Attributes.GetUserString("BoschProductSKU");
                    
                    // Get the instance reference to extract transform
                    if (obj.Geometry is global::Rhino.Geometry.InstanceReferenceGeometry instRef)
                    {
                        var xform = instRef.Xform;
                        
                        boschProducts.Add(new CollectionItem
                        {
                            ProductId = productId,
                            ProductName = productName ?? "Unknown",
                            SKU = productSKU,
                            Transform = TransformToArray(xform)
                        });
                    }
                }
            }

            if (boschProducts.Count == 0)
            {
                RhinoApp.WriteLine("No Bosch products found in selection. Please select products inserted from the Media Browser.");
                return Result.Cancel;
            }

            RhinoApp.WriteLine($"Found {boschProducts.Count} Bosch product(s) in selection.");

            // Prompt for collection name
            var gs = new GetString();
            gs.SetCommandPrompt("Enter collection name");
            gs.AcceptNothing(false);
            var result = gs.Get();
            
            if (result != GetResult.String)
                return Result.Cancel;
            
            var collectionName = gs.StringResult();
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                RhinoApp.WriteLine("Invalid collection name.");
                return Result.Cancel;
            }

            // Create collection object
            var collection = new Collection
            {
                Name = collectionName,
                Description = $"Created from {boschProducts.Count} selected products",
                Items = boschProducts,
                CreatedDate = DateTime.UtcNow
            };

            // Save to public collections folder
            var basePath = GetCollectionsPath();
            if (string.IsNullOrEmpty(basePath))
            {
                RhinoApp.WriteLine("ERROR: Could not determine collections path. Check settings.");
                return Result.Failure;
            }

            var fileName = SanitizeFileName(collectionName) + ".json";
            var filePath = Path.Combine(basePath, fileName);

            // Serialize to JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(collection, options);
            File.WriteAllText(filePath, json);

            RhinoApp.WriteLine($"SUCCESS: Collection '{collectionName}' created with {boschProducts.Count} items.");
            RhinoApp.WriteLine($"Saved to: {filePath}");

            return Result.Success;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"ERROR creating collection: {ex.Message}");
            RhinoApp.WriteLine($"Stack: {ex.StackTrace}");
            return Result.Failure;
        }
    }

    private string? GetCollectionsPath()
    {
        // Try to get from settings service
        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BoschMediaBrowser",
            "settings.json"
        );

        if (File.Exists(settingsPath))
        {
            try
            {
                var json = File.ReadAllText(settingsPath);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("publicCollectionsPath", out var pathProp))
                {
                    var path = pathProp.GetString();
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        return path;
                }
                
                // Fallback to base path + _public-collections
                if (doc.RootElement.TryGetProperty("baseServerPath", out var baseProp))
                {
                    var basePath = baseProp.GetString();
                    if (!string.IsNullOrEmpty(basePath))
                    {
                        var collectionsPath = Path.Combine(basePath, "_public-collections");
                        if (!Directory.Exists(collectionsPath))
                        {
                            Directory.CreateDirectory(collectionsPath);
                        }
                        return collectionsPath;
                    }
                }
            }
            catch { }
        }

        // Last resort: use default
        var defaultPath = @"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\_public-collections";
        if (!Directory.Exists(defaultPath))
        {
            try
            {
                Directory.CreateDirectory(defaultPath);
            }
            catch
            {
                return null;
            }
        }
        return defaultPath;
    }

    private double[] TransformToArray(global::Rhino.Geometry.Transform xform)
    {
        // Store transform as 16-element array (4x4 matrix)
        return new[]
        {
            xform.M00, xform.M01, xform.M02, xform.M03,
            xform.M10, xform.M11, xform.M12, xform.M13,
            xform.M20, xform.M21, xform.M22, xform.M23,
            xform.M30, xform.M31, xform.M32, xform.M33
        };
    }

    private string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}

/// <summary>
/// Collection data structure
/// </summary>
public class Collection
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CollectionItem> Items { get; set; } = new();
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Item in a collection with transform
/// </summary>
public class CollectionItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public double[] Transform { get; set; } = new double[16];
}
