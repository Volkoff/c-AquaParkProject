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
                // Na�teme atrakce pro v�b�r
                var attractions = _context.Attractions.ToList();
                cmbAttraction.ItemsSource = attractions;
                cmbAttraction.DisplayMemberPath = "Name";
                cmbAttraction.SelectedValuePath = "AttractionId";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error loading attractions: {ex.Message}";
            }
        }

        private void LoadStaff()
        {
            try
            {
                var staff = _context.Staff.ToList();
                cmbStaff.ItemsSource = staff;
                cmbStaff.DisplayMemberPath = "FullName";
                cmbStaff.SelectedValuePath = "StaffId";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error loading staff: {ex.Message}";
            }
        }

        private void LoadMaintenanceRecords()
        {
            try
            {
                var records = _context.MaintenanceRecords
                    .Include(m => m.Staff)
                    .Where(m => m.LogType == "MAINTENANCE")
                    .ToList();

                dgMaintenance.ItemsSource = records;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                lblStatus.Text = $"Error loading maintenance records: {ex.Message}";
            }
        }

        private void DgMaintenance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMaintenance = dgMaintenance.SelectedItem as MaintenanceRecord;
            if (_selectedMaintenance != null)
            {
                cmbStaff.SelectedValue = _selectedMaintenance.ReportedBy;
                dpReportDate.SelectedDate = _selectedMaintenance.ReportDate;

                if (_selectedMaintenance.RelatedTable == "ATTRACTIONS" && _selectedMaintenance.RelatedId.HasValue)
                {
                    cmbAttraction.SelectedValue = _selectedMaintenance.RelatedId.Value;
                }

                txtProblemDescription.Text = _selectedMaintenance.ProblemDescription ?? "";
                lblStatus.Text = $"Selected maintenance record: {_selectedMaintenance.MaintenanceId}";
            }
        }

        private void BtnAddMaintenance_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedMaintenance = null;
            lblStatus.Text = "Ready to add maintenance record";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? attractionId = cmbAttraction.SelectedValue as int?;

                if (_selectedMaintenance == null)
                {
                    var newRecord = new MaintenanceRecord
                    {
                        RelatedTable = "ATTRACTIONS",
                        RelatedId = attractionId,
                        ReportedBy = cmbStaff.SelectedValue as int?,
                        ReportDate = dpReportDate.SelectedDate ?? DateTime.Now,
                        LogType = "MAINTENANCE",
                        ProblemDescription = txtProblemDescription.Text
                    };
                    _context.MaintenanceRecords.Add(newRecord);
                }
                else
                {
                    _selectedMaintenance.RelatedId = attractionId;
                    _selectedMaintenance.ReportedBy = cmbStaff.SelectedValue as int?;
                    _selectedMaintenance.ReportDate = dpReportDate.SelectedDate ?? DateTime.Now;
                    _selectedMaintenance.ProblemDescription = txtProblemDescription.Text;
                }

                _context.SaveChanges();
                LoadMaintenanceRecords();
                ClearForm();
                lblStatus.Text = "Maintenance saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving: {ex.Message}");
                lblStatus.Text = $"Error saving maintenance: {ex.Message}";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMaintenance != null)
            {
                _context.MaintenanceRecords.Remove(_selectedMaintenance);
                _context.SaveChanges();
                LoadMaintenanceRecords();
                ClearForm();
                lblStatus.Text = "Maintenance record deleted successfully";
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            cmbAttraction.SelectedIndex = -1;
            cmbStaff.SelectedIndex = -1;
            dpReportDate.SelectedDate = DateTime.Now;
            txtProblemDescription.Text = "";
            _selectedMaintenance = null;
            lblStatus.Text = "Form cleared";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }
        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}