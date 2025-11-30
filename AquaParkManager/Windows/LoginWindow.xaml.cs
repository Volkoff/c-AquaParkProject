using AquaParkManager.Models;
using System;
using System.Linq;
using System.Windows;

namespace AquaParkManager.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Please enter username and password.";
                return;
            }

            try
            {
                using (var context = new AquaParkContext())
                {
                    // V reálné aplikaci by zde mělo být hashování hesla (SHA256).
                    // Pro účely seed dat ('hashed_secret_123') porovnáme přímo.

                    var user = context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

                    if (user != null)
                    {
                        // Přihlášení úspěšné
                        App.CurrentUser = user; // Uložíme si uživatele

                        MainWindow main = new MainWindow();
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        lblError.Text = "Invalid username or password.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Database Error: {ex.Message}";
            }
        }
    }
}