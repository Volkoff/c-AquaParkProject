using System.Configuration;
using System.Data;
using System.Windows;

namespace AquaParkManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize database with sample data
            try
            {
                DatabaseInitializer.Initialize();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
