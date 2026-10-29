using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]

namespace Doxie.HelpFileGenerator.GUI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
