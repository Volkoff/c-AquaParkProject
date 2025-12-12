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
                App.ConnectionString = connString;
                SelectedConnectionString = connString;
                using var ctx = new AquaParkContext();
                ctx.Database.CanConnect();
                MessageBox.Show("Connection successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryBuildConnectionString(out string connString)
        {
            connString = string.Empty;

            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("Host is required.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPort.Text))
            {
                MessageBox.Show("Port is required.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtSid.Text))
            {
                MessageBox.Show("SID is required.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("User Id is required.");
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
