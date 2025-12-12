using AquaParkManager.Models;
using AquaParkManager.Windows;
using System.Windows;

namespace AquaParkManager
{
    public partial class App : Application
    {
        // Globální proměnná pro přihlášeného uživatele
        public static User? CurrentUser { get; set; }
        // Selected connection string provided at startup (set by ConnectionConfigWindow)
        public static string? ConnectionString { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Prevent app from closing when dialogs close before MainWindow is set
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            // Prompt for database connection before showing login
            while (true)
            {
                var connectionWindow = new ConnectionConfigWindow();
                var result = connectionWindow.ShowDialog();

                if (result == true && !string.IsNullOrWhiteSpace(ConnectionString))
                {
                    var login = new LoginWindow();
                    login.Show();
                    break;
                }

                if (result != true)
                {
                    // User cancelled; shut down gracefully
                    Shutdown();
                    break;
                }

                MessageBox.Show("Please provide a connection string to continue.");
            }
        }
    }
}