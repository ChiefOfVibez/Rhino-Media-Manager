using Eto.Forms;
using Eto.Drawing;
using Rhino;

namespace BoschMediaBrowser.Rhino.UI;

/// <summary>
/// Minimal test panel to verify Rhino panel mechanism
/// </summary>
[System.Runtime.InteropServices.Guid("F1E2D3C4-B5A6-9786-5432-1A2B3C4D5E6F")]
public class TestPanel : Panel
{
    public TestPanel()
    {
        RhinoApp.WriteLine("=== TEST PANEL CONSTRUCTOR CALLED ===");
        
        Content = new StackLayout
        {
            Padding = 20,
            Spacing = 10,
            Items =
            {
                new Label 
                { 
                    Text = "TEST PANEL - If you see this, panels work!",
                    Font = new Font(SystemFont.Bold, 16),
                    TextColor = Colors.Green
                },
                new TextBox { Text = "This is a test textbox" },
                new Button { Text = "Test Button" }
            }
        };
        
        RhinoApp.WriteLine("=== TEST PANEL CONSTRUCTOR COMPLETE ===");
    }
}
