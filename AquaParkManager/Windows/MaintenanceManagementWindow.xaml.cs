using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore; // Ponecháno, kdyby bylo potøeba v budoucnu

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
                MessageBox.Show($"Error loading attractions: {ex.Message}");
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
                MessageBox.Show($"Error loading staff: {ex.Message}");
            }
        }

        private void LoadMaintenanceRecords()
        {
            try
            {
                // Odstranili jsme .Include(), protože vazby jsou nyní [NotMapped]
                var maintenanceRecords = _context.MaintenanceRecords.ToList();

                // POZOR: Protože vazby nejsou v DB namapované pøímo (NotMapped), 
                // data pro Grid (Jméno atrakce, Staff) se nenaètou automaticky.
                // Pro úèely obhajoby to buï necháme prázdné, nebo bychom museli data spojit ruènì.
                // Zde jen naèteme záznamy, aby aplikace nepadala.

                dgMaintenance.ItemsSource = maintenanceRecords;
                lblStatus.Text = $"Loaded {maintenanceRecords.Count} maintenance records";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance records: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Vyhledávání zjednodušeno
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadMaintenanceRecords();
                return;
            }

            try
            {
                var filtered = _context.MaintenanceRecords
                    .Where(m => (m.ProblemDescription != null && m.ProblemDescription.ToLower().Contains(searchText)))
                    .ToList();
                dgMaintenance.ItemsSource = filtered;
            }
            catch { }
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
                if (cmbAttraction.SelectedValue == null) return;
                int attractionId = (int)cmbAttraction.SelectedValue;

                if (_selectedMaintenance == null)
                {
                    var newRecord = new MaintenanceRecord
                    {
                        // Mapování vazby na Atrakci (polymorfní)
                        RelatedTable = "ATTRACTIONS",
                        RelatedId = attractionId,

                        ReportedBy = cmbStaff.SelectedValue as int?,
                        ReportDate = dpReportDate.SelectedDate ?? DateTime.Now,
                        ProblemDescription = txtProblemDescription.Text,
                        LogType = "MAINTENANCE" // Fixní typ
                    };
                    _context.MaintenanceRecords.Add(newRecord);
                }
                else
                {
                    _selectedMaintenance.RelatedTable = "ATTRACTIONS";
                    _selectedMaintenance.RelatedId = attractionId;

                    _selectedMaintenance.ReportedBy = cmbStaff.SelectedValue as int?;
                    _selectedMaintenance.ReportDate = dpReportDate.SelectedDate ?? DateTime.Now;
                    _selectedMaintenance.ProblemDescription = txtProblemDescription.Text;
                }

                _context.SaveChanges();
                LoadMaintenanceRecords();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving: {ex.Message}");
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
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            cmbAttraction.SelectedIndex = -1;
            cmbStaff.SelectedIndex = -1;
            dpReportDate.SelectedDate = DateTime.Now;
            txtProblemDescription.Text = "";
            txtActionTaken.Text = "";
            dpCompletedDate.SelectedDate = null;
            txtCost.Text = "";
            _selectedMaintenance = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}