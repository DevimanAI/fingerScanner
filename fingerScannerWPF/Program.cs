csharp
using System;
using System.Windows;

namespace fingerScannerWPF
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                var application = new Application();
                var mainWindow = new MainWindow();
                application.Run(mainWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}