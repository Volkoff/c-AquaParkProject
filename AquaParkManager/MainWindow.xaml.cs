using System.Windows;
using AquaParkManager.Windows;

namespace AquaParkManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnFac_Click(object sender, RoutedEventArgs e) { new FacilitiesWindow().ShowDialog(); }
        private void BtnHR_Click(object sender, RoutedEventArgs e) { new HRWindow().ShowDialog(); }
        private void BtnSales_Click(object sender, RoutedEventArgs e) { new SalesWindow().ShowDialog(); }
    }
}