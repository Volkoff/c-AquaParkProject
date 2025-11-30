using AquaParkManager.Models;
using AquaParkManager.Windows;
using System.Windows;

namespace AquaParkManager
{
    public partial class App : Application
    {
        // Globální proměnná pro přihlášeného uživatele
        public static User? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Startujeme Login oknem
            LoginWindow login = new LoginWindow();
            login.Show();
        }
    }
}