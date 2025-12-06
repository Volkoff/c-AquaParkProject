using AquaParkManager.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace AquaParkManager.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
                    // Vypočteme hash zadaného hesla
                    string hashedPassword = ComputeSha256Hash(password);

                    // Hledáme uživatele. Pro kompatibilitu s testovacími daty (pokud nejsou zahashovaná)
                    // kontrolujeme jak hash, tak čistý text (fallback).
                    var user = context.Users.FirstOrDefault(u => u.Username == username &&
                               (u.PasswordHash == hashedPassword || u.PasswordHash == password));

                    if (user != null)
                    {
                        if (user.IsActive == "N")
                        {
                            lblError.Text = "Account is disabled.";
                            return;
                        }

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

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}