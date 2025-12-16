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

            if (string.IsNullOrEmpty(username))
            {
                lblError.Text = "Zadejte uživatelské jméno.";
                return;
            }

            try
            {
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
                lblError.Text = $"Chyba DB: {ex.Message}";
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegisterWindow();
            reg.ShowDialog();
        }
    }
}