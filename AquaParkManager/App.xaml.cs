using AquaParkManager.Models;
using AquaParkManager.Windows;
using System.Windows;

namespace AquaParkManager
{
    public partial class App : Application
    {
        public static User? CurrentUser { get; set; } // Null = Neregistrovaný
        public static string? ConnectionString { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Zabrání vypnutí aplikace po zavření konfiguračního okna
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var connectionWindow = new ConnectionConfigWindow();
            if (connectionWindow.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            // Výchozí stav: Nepřihlášen
            CurrentUser = null;

            // Otevření hlavního okna
            var main = new MainWindow();
            Application.Current.MainWindow = main;
            main.Show();

            // Přepnutí režimu vypínání
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}