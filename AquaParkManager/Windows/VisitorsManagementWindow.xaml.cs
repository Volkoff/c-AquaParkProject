using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

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
                var visitors = _context.Visitors
                    .Include(v => v.Address)
                    .ThenInclude(a => a.PostalCode)
                    .ToList();
                dgVisitors.ItemsSource = visitors;
                lblStatus.Text = $"Loaded {visitors.Count} visitors";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading visitors: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText)) { LoadVisitors(); return; }
            try
            {
                var filteredVisitors = _context.Visitors
                    .Include(v => v.Address).ThenInclude(a => a.PostalCode)
                    .Where(v => (v.FirstName != null && v.FirstName.ToLower().Contains(searchText)) ||
                               (v.LastName != null && v.LastName.ToLower().Contains(searchText)))
                    .ToList();
                dgVisitors.ItemsSource = filteredVisitors;
                lblStatus.Text = $"Search results: {filteredVisitors.Count} visitors";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error filtering visitors: {ex.Message}";
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
            txtNotes.Text = visitor.Notes ?? "";

            if (visitor.Address != null)
            {
                txtStreet.Text = visitor.Address.Street ?? "";
                txtHouseNumber.Text = visitor.Address.HouseNumber;

                if (visitor.Address.PostalCode != null)
                {
                    txtZip.Text = visitor.Address.PostalCode.Code;
                    txtCity.Text = visitor.Address.PostalCode.City;
                    txtRegion.Text = visitor.Address.PostalCode.Region;
                    txtCountry.Text = visitor.Address.PostalCode.Country;
                }
            }
            else
            {
                ClearAddressFields();
            }

            var membership = _context.Memberships
        .FirstOrDefault(m => m.VisitorId == visitor.VisitorId && m.Status == "ACTIVE" && (m.EndDate == null || m.EndDate > DateTime.Now));

            txtMembershipStatus.Text = membership != null
                ? $"{membership.MembershipType} (Platí do: {membership.EndDate:dd.MM.yyyy})"
                : "Žádné aktivní členství";
        }

        private void BtnAddVisitor_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedVisitor = null;
            txtFirstName.Focus();
            lblStatus.Text = "Ready to add new visitor";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("First Name and Last Name are required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtHouseNumber.Text) || string.IsNullOrWhiteSpace(txtCity.Text) ||
                    string.IsNullOrWhiteSpace(txtZip.Text))
                {
                    MessageBox.Show("Address (House No., City, Zip) is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int addressId = GetOrCreateAddress(
                    txtStreet.Text.Trim(),
                    txtHouseNumber.Text.Trim(),
                    txtCity.Text.Trim(),
                    txtZip.Text.Trim(),
                    txtRegion.Text.Trim(),
                    txtCountry.Text.Trim()
                );

                if (_selectedVisitor == null)
                {
                    var newVisitor = new Visitor
                    {
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        DateOfBirth = dpDateOfBirth.SelectedDate,
                        Email = txtEmail.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        Notes = txtNotes.Text.Trim(),
                        AddressId = addressId
                    };
                    _context.Visitors.Add(newVisitor);
                }
                else
                {
                    _selectedVisitor.FirstName = txtFirstName.Text.Trim();
                    _selectedVisitor.LastName = txtLastName.Text.Trim();
                    _selectedVisitor.DateOfBirth = dpDateOfBirth.SelectedDate;
                    _selectedVisitor.Email = txtEmail.Text.Trim();
                    _selectedVisitor.Phone = txtPhone.Text.Trim();
                    _selectedVisitor.Notes = txtNotes.Text.Trim();
                    _selectedVisitor.AddressId = addressId;
                }

                _context.SaveChanges();
                LoadVisitors();
                ClearForm();
                lblStatus.Text = "Visitor saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving visitor: {ex.Message}");
                lblStatus.Text = $"Error saving visitor: {ex.Message}";
            }
        }

        private int GetOrCreateAddress(string street, string houseNum, string city, string zip, string region, string country)
        {
            if (string.IsNullOrEmpty(region)) region = "Nezad�no";
            if (string.IsNullOrEmpty(country)) country = "Nezad�no";

            var postalCode = _context.PostalCodes
                .FirstOrDefault(p => p.Code == zip && p.City == city);

            if (postalCode == null)
            {
                postalCode = new PostalCode
                {
                    Code = zip,
                    City = city,
                    Region = region,
                    Country = country
                };
                _context.PostalCodes.Add(postalCode);
                _context.SaveChanges();
            }

            var address = _context.Address
                .FirstOrDefault(a => a.Street == street && a.HouseNumber == houseNum && a.PostalCodeId == postalCode.PostalCodeId);

            if (address == null)
            {
                address = new Address
                {
                    Street = street,
                    HouseNumber = houseNum,
                    PostalCodeId = postalCode.PostalCodeId
                };
                _context.Address.Add(address);
                _context.SaveChanges();
            }

            return address.AddressId;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVisitor == null) return;
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
                MessageBox.Show($"Error: {ex.Message}");
                lblStatus.Text = $"Error deleting visitor: {ex.Message}";
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedVisitor = null; }

        private void ClearForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            dpDateOfBirth.SelectedDate = null;
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtNotes.Text = "";
            ClearAddressFields();
            _selectedVisitor = null;
            lblStatus.Text = "Form cleared";
        }

        private void ClearAddressFields()
        {
            txtStreet.Text = "";
            txtHouseNumber.Text = "";
            txtCity.Text = "";
            txtZip.Text = "";
            txtRegion.Text = "Pardubick� kraj";
            txtCountry.Text = "�esk� republika";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}