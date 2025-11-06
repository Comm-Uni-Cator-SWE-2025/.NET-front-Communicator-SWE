using System.Windows;

namespace UX.Icons;

/// <summary>
/// Demo application to showcase the icon library
/// Run this project to see all available icons
/// </summary>
public class DemoApp
{
    [STAThread]
    public static void Main()
    {
        var app = new Application();
        var window = new IconShowcaseWindow();
        app.Run(window);
    }
}
