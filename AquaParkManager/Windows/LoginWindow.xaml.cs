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
                    var user = context.Users.FirstOrDefault(u => u.Username == username);

                    if (user != null)
                    {
                        // Bezpečné ověření hesla s využitím SALT z DB
                        string computedHash;
                        if (!string.IsNullOrEmpty(user.PasswordSalt))
                        {
                            computedHash = ComputeSha256Hash(password, user.PasswordSalt);
                        }
                        else
                        {
                            // Fallback pro stará data bez soli
                            computedHash = ComputeSha256Hash(password, null);
                        }

                        // Porovnání hashe nebo čistého textu (pro testovací data)
                        bool isValid = (user.PasswordHash == computedHash) || (user.PasswordHash == password);

                        if (isValid)
                        {
                            if (user.IsActive == "N")
                            {
                                lblError.Text = "Account is disabled.";
                                return;
                            }

                            App.CurrentUser = user;
                            MainWindow main = new MainWindow();
                            main.Show();
                            this.Close();
                            return;
                        }
                    }
                    lblError.Text = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Database Error: {ex.Message}";
            }
        }

        private static string ComputeSha256Hash(string rawData, string? salt)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                string dataToHash = rawData;
                if (!string.IsNullOrEmpty(salt)) dataToHash += salt;

                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
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