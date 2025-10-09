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
            _context = new AquaParkContext();
            LoadAttractions();
            LoadStaff();
            LoadMaintenanceRecords();
            ClearForm();
        }

        private void LoadAttractions()
        {
            try
            {
                var attractions = _context.Attractions.ToList();
                cmbAttraction.ItemsSource = attractions;
                cmbAttraction.DisplayMemberPath = "Name";
                cmbAttraction.SelectedValuePath = "AttractionId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attractions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStaff()
        {
            try
            {
                var staff = _context.Staff.ToList();
                cmbStaff.ItemsSource = staff;
                cmbStaff.DisplayMemberPath = "FirstName";
                cmbStaff.SelectedValuePath = "StaffId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMaintenanceRecords()
        {
            try
            {
                var maintenanceRecords = _context.MaintenanceRecords
                    .Include(m => m.Attraction)
                    .Include(m => m.Staff)
                    .ToList();
                dgMaintenance.ItemsSource = maintenanceRecords;
                lblStatus.Text = $"Loaded {maintenanceRecords.Count} maintenance records";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading maintenance records";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadMaintenanceRecords();
                return;
            }

            try
            {
                var filteredRecords = _context.MaintenanceRecords
                    .Include(m => m.Attraction)
                    .Include(m => m.Staff)
                    .Where(m => m.Attraction.Name.ToLower().Contains(searchText) ||
                               (m.Staff != null && m.Staff.FirstName.ToLower().Contains(searchText)) ||
                               (m.ProblemDescription != null && m.ProblemDescription.ToLower().Contains(searchText)))
                    .ToList();
                
                dgMaintenance.ItemsSource = filteredRecords;
                lblStatus.Text = $"Found {filteredRecords.Count} maintenance records";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching maintenance records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgMaintenance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMaintenance = dgMaintenance.SelectedItem as MaintenanceRecord;
            if (_selectedMaintenance != null)
            {
                LoadMaintenanceDetails(_selectedMaintenance);
            }
        }

        private void LoadMaintenanceDetails(MaintenanceRecord maintenance)
        {
            cmbAttraction.SelectedValue = maintenance.AttractionId;
            cmbStaff.SelectedValue = maintenance.ReportedBy;
            dpReportDate.SelectedDate = maintenance.ReportDate;
            txtProblemDescription.Text = maintenance.ProblemDescription ?? "";
            txtActionTaken.Text = maintenance.ActionTaken ?? "";
            dpCompletedDate.SelectedDate = maintenance.CompletedDate;
            txtCost.Text = maintenance.Cost.ToString();
        }

        private void BtnAddMaintenance_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedMaintenance = null;
            cmbAttraction.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbAttraction.SelectedValue == null)
                {
                    MessageBox.Show("Please select an attraction.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtCost.Text, out decimal cost) || cost < 0)
                {
                    MessageBox.Show("Please enter a valid cost (>= 0).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedMaintenance == null)
                {
                    // Add new maintenance record
                    var newMaintenance = new MaintenanceRecord
                    {
                        AttractionId = (int)cmbAttraction.SelectedValue,
                        ReportedBy = cmbStaff.SelectedValue as int?,
                        ReportDate = dpReportDate.SelectedDate ?? DateTime.Now,
                        ProblemDescription = txtProblemDescription.Text.Trim(),
                        ActionTaken = txtActionTaken.Text.Trim(),
                        CompletedDate = dpCompletedDate.SelectedDate,
                        Cost = cost
                    };

                    _context.MaintenanceRecords.Add(newMaintenance);
                    _context.SaveChanges();
                    lblStatus.Text = "Maintenance record added successfully";
                }
                else
                {
                    // Update existing maintenance record
                    _selectedMaintenance.AttractionId = (int)cmbAttraction.SelectedValue;
                    _selectedMaintenance.ReportedBy = cmbStaff.SelectedValue as int?;
                    _selectedMaintenance.ReportDate = dpReportDate.SelectedDate ?? _selectedMaintenance.ReportDate;
                    _selectedMaintenance.ProblemDescription = txtProblemDescription.Text.Trim();
                    _selectedMaintenance.ActionTaken = txtActionTaken.Text.Trim();
                    _selectedMaintenance.CompletedDate = dpCompletedDate.SelectedDate;
                    _selectedMaintenance.Cost = cost;

                    _context.SaveChanges();
                    lblStatus.Text = "Maintenance record updated successfully";
                }

                LoadMaintenanceRecords();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving maintenance record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving maintenance record";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMaintenance == null)
            {
                MessageBox.Show("Please select a maintenance record to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete this maintenance record?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.MaintenanceRecords.Remove(_selectedMaintenance);
                    _context.SaveChanges();
                    LoadMaintenanceRecords();
                    ClearForm();
                    lblStatus.Text = "Maintenance record deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting maintenance record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error deleting maintenance record";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedMaintenance = null;
            dgMaintenance.SelectedItem = null;
        }

        private void ClearForm()
        {
            cmbAttraction.SelectedIndex = -1;
            cmbStaff.SelectedIndex = -1;
            dpReportDate.SelectedDate = DateTime.Now;
            txtProblemDescription.Text = "";
            txtActionTaken.Text = "";
            dpCompletedDate.SelectedDate = null;
            txtCost.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
