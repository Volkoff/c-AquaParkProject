using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;

namespace AquaParkManager.Windows
{
    public partial class VisitorsManagementWindow : Window
    {
        private AquaParkContext _context;
        private Visitor? _selectedVisitor;

        public VisitorsManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadVisitors();
            ClearForm();
        }

        private void LoadVisitors()
        {
            try
            {
                var visitors = _context.Visitors.ToList();
                dgVisitors.ItemsSource = visitors;
                lblStatus.Text = $"Loaded {visitors.Count} visitors";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading visitors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading visitors";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadVisitors();
                return;
            }

            try
            {
                var filteredVisitors = _context.Visitors
                    .Where(v => (v.FirstName != null && v.FirstName.ToLower().Contains(searchText)) ||
                               (v.LastName != null && v.LastName.ToLower().Contains(searchText)) ||
                               (v.Email != null && v.Email.ToLower().Contains(searchText)) ||
                               (v.Phone != null && v.Phone.ToLower().Contains(searchText)))
                    .ToList();
                
                dgVisitors.ItemsSource = filteredVisitors;
                lblStatus.Text = $"Found {filteredVisitors.Count} visitors";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching visitors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgVisitors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedVisitor = dgVisitors.SelectedItem as Visitor;
            if (_selectedVisitor != null)
            {
                LoadVisitorDetails(_selectedVisitor);
            }
        }

        private void LoadVisitorDetails(Visitor visitor)
        {
            txtFirstName.Text = visitor.FirstName ?? "";
            txtLastName.Text = visitor.LastName ?? "";
            dpDateOfBirth.SelectedDate = visitor.DateOfBirth;
            txtEmail.Text = visitor.Email ?? "";
            txtPhone.Text = visitor.Phone ?? "";
            txtEmergencyContact.Text = visitor.EmergencyContact ?? "";
            txtNotes.Text = visitor.Notes ?? "";
        }

        private void BtnAddVisitor_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedVisitor = null;
            txtFirstName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedVisitor == null)
                {
                    // Add new visitor
                    var newVisitor = new Visitor
                    {
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        DateOfBirth = dpDateOfBirth.SelectedDate,
                        Email = txtEmail.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        EmergencyContact = txtEmergencyContact.Text.Trim(),
                        Notes = txtNotes.Text.Trim()
                    };

                    _context.Visitors.Add(newVisitor);
                    _context.SaveChanges();
                    lblStatus.Text = "Visitor added successfully";
                }
                else
                {
                    // Update existing visitor
                    _selectedVisitor.FirstName = txtFirstName.Text.Trim();
                    _selectedVisitor.LastName = txtLastName.Text.Trim();
                    _selectedVisitor.DateOfBirth = dpDateOfBirth.SelectedDate;
                    _selectedVisitor.Email = txtEmail.Text.Trim();
                    _selectedVisitor.Phone = txtPhone.Text.Trim();
                    _selectedVisitor.EmergencyContact = txtEmergencyContact.Text.Trim();
                    _selectedVisitor.Notes = txtNotes.Text.Trim();

                    _context.SaveChanges();
                    lblStatus.Text = "Visitor updated successfully";
                }

                LoadVisitors();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving visitor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving visitor";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVisitor == null)
            {
                MessageBox.Show("Please select a visitor to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var visitorName = $"{_selectedVisitor.FirstName} {_selectedVisitor.LastName}".Trim();
            if (string.IsNullOrEmpty(visitorName))
                visitorName = "this visitor";

            var result = MessageBox.Show(
                $"Are you sure you want to delete {visitorName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Visitors.Remove(_selectedVisitor);
                    _context.SaveChanges();
                    LoadVisitors();
                    ClearForm();
                    lblStatus.Text = "Visitor deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting visitor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error deleting visitor";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedVisitor = null;
            dgVisitors.SelectedItem = null;
        }

        private void ClearForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            dpDateOfBirth.SelectedDate = null;
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtEmergencyContact.Text = "";
            txtNotes.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
