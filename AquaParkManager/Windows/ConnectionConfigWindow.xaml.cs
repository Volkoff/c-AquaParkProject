using System;
using System.Windows;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class ConnectionConfigWindow : Window
    {
        public string? SelectedConnectionString { get; private set; }

        public ConnectionConfigWindow()
        {
            InitializeComponent();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildConnectionString(out var connString))
                return;

            App.ConnectionString = connString;
            SelectedConnectionString = connString;

            // Dùležité: Toto øekne App.xaml.cs, že se pøipojení povedlo
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildConnectionString(out var connString))
                return;

            try
            {
                // Doèasnì nastavíme pro test
                var originalConn = App.ConnectionString;
                App.ConnectionString = connString;

                using var ctx = new AquaParkContext();
                if (ctx.Database.CanConnect())
                {
                    MessageBox.Show("Connection successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Connection failed (CanConnect returned false).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryBuildConnectionString(out string connString)
        {
            connString = string.Empty;

            if (string.IsNullOrWhiteSpace(txtHost.Text) || string.IsNullOrWhiteSpace(txtPort.Text) ||
                string.IsNullOrWhiteSpace(txtSid.Text) || string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Please fill in Host, Port, SID and User.");
                return false;
            }

            var host = txtHost.Text.Trim();
            var port = txtPort.Text.Trim();
            var sid = txtSid.Text.Trim();
            var user = txtUser.Text.Trim();
            var password = txtPassword.Password;

            connString = $"User Id={user};Password={password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SID={sid})))";
            return true;
        }
    }
}