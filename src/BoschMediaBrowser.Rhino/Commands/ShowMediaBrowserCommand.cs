using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace BoschMediaBrowser.Rhino.Commands;

/// <summary>
/// Command to show/toggle the Bosch Media Browser panel
/// </summary>
public class ShowMediaBrowserCommand : Command
{
    public ShowMediaBrowserCommand()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the command instance
    /// </summary>
    public static ShowMediaBrowserCommand? Instance { get; private set; }

    /// <summary>
    /// English command name (localization not yet implemented)
    /// </summary>
    public override string EnglishName => "ShowMediaBrowser";

    /// <summary>
    /// Execute the command
    /// </summary>
    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        try
        {
            RhinoApp.WriteLine("=== ShowMediaBrowser Command Started ===");
            
            // Get or create the panel
            var panelId = typeof(UI.MediaBrowserPanel).GUID;
            RhinoApp.WriteLine($"Panel GUID: {panelId}");

            // Check if panel is already visible
            var isVisible = Panels.IsPanelVisible(panelId);
            RhinoApp.WriteLine($"Panel currently visible: {isVisible}");

            if (isVisible)
            {
                // Toggle off (close)
                Panels.ClosePanel(panelId);
                RhinoApp.WriteLine("Bosch Media Browser panel closed.");
            }
            else
            {
                // Show the panel
                RhinoApp.WriteLine("Calling Panels.OpenPanel...");
                Panels.OpenPanel(panelId);
                RhinoApp.WriteLine("Bosch Media Browser panel opened.");
            }

            return Result.Success;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Error showing media browser: {ex.Message}");
            RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
            return Result.Failure;
        }
    }
}
