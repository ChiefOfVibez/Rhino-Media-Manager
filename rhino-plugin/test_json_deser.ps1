# Quick test to deserialize a JSON file using the Product model
$ErrorActionPreference = "Stop"

# Build the test console app
$testCode = @'
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

// Product model (minimal)
public class Product
{
    public string Id { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string Range { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<Holder> Holders { get; set; } = new();
    public Dictionary<string, HolderTransform>? HolderTransforms { get; set; }
    public string? ReferenceHolder { get; set; }
    public Packaging? Packaging { get; set; }
    public PreviewRefs Previews { get; set; } = new();
    public ProductMetadata Metadata { get; set; } = new();
}

public class Holder
{
    public string Variant { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? CodArticol { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string? Preview { get; set; }
}

public class HolderTransform
{
    public double[] Translation { get; set; } = new double[] { 0, 0, 0 };
    public double[] Rotation { get; set; } = new double[] { 0, 0, 0 };
    public double[] Scale { get; set; } = new double[] { 1, 1, 1 };
    public string? Notes { get; set; }
}

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

public class PreviewRefs
{
    public PreviewImage? Mesh3d { get; set; }
    public PreviewImage? MeshPreview { get; set; }
    public PreviewImage? Grafica3d { get; set; }
    public PreviewImage? GraficaPreview { get; set; }
}

public class PreviewImage
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
}

public class ProductMetadata
{
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModified { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: test_json_deser.exe <json-file-path>");
            return;
        }

        string jsonPath = args[0];
        try
        {
            string json = File.ReadAllText(jsonPath);
            Console.WriteLine($"Reading: {jsonPath}");
            Console.WriteLine($"JSON length: {json.Length} chars\n");

            var product = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (product != null)
            {
                Console.WriteLine($"✓ SUCCESS!");
                Console.WriteLine($"  ProductName: {product.ProductName}");
                Console.WriteLine($"  Range: {product.Range}");
                Console.WriteLine($"  Category: {product.Category}");
                Console.WriteLine($"  Holders: {product.Holders.Count}");
                Console.WriteLine($"  HolderTransforms: {product.HolderTransforms?.Count ?? 0}");
                Console.WriteLine($"  ReferenceHolder: {product.ReferenceHolder}");
                Console.WriteLine($"  Packaging Length: {product.Packaging?.Length}");
            }
            else
            {
                Console.WriteLine("✗ Deserialization returned NULL");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"✗ JSON ERROR: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  Inner: {ex.InnerException.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ ERROR: {ex.GetType().Name} - {ex.Message}");
        }
    }
}
'@

Write-Host "Testing JSON deserialization..."
Write-Host "Compiling test program..." -ForegroundColor Cyan

# Save and compile
$testFile = "temp_test.cs"
$exeFile = "temp_test.exe"
$testCode | Out-File -FilePath $testFile -Encoding UTF8

csc /nologo /out:$exeFile $testFile 2>&1 | Out-Null

if (-not (Test-Path $exeFile)) {
    Write-Host "Failed to compile test program" -ForegroundColor Red
    exit 1
}

Write-Host "Running test on GHE 18V-60.json..." -ForegroundColor Cyan
& .\$exeFile "M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders\PRO\Garden\GHE 18V-60\GHE 18V-60.json"

Write-Host "`nRunning test on GBL 18V-750.json (working)..." -ForegroundColor Cyan
& .\$exeFile "M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Tools and Holders\PRO\Garden\GBL 18V-750\GBL 18V-750.json"

# Cleanup
Remove-Item $testFile -ErrorAction SilentlyContinue
Remove-Item $exeFile -ErrorAction SilentlyContinue

Write-Host "`nTest complete!" -ForegroundColor Green
