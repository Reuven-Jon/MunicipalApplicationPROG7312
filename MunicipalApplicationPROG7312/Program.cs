using MunicipalApplicationPROG7312.UI;
using System;
using System.Windows.Forms;

namespace MunicipalApplication
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();   // High-DPI + defaults for .NET 8
            Application.Run(new MainForm());         // Start on your main menu
        }
    }
}
