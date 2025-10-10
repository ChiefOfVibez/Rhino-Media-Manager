using Eto.Forms;
using Eto.Drawing;
using Rhino;

namespace BoschMediaBrowser.Rhino.UI;

/// <summary>
/// TEST - Exact copy of TestPanel but with MediaBrowserPanel's GUID
/// If this works, the problem is in MediaBrowserPanel class itself
/// If this fails, the problem is the GUID
/// </summary>
[System.Runtime.InteropServices.Guid("A3B5C7D9-1E2F-4A5B-8C9D-0E1F2A3B4C5D")]
public class MediaBrowserPanel2 : Panel
{
    public MediaBrowserPanel2()
    {
        RhinoApp.WriteLine("=== MEDIABROWSERPANEL2 CONSTRUCTOR CALLED ===");
        RhinoApp.WriteLine("This is a TEST copy with MediaBrowserPanel's GUID");
        
        Content = new StackLayout
        {
            Padding = 20,
            Spacing = 10,
            Items =
            {
                new Label 
                { 
                    Text = "MEDIA BROWSER PANEL 2 - TEST VERSION",
                    Font = new Font(SystemFont.Bold, 16),
                    TextColor = Colors.Blue
                },
                new Label
                {
                    Text = "If you see this, the GUID works fine.",
                    TextColor = Colors.Green
                },
                new Label
                {
                    Text = "The problem is in MediaBrowserPanel class.",
                    TextColor = Colors.Orange
                },
                new TextBox { Text = "This is a test textbox" },
                new Button { Text = "Test Button" }
            }
        };
        
        RhinoApp.WriteLine("=== MEDIABROWSERPANEL2 CONSTRUCTOR COMPLETE ===");
    }
}
