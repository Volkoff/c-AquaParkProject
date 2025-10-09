using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;

namespace AquaParkManager.Windows
{
    public partial class StaffManagementWindow : Window
    {
        private AquaParkContext _context;
        private Staff? _selectedStaff;

        public StaffManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadStaff();
            ClearForm();
        }

        private void LoadStaff()
        {
            try
            {
                var staff = _context.Staff.ToList();
                dgStaff.ItemsSource = staff;
                lblStatus.Text = $"Loaded {staff.Count} staff members";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading staff";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadStaff();
                return;
            }

            try
            {
                var filteredStaff = _context.Staff
                    .Where(s => s.FirstName.ToLower().Contains(searchText) ||
                               s.LastName.ToLower().Contains(searchText) ||
                               s.Email.ToLower().Contains(searchText) ||
                               s.JobTitle.ToLower().Contains(searchText))
                    .ToList();
                
                dgStaff.ItemsSource = filteredStaff;
                lblStatus.Text = $"Found {filteredStaff.Count} staff members";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching staff: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgStaff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStaff = dgStaff.SelectedItem as Staff;
            if (_selectedStaff != null)
            {
                LoadStaffDetails(_selectedStaff);
            }
        }

        private void LoadStaffDetails(Staff staff)
        {
            txtFirstName.Text = staff.FirstName;
            txtLastName.Text = staff.LastName;
            txtEmail.Text = staff.Email ?? "";
            txtPhone.Text = staff.Phone ?? "";
            txtJobTitle.Text = staff.JobTitle ?? "";
            dpHireDate.SelectedDate = staff.HireDate;
            chkActive.IsChecked = staff.Active == "Y";
            txtNotes.Text = staff.Notes ?? "";
        }

        private void BtnAddStaff_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedStaff = null;
            txtFirstName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("First Name and Last Name are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedStaff == null)
                {
                    // Add new staff
                    var newStaff = new Staff
                    {
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        JobTitle = txtJobTitle.Text.Trim(),
                        HireDate = dpHireDate.SelectedDate ?? DateTime.Now,
                        Active = chkActive.IsChecked == true ? "Y" : "N",
                        Notes = txtNotes.Text.Trim()
                    };

                    _context.Staff.Add(newStaff);
                    _context.SaveChanges();
                    lblStatus.Text = "Staff member added successfully";
                }
                else
                {
                    // Update existing staff
                    _selectedStaff.FirstName = txtFirstName.Text.Trim();
                    _selectedStaff.LastName = txtLastName.Text.Trim();
                    _selectedStaff.Email = txtEmail.Text.Trim();
                    _selectedStaff.Phone = txtPhone.Text.Trim();
                    _selectedStaff.JobTitle = txtJobTitle.Text.Trim();
                    _selectedStaff.HireDate = dpHireDate.SelectedDate ?? _selectedStaff.HireDate;
                    _selectedStaff.Active = chkActive.IsChecked == true ? "Y" : "N";
                    _selectedStaff.Notes = txtNotes.Text.Trim();

                    _context.SaveChanges();
                    lblStatus.Text = "Staff member updated successfully";
                }

                LoadStaff();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving staff: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving staff";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStaff == null)
            {
                MessageBox.Show("Please select a staff member to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {_selectedStaff.FirstName} {_selectedStaff.LastName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Staff.Remove(_selectedStaff);
                    _context.SaveChanges();
                    LoadStaff();
                    ClearForm();
                    lblStatus.Text = "Staff member deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting staff: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error deleting staff";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedStaff = null;
            dgStaff.SelectedItem = null;
        }

        private void ClearForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtJobTitle.Text = "";
            dpHireDate.SelectedDate = DateTime.Now;
            chkActive.IsChecked = true;
            txtNotes.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
