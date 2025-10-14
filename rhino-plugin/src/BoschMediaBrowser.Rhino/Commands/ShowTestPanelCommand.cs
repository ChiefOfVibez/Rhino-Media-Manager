using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace BoschMediaBrowser.Rhino.Commands;

/// <summary>
/// Command to test basic panel functionality
/// </summary>
public class ShowTestPanelCommand : Command
{
    public ShowTestPanelCommand()
    {
        Instance = this;
    }

    public static ShowTestPanelCommand? Instance { get; private set; }

    public override string EnglishName => "ShowTestPanel";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        try
        {
            RhinoApp.WriteLine("=== ShowTestPanel Command Started ===");
            
            var panelId = typeof(UI.TestPanel).GUID;
            RhinoApp.WriteLine($"Test Panel GUID: {panelId}");

            var isVisible = Panels.IsPanelVisible(panelId);
            RhinoApp.WriteLine($"Panel currently visible: {isVisible}");

            if (isVisible)
            {
                Panels.ClosePanel(panelId);
                RhinoApp.WriteLine("Test panel closed.");
            }
            else
            {
                RhinoApp.WriteLine("Calling Panels.OpenPanel...");
                Panels.OpenPanel(panelId);
                RhinoApp.WriteLine("Test panel opened.");
            }

            return Result.Success;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Error showing test panel: {ex.Message}");
            RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
            return Result.Failure;
        }
    }
}
