using AquaParkManager.Models;
using AquaParkManager.Windows;
using System.Windows;

namespace AquaParkManager
{
    public partial class App : Application
    {
        public static User? CurrentUser { get; set; } // Null = Neregistrovaný
        public static string? ConnectionString { get; set; }
        public static User? AdminUser { get; set; } // Saved admin for emulation
        public static bool IsEmulated { get; set; } = false; // True when viewing as another user

        protected override void OnStartup(StartupEventArgs e)
        {
            try
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

                // App only shuts down when we explicitly call Shutdown()
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při spuštění aplikace: {ex.Message}\n\n{ex.StackTrace}", "Chyba spuštění");
                System.Diagnostics.Debug.WriteLine($"OnStartup Error: {ex}");
                Shutdown();
            }
        }
    }
}