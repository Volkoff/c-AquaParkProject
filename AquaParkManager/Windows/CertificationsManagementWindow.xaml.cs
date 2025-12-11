using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class CertificationsManagementWindow : Window
    {
        private AquaParkContext _context;
        private Certification? _selectedCertification;
        // Helper to avoid direct dependence on generated lblStatus field (sometimes XAML partial class not present at compile-time)
        private void SetStatus(string text)
        {
            try
            {
                var tb = this.FindName("lblStatus") as System.Windows.Controls.TextBlock;
                if (tb != null)
                    tb.Text = text;
            }
            catch { /* ignore if not found */ }
        }

        public CertificationsManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadStaff();
            LoadCertifications();
            ClearForm();
        }

        private void LoadStaff()
        {
            try
            {
                var staff = _context.Staff.Where(s => s.Active == "Y").ToList();
                cmbStaff.ItemsSource = staff;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading staff: " + ex.Message);
                SetStatus($"Error loading staff: {ex.Message}");
            }
        }

        private void LoadCertifications()
        {
            try
            {
                var entities = _context.Certifications
                    .Include(c => c.Staff)
                    .ToList();

                var certifications = entities.Select(c => new CertificationViewModel
                {
                    CertId = c.CertId,
                    StaffId = c.StaffId,
                    CertName = c.CertName,
                    IssuedBy = c.IssuedBy,
                    IssuedDate = c.IssuedDate,
                    ExpiryDate = c.ExpiryDate,
                    VerificationStatus = c.VerificationStatus,
                    Notes = c.Notes,
                    Staff = c.Staff,
                    IsExpired = c.ExpiryDate.HasValue && c.ExpiryDate.Value < DateTime.Now,
                    IsExpiringSoon = c.ExpiryDate.HasValue && c.ExpiryDate.Value < DateTime.Now.AddDays(30) && c.ExpiryDate.Value >= DateTime.Now
                }).ToList();

                dgCertifications.ItemsSource = certifications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading certifications: {ex.Message}");
                SetStatus($"Error loading certifications: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCertifications();
        }

        private void ChkExpiring_Checked(object sender, RoutedEventArgs e) => FilterCertifications();
        private void ChkExpiring_Unchecked(object sender, RoutedEventArgs e) => FilterCertifications();
        private void ChkExpired_Checked(object sender, RoutedEventArgs e) => FilterCertifications();
        private void ChkExpired_Unchecked(object sender, RoutedEventArgs e) => FilterCertifications();

        private void FilterCertifications()
        {
            var searchText = txtSearch.Text.ToLower();
            var showExpiring = chkExpiring.IsChecked == true;
            var showExpired = chkExpired.IsChecked == true;

            try
            {
                var entities = _context.Certifications
                    .Include(c => c.Staff)
                    .Where(c => string.IsNullOrEmpty(searchText) ||
                               (c.CertName != null && c.CertName.ToLower().Contains(searchText)) ||
                               (c.Staff != null && (c.Staff.FirstName + " " + c.Staff.LastName).ToLower().Contains(searchText)))
                    .ToList();

                var certifications = entities.Select(c => new CertificationViewModel
                {
                    CertId = c.CertId,
                    StaffId = c.StaffId,
                    CertName = c.CertName,
                    IssuedBy = c.IssuedBy,
                    IssuedDate = c.IssuedDate,
                    ExpiryDate = c.ExpiryDate,
                    VerificationStatus = c.VerificationStatus,
                    Notes = c.Notes,
                    Staff = c.Staff,
                    IsExpired = c.ExpiryDate.HasValue && c.ExpiryDate.Value < DateTime.Now,
                    IsExpiringSoon = c.ExpiryDate.HasValue && c.ExpiryDate.Value < DateTime.Now.AddDays(30) && c.ExpiryDate.Value >= DateTime.Now
                }).ToList();

                if (showExpiring)
                    certifications = certifications.Where(c => c.IsExpiringSoon).ToList();
                if (showExpired)
                    certifications = certifications.Where(c => c.IsExpired).ToList();

                dgCertifications.ItemsSource = certifications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering certifications: {ex.Message}");
                SetStatus($"Error filtering certifications: {ex.Message}");
            }
        }

        private void DgCertifications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCertifications.SelectedItem is CertificationViewModel vm)
            {
                _selectedCertification = _context.Certifications.Find(vm.CertId);
                if (_selectedCertification != null)
                {
                    cmbStaff.SelectedValue = _selectedCertification.StaffId;
                    txtCertName.Text = _selectedCertification.CertName;
                    txtIssuedBy.Text = _selectedCertification.IssuedBy;
                    dpIssuedDate.SelectedDate = _selectedCertification.IssuedDate;
                    dpExpiryDate.SelectedDate = _selectedCertification.ExpiryDate;
                    cmbStatus.Text = _selectedCertification.VerificationStatus;
                    txtNotes.Text = _selectedCertification.Notes;

                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    SetStatus($"Selected certification: {txtCertName.Text}");
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var certification = new Certification
                {
                    StaffId = (int)cmbStaff.SelectedValue,
                    CertName = txtCertName.Text,
                    IssuedBy = txtIssuedBy.Text,
                    IssuedDate = dpIssuedDate.SelectedDate,
                    ExpiryDate = dpExpiryDate.SelectedDate,
                    VerificationStatus = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "PENDING",
                    Notes = txtNotes.Text
                };

                _context.Certifications.Add(certification);
                _context.SaveChanges();

                MessageBox.Show("Certification added successfully!");
                SetStatus("Certification added successfully!");
                LoadCertifications();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding certification: {ex.Message}");
                SetStatus($"Error adding certification: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCertification == null || !ValidateForm()) return;

            try
            {
                _selectedCertification.StaffId = (int)cmbStaff.SelectedValue;
                _selectedCertification.CertName = txtCertName.Text;
                _selectedCertification.IssuedBy = txtIssuedBy.Text;
                _selectedCertification.IssuedDate = dpIssuedDate.SelectedDate;
                _selectedCertification.ExpiryDate = dpExpiryDate.SelectedDate;
                _selectedCertification.VerificationStatus = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "PENDING";
                _selectedCertification.Notes = txtNotes.Text;

                _context.SaveChanges();

                MessageBox.Show("Certification updated successfully!");
                SetStatus("Certification updated successfully!");
                LoadCertifications();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating certification: {ex.Message}");
                SetStatus($"Error updating certification: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCertification == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this certification?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Certifications.Remove(_selectedCertification);
                    _context.SaveChanges();

                    MessageBox.Show("Certification deleted successfully!");
                    SetStatus("Certification deleted successfully!");
                    LoadCertifications();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting certification: {ex.Message}");
                    SetStatus($"Error deleting certification: {ex.Message}");
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            SetStatus("Form cleared");
        }

        private void ClearForm()
        {
            _selectedCertification = null;
            cmbStaff.SelectedIndex = -1;
            txtCertName.Clear();
            txtIssuedBy.Clear();
            dpIssuedDate.SelectedDate = null;
            dpExpiryDate.SelectedDate = null;
            cmbStatus.SelectedIndex = 0;
            txtNotes.Clear();
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        private bool ValidateForm()
        {
            if (cmbStaff.SelectedValue == null)
            {
                MessageBox.Show("Please select a staff member.");
                SetStatus("Please select a staff member.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCertName.Text))
            {
                MessageBox.Show("Please enter certification name.");
                SetStatus("Please enter certification name.");
                return false;
            }
            return true;
        }
    }

    public class CertificationViewModel
    {
        public int CertId { get; set; }
        public int StaffId { get; set; }
        public string CertName { get; set; } = string.Empty;
        public string? IssuedBy { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public Staff? Staff { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }

        public string AlertText => IsExpired ? "⚠ EXPIRED" : IsExpiringSoon ? "⚠ EXPIRING" : "";
        public Brush AlertColor => IsExpired ? Brushes.Red : IsExpiringSoon ? Brushes.Orange : Brushes.Green;
    }
}
