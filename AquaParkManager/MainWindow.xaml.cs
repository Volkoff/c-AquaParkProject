using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AquaParkManager.Windows;

namespace AquaParkManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Zobrazení jména v titulku nebo status baru
            if (App.CurrentUser != null)
            {
                this.Title = $"Aqua Park Manager - Logged in as: {App.CurrentUser.Username}";
            }
        }

        // Přidej metodu pro Logout (např. na kliknutí tlačítka v menu)
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void BtnStaff_Click(object sender, RoutedEventArgs e)
        {
            var staffWindow = new StaffManagementWindow();
            staffWindow.ShowDialog();
        }

        private void BtnPools_Click(object sender, RoutedEventArgs e)
        {
            var poolsWindow = new PoolsManagementWindow();
            poolsWindow.ShowDialog();
        }

        private void BtnSlides_Click(object sender, RoutedEventArgs e)
        {
            var slidesWindow = new SlidesManagementWindow();
            slidesWindow.ShowDialog();
        }

        private void BtnVisitors_Click(object sender, RoutedEventArgs e)
        {
            var visitorsWindow = new VisitorsManagementWindow();
            visitorsWindow.ShowDialog();
        }

        private void BtnTickets_Click(object sender, RoutedEventArgs e)
        {
            var ticketsWindow = new TicketsManagementWindow();
            ticketsWindow.ShowDialog();
        }

        private void BtnBookings_Click(object sender, RoutedEventArgs e)
        {
            var bookingsWindow = new BookingsManagementWindow();
            bookingsWindow.ShowDialog();
        }


        private void BtnMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var maintenanceWindow = new MaintenanceManagementWindow();
            maintenanceWindow.ShowDialog();
        }

        private void BtnCertifications_Click(object sender, RoutedEventArgs e)
        {
            var certificationsWindow = new CertificationsManagementWindow();
            certificationsWindow.ShowDialog();
        }

        private void BtnInventory_Click(object sender, RoutedEventArgs e)
        {
            var inventoryWindow = new InventoryManagementWindow();
            inventoryWindow.ShowDialog();
        }

        private void BtnScheduling_Click(object sender, RoutedEventArgs e)
        {
            var schedulingWindow = new SchedulingWindow();
            schedulingWindow.ShowDialog();
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsAnalyticsWindow();
            reportsWindow.ShowDialog();
        }

        private void BtnMedia_Click(object sender, RoutedEventArgs e)
        {
            var mediaWindow = new MediaManagementWindow();
            mediaWindow.ShowDialog();
        }
        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            var schedulingWindow = new SchedulingWindow();
            schedulingWindow.ShowDialog();
        }

        private void BtnAnalytics_Click(object sender, RoutedEventArgs e)
        {
            var analyticsWindow = new ReportsAnalyticsWindow();
            analyticsWindow.ShowDialog();
        }
    }
}