using Rhino;
using Rhino.PlugIns;
using Rhino.UI;

namespace BoschMediaBrowser.Rhino;

/// <summary>
/// Main plugin class for Bosch Media Browser
/// </summary>
[System.Runtime.InteropServices.Guid("B05C43D0-ED1A-4B05-8E12-345678ABCDEF")]
public class BoschMediaBrowserPlugin : PlugIn
{
    public BoschMediaBrowserPlugin()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the plugin instance
    /// </summary>
    public static BoschMediaBrowserPlugin? Instance { get; private set; }

    /// <summary>
    /// Called when the plugin is loaded
    /// </summary>
    protected override LoadReturnCode OnLoad(ref string errorMessage)
    {
        try
        {
            RhinoApp.WriteLine("==========================================");
            RhinoApp.WriteLine("=== BOSCH MEDIA BROWSER PLUGIN LOADING ===");
            RhinoApp.WriteLine("==========================================");
            RhinoApp.WriteLine($"Plugin GUID: {this.Id}");
            RhinoApp.WriteLine($"Plugin Name: {this.Name}");
            RhinoApp.WriteLine($"Plugin Version: {this.Version}");
            
            // Register the TEST panel (with different GUID)
            RhinoApp.WriteLine("");
            RhinoApp.WriteLine("Registering TEST panel...");
            Panels.RegisterPanel(
                this,
                typeof(UI.TestPanel),
                "Test Panel",
                null
            );
            RhinoApp.WriteLine($"Test Panel Type: {typeof(UI.TestPanel).FullName}");
            RhinoApp.WriteLine($"Test Panel GUID: {typeof(UI.TestPanel).GUID}");
            
            // Register the SIMPLIFIED media browser panel
            RhinoApp.WriteLine("Registering SIMPLIFIED MediaBrowserPanel...");
            Panels.RegisterPanel(
                this,
                typeof(UI.MediaBrowserPanel),
                "Bosch Media Browser",
                null
            );
            RhinoApp.WriteLine($"Panel Type: {typeof(UI.MediaBrowserPanel).FullName}");
            RhinoApp.WriteLine($"Panel GUID: {typeof(UI.MediaBrowserPanel).GUID}");
            
            // IMPORTANT: Close the panel if it was auto-opened by Rhino
            // This prevents crash on startup when Rhino tries to restore panel state
            try
            {
                RhinoApp.WriteLine("Checking if panel is visible...");
                var panelId = typeof(UI.MediaBrowserPanel).GUID;
                if (Panels.IsPanelVisible(panelId))
                {
                    RhinoApp.WriteLine("Panel is visible - closing to prevent startup crash...");
                    Panels.ClosePanel(panelId);
                    RhinoApp.WriteLine("Panel closed successfully.");
                }
                else
                {
                    RhinoApp.WriteLine("Panel is not visible - good!");
                }
            }
            catch (Exception panelEx)
            {
                RhinoApp.WriteLine($"Could not check/close panel (expected on first run): {panelEx.Message}");
            }

            RhinoApp.WriteLine("");
            RhinoApp.WriteLine("==========================================");
            RhinoApp.WriteLine("Bosch Media Browser plugin loaded successfully!");
            RhinoApp.WriteLine("==========================================");
            RhinoApp.WriteLine("Run 'ShowMediaBrowser' command to open the panel.");
            RhinoApp.WriteLine("Run 'ShowTestPanel' command to test basic panel functionality.");
            RhinoApp.WriteLine("");

            return LoadReturnCode.Success;
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load Bosch Media Browser: {ex.Message}";
            RhinoApp.WriteLine(errorMessage);
            RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
            return LoadReturnCode.ErrorShowDialog;
        }
    }

    /// <summary>
    /// Called when the plugin is unloaded
    /// </summary>
    protected override void OnShutdown()
    {
        RhinoApp.WriteLine("Bosch Media Browser plugin unloaded.");
        base.OnShutdown();
    }
}
