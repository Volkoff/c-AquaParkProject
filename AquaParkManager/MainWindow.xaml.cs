using System.Linq;
using System.Windows;
using AquaParkManager.Models;
using AquaParkManager.Windows;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyRoles();
        }

        private void ApplyRoles()
        {
            bool isAdmin = false;

            // Bezpečná kontrola CurrentUser
            if (App.CurrentUser != null)
            {
                using (var ctx = new AquaParkContext())
                {
                    var roles = ctx.UserRoles.Include(ur => ur.Role)
                                   .Where(ur => ur.UserId == App.CurrentUser.UserId)
                                   .Select(ur => ur.Role != null ? ur.Role.RoleName : "")
                                   .ToList();
                    isAdmin = roles.Contains("ADMIN") || roles.Contains("MANAGER");
                }
            }

            if (App.CurrentUser == null)
            {
                this.Title = "Aqua Park - Neregistrovaný návštěvník";
                txtUserStatus.Text = "Prohlížíte jako host";

                pnlAdminModules.Visibility = Visibility.Collapsed;
                // Protože pnlAdminModules skrývá vše uvnitř, nemusíme skrývat jednotlivá tlačítka
                btnLogin.Content = "Přihlásit se";
            }
            else
            {
                this.Title = $"Přihlášen: {App.CurrentUser.Username}" + (isAdmin ? " (ADMIN)" : "");
                txtUserStatus.Text = $"Uživatel: {App.CurrentUser.Username}";
                btnLogin.Content = "Odhlásit se";

                pnlAdminModules.Visibility = Visibility.Visible;

                // Tlačítka specifická pro Admina
                btnUsers.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

                // DB Info vidí každý přihlášený (splnění bodu 30)
                btnDbInfo.Visibility = Visibility.Visible;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                var login = new LoginWindow();
                if (login.ShowDialog() == true)
                {
                    // Refresh po přihlášení
                    var newMain = new MainWindow();
                    Application.Current.MainWindow = newMain;
                    newMain.Show();
                    this.Close();
                }
            }
            else
            {
                App.CurrentUser = null;
                var newMain = new MainWindow();
                Application.Current.MainWindow = newMain;
                newMain.Show();
                this.Close();
            }
        }

        // Navigace
        private void BtnTickets_Click(object sender, RoutedEventArgs e) => new TicketsManagementWindow().ShowDialog();
        private void BtnPools_Click(object sender, RoutedEventArgs e) => new PoolsManagementWindow().ShowDialog();
        private void BtnSlides_Click(object sender, RoutedEventArgs e) => new SlidesManagementWindow().ShowDialog();
        private void BtnBookings_Click(object sender, RoutedEventArgs e) => new BookingsManagementWindow().ShowDialog();
        private void BtnStaff_Click(object sender, RoutedEventArgs e) => new StaffManagementWindow().ShowDialog();
        private void BtnVisitors_Click(object sender, RoutedEventArgs e) => new VisitorsManagementWindow().ShowDialog();
        private void BtnMaintenance_Click(object sender, RoutedEventArgs e) => new MaintenanceManagementWindow().ShowDialog();
        private void BtnInventory_Click(object sender, RoutedEventArgs e) => new InventoryManagementWindow().ShowDialog();
        private void BtnCertifications_Click(object sender, RoutedEventArgs e) => new CertificationsManagementWindow().ShowDialog();
        private void BtnScheduling_Click(object sender, RoutedEventArgs e) => new SchedulingWindow().ShowDialog();
        private void BtnReports_Click(object sender, RoutedEventArgs e) => new ReportsAnalyticsWindow().ShowDialog();
        private void BtnMedia_Click(object sender, RoutedEventArgs e) => new MediaManagementWindow().ShowDialog();

        // Nová okna (musí existovat)
        private void BtnUsers_Click(object sender, RoutedEventArgs e) => new UsersManagementWindow().ShowDialog();
        private void BtnDbInfo_Click(object sender, RoutedEventArgs e) => new DatabaseObjectsWindow().ShowDialog();
    }
}