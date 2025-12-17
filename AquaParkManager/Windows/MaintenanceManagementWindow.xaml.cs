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


            if (!IsUserAdmin())
            {
                MessageBox.Show("Přístup k provozním logům (Historie) má pouze Administrátor.", "Přístup zamítnut");
                this.Close();
                return; 
            }

            _context = new AquaParkContext();
            LoadMaintenanceRecords();
            ClearForm();
            LoadAttractions();
            LoadStaff();
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
                var records = _context.MaintenanceRecords
                    .Include(m => m.Staff)
                    .OrderByDescending(m => m.ReportDate)
                    .ToList();

                dgMaintenance.ItemsSource = records;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LoadAttractions()
        {
            cmbAttraction.ItemsSource = _context.Attractions.ToList();
            cmbAttraction.DisplayMemberPath = "Name";       // Co se zobrazí
            cmbAttraction.SelectedValuePath = "AttractionId"; // Co je hodnota
        }
        private void LoadStaff()
        {
            cmbStaff.ItemsSource = _context.Staff.Where(s => s.Active == "Y").ToList();
            cmbStaff.DisplayMemberPath = "FullName";
            cmbStaff.SelectedValuePath = "StaffId";
        }
        private void DgMaintenance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMaintenance = dgMaintenance.SelectedItem as MaintenanceRecord;
            if (_selectedMaintenance != null)
            {
                cmbAttraction.SelectedValue = _selectedMaintenance.RelatedId;
                cmbStaff.SelectedValue = _selectedMaintenance.ReportedBy;
                dpReportDate.SelectedDate = _selectedMaintenance.ReportDate;
                txtProblemDescription.Text = _selectedMaintenance.ProblemDescription;
            }
        }
        private void BtnAddMaintenance_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnSave_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnDelete_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { /*...*/ }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { /*...*/ }
        private void ClearForm() { /*...*/ }
    }
}