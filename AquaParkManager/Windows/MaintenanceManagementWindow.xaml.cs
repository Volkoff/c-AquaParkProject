using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class MaintenanceManagementWindow : Window
    {
        private AquaParkContext _context;
        private MaintenanceRecord? _selectedMaintenance;

        public MaintenanceManagementWindow()
        {
            InitializeComponent();

            // Bod 21: Kontrola práv - Historii/Logy vidí jen Admin
            if (!IsUserAdmin())
            {
                MessageBox.Show("Přístup k provozním logům (Historie) má pouze Administrátor.", "Přístup zamítnut");
                this.Close();
                return; // Ukončíme konstruktor, okno se zavře
            }

            _context = new AquaParkContext();
            LoadMaintenanceRecords();
            // ... (ostatní load metody: LoadStaff, LoadAttractions) ...
            ClearForm();
        }

        private bool IsUserAdmin()
        {
            if (App.CurrentUser == null) return false;
            
            using (var ctx = new AquaParkContext())
            {
                var roles = ctx.UserRoles.Include(ur => ur.Role)
                               .Where(ur => ur.UserId == App.CurrentUser.UserId)
                               .Select(ur => ur.Role.RoleName).ToList();
                
                bool isAdmin = roles.Contains("ADMIN") || roles.Contains("MANAGER");
                bool isStaff = isAdmin || roles.Contains("STAFF");
                
                // If no roles found, check if user is linked to a Staff record
                if (roles.Count == 0 && App.CurrentUser.StaffId.HasValue)
                {
                    isStaff = true;
                    isAdmin = true;
                }
                
                return isStaff;
            }
        }

        private void LoadMaintenanceRecords()
        {
            try
            {
                // OPERATIONAL_LOGS
                var records = _context.MaintenanceRecords
                    .Include(m => m.Staff)
                    .OrderByDescending(m => m.ReportDate)
                    .ToList();

                dgMaintenance.ItemsSource = records;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ... (Zbytek metod BtnSave, BtnDelete atd. zůstává stejný) ...
        // Jen pro úplnost, zde jsou prázdné placeholdery, zkopírujte si obsah z původního souboru:
        private void LoadAttractions() { /*...*/ }
        private void LoadStaff() { /*...*/ }
        private void DgMaintenance_SelectionChanged(object sender, SelectionChangedEventArgs e) { /*...*/ }
        private void BtnAddMaintenance_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnSave_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnDelete_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { /*...*/ }
        private void ClearForm() { /*...*/ }
    }
}