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

        private void BtnAttractions_Click(object sender, RoutedEventArgs e)
        {
            var attractionsWindow = new AttractionsManagementWindow();
            attractionsWindow.ShowDialog();
        }

        private void BtnMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var maintenanceWindow = new MaintenanceManagementWindow();
            maintenanceWindow.ShowDialog();
        }
    }
}