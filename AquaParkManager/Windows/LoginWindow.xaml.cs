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
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při inicializaci okna přihlášení: {ex.Message}\n\n{ex.StackTrace}", "Chyba aplikace");
                throw;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrEmpty(username))
                {
                    lblError.Text = "Zadejte uživatelské jméno.";
                    return;
                }

                using (var context = new AquaParkContext())
                {
                    // Poznámka: V reálné aplikaci hashujte hesla.
                    var user = context.Users.FirstOrDefault(u => u.Username == username);

                    if (user != null) // Zde zjednodušeně bez kontroly hesla pro demo, přidejte && user.Password == ...
                    {
                        App.CurrentUser = user;
                        this.DialogResult = true; // Zavře okno a vrátí true do MainWindow
                        this.Close();
                    }
                    else
                    {
                        lblError.Text = "Neplatné údaje.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Chyba: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"BtnLogin_Click Error: {ex}");
                MessageBox.Show($"Chyba při přihlášení: {ex.Message}\n\n{ex.StackTrace}", "Chyba přihlášení");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reg = new RegisterWindow();
                reg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při otevírání registrace: {ex.Message}", "Chyba");
                System.Diagnostics.Debug.WriteLine($"BtnRegister_Click Error: {ex}");
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                BtnLogin_Click(null, null);
                e.Handled = true;
            }
        }
    }
}