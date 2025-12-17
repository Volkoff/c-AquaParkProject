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
                    // Save current admin user
                    App.AdminUser = App.CurrentUser;
                    
                    // Switch to emulated user
                    App.CurrentUser = selectedUser;
                    App.IsEmulated = true;
                    
                    MessageBox.Show($"Nyní jste v emulaci jako: {selectedUser.Username}\n\nKlikněte na tlačítko 'Vrátit se na ADMIN' pro návrat.", "Emulace aktivní");
                    
                    // Restart hlavního okna pro aplikaci práv
                    var oldMain = Application.Current.MainWindow;
                    var newMain = new MainWindow();
                    
                    Application.Current.MainWindow = newMain;
                    oldMain.Close();
                    newMain.Show();
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}