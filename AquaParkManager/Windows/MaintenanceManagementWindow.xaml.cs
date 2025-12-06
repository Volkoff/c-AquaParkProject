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
            catch { }
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
            catch { }
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

                string rawDesc = _selectedMaintenance.ProblemDescription ?? "";

                txtProblemDescription.Text = GetValueFromTag(rawDesc, "PROBLEM");
                txtActionTaken.Text = GetValueFromTag(rawDesc, "ACTION");
                txtCost.Text = GetValueFromTag(rawDesc, "COST");

                string dateStr = GetValueFromTag(rawDesc, "COMPLETED");
                if (DateTime.TryParse(dateStr, out DateTime dt)) dpCompletedDate.SelectedDate = dt;
                else dpCompletedDate.SelectedDate = null;
            }
        }

        private string GetValueFromTag(string text, string tag)
        {
            string startTag = $"[{tag}]:";
            int startIndex = text.IndexOf(startTag);
            if (startIndex == -1) return "";

            startIndex += startTag.Length;
            int endIndex = text.IndexOf("|", startIndex);
            if (endIndex == -1) endIndex = text.Length;

            return text.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private void BtnAddMaintenance_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedMaintenance = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbAttraction.SelectedValue == null) return;
                int attractionId = (int)cmbAttraction.SelectedValue;

                string completedStr = dpCompletedDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                string packedDesc = $"[PROBLEM]: {txtProblemDescription.Text} | [ACTION]: {txtActionTaken.Text} | [COST]: {txtCost.Text} | [COMPLETED]: {completedStr}";

                if (_selectedMaintenance == null)
                {
                    var newRecord = new MaintenanceRecord
                    {
                        RelatedTable = "ATTRACTIONS",
                        RelatedId = attractionId,
                        ReportedBy = cmbStaff.SelectedValue as int?,
                        ReportDate = dpReportDate.SelectedDate ?? DateTime.Now,
                        LogType = "MAINTENANCE",
                        ProblemDescription = packedDesc
                    };
                    _context.MaintenanceRecords.Add(newRecord);
                }
                else
                {
                    _selectedMaintenance.RelatedId = attractionId;
                    _selectedMaintenance.ReportedBy = cmbStaff.SelectedValue as int?;
                    _selectedMaintenance.ReportDate = dpReportDate.SelectedDate ?? DateTime.Now;
                    _selectedMaintenance.ProblemDescription = packedDesc;
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

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }
        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}