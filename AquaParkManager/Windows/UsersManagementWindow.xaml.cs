using System.Linq;
using System.Windows;
using AquaParkManager.Models;

namespace AquaParkManager.Windows
{
    public partial class UsersManagementWindow : Window
    {
        public UsersManagementWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var ctx = new AquaParkContext())
            {
                dgUsers.ItemsSource = ctx.Users.ToList();
            }
        }

        private void BtnEmulate_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is User selectedUser)
            {
                if (MessageBox.Show($"Opravdu se chcete přepnout na uživatele {selectedUser.Username}?", "Emulace", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // EMULACE UŽIVATELE
                    App.CurrentUser = selectedUser;
                    MessageBox.Show($"Nyní jste přihlášen jako: {selectedUser.Username}");

                    // Restart hlavního okna pro aplikaci práv
                    var newMain = new MainWindow();
                    Application.Current.MainWindow.Close();
                    Application.Current.MainWindow = newMain;
                    newMain.Show();

                    this.Close();
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}