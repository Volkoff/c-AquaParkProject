using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class StaffManagementWindow : Window
    {
        private AquaParkContext _context;
        private Staff? _selectedStaff;
        private List<RoleSelection> _roleSelections = new List<RoleSelection>();

        public StaffManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();

            LoadRoles();
            LoadStaff();
            ClearForm();
        }

        private void LoadRoles()
        {
            try
            {
                var roles = _context.Roles.ToList();
                _roleSelections = roles.Select(r => new RoleSelection
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    IsSelected = false
                }).ToList();

                lstRoles.ItemsSource = _roleSelections;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba pøi naèítání rolí: " + ex.Message);
            }
        }

        private void LoadStaff()
        {
            try
            {
                // Musíme naèíst i Adresu a její PSÈ (Include)
                var staff = _context.Staff
                    .Include(s => s.Address)
                    .ThenInclude(a => a.PostalCode)
                    .ToList();

                dgStaff.ItemsSource = staff;
                lblStatus.Text = $"Naèteno {staff.Count} zamìstnancù";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba pøi naèítání zamìstnancù: {ex.Message}\nInner: {ex.InnerException?.Message}");
            }
        }

        private void LoadStaffDetails(Staff staff)
        {
            txtFirstName.Text = staff.FirstName;
            txtLastName.Text = staff.LastName;
            txtEmail.Text = staff.Email ?? "";
            txtPhone.Text = staff.Phone ?? "";

            // --- Naètení Adresy ---
            if (staff.Address != null)
            {
                txtStreet.Text = staff.Address.Street ?? "";
                txtHouseNumber.Text = staff.Address.HouseNumber;

                if (staff.Address.PostalCode != null)
                {
                    txtZip.Text = staff.Address.PostalCode.Code;
                    txtCity.Text = staff.Address.PostalCode.City;
                    txtRegion.Text = staff.Address.PostalCode.Region;
                    txtCountry.Text = staff.Address.PostalCode.Country;
                }
            }
            else
            {
                // Vymazat pole adresy, pokud zamìstnanec adresu nemá (nemìlo by nastat díky NOT NULL)
                ClearAddressFields();
            }
            // ----------------------

            dpHireDate.SelectedDate = staff.HireDate;
            chkActive.IsChecked = staff.Active == "Y";
            txtNotes.Text = staff.Notes ?? "";

            // Nastavení rolí
            var assignedRoleIds = _context.StaffRoles
                                    .Where(sr => sr.StaffId == staff.StaffId)
                                    .Select(sr => sr.RoleId)
                                    .ToList();

            foreach (var roleItem in _roleSelections)
            {
                roleItem.IsSelected = assignedRoleIds.Contains(roleItem.RoleId);
            }
            lstRoles.Items.Refresh();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Validace základních údajù
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("Jméno a pøíjmení jsou povinné.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Validace adresy (DB vyžaduje HouseNumber, City, Zip, Region, Country)
                if (string.IsNullOrWhiteSpace(txtHouseNumber.Text) || string.IsNullOrWhiteSpace(txtCity.Text) ||
                    string.IsNullOrWhiteSpace(txtZip.Text))
                {
                    MessageBox.Show("Vyplòte prosím adresu (Èíslo popisné, Mìsto, PSÈ).", "Chyba adresy", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Zpracování ADRESY a PSÈ
                int addressId = GetOrCreateAddress(
                    txtStreet.Text.Trim(),
                    txtHouseNumber.Text.Trim(),
                    txtCity.Text.Trim(),
                    txtZip.Text.Trim(),
                    txtRegion.Text.Trim(),
                    txtCountry.Text.Trim()
                );

                // 4. Pøíprava rolí (JobTitle string)
                var selectedRoles = _roleSelections.Where(r => r.IsSelected).ToList();
                string jobTitleString = selectedRoles.Any() ? string.Join(", ", selectedRoles.Select(r => r.RoleName)) : "Zamìstnanec";

                int staffId = 0;

                if (_selectedStaff == null)
                {
                    // --- INSERT ---
                    var newStaff = new Staff
                    {
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        JobTitle = jobTitleString,
                        HireDate = dpHireDate.SelectedDate ?? DateTime.Now,
                        Active = chkActive.IsChecked == true ? "Y" : "N",
                        Notes = txtNotes.Text.Trim(),
                        AddressId = addressId // Zde použijeme získané ID
                    };

                    _context.Staff.Add(newStaff);
                    _context.SaveChanges();
                    staffId = newStaff.StaffId;
                    lblStatus.Text = "Zamìstnanec pøidán.";
                }
                else
                {
                    // --- UPDATE ---
                    _selectedStaff.FirstName = txtFirstName.Text.Trim();
                    _selectedStaff.LastName = txtLastName.Text.Trim();
                    _selectedStaff.Email = txtEmail.Text.Trim();
                    _selectedStaff.Phone = txtPhone.Text.Trim();
                    _selectedStaff.JobTitle = jobTitleString;
                    _selectedStaff.HireDate = dpHireDate.SelectedDate ?? _selectedStaff.HireDate;
                    _selectedStaff.Active = chkActive.IsChecked == true ? "Y" : "N";
                    _selectedStaff.Notes = txtNotes.Text.Trim();
                    _selectedStaff.AddressId = addressId; // Aktualizace adresy

                    _context.SaveChanges();
                    staffId = _selectedStaff.StaffId;
                    lblStatus.Text = "Zamìstnanec aktualizován.";
                }

                // 5. Aktualizace vazební tabulky rolí
                UpdateStaffRolesInDatabase(staffId, selectedRoles);

                LoadStaff();
                ClearForm();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "";
                MessageBox.Show($"Chyba pøi ukládání: {ex.Message}\n\nDetaily: {inner}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- POMOCNÁ METODA PRO ADRESY ---
        private int GetOrCreateAddress(string street, string houseNum, string city, string zip, string region, string country)
        {
            // A. Najdeme nebo vytvoøíme PSÈ
            var postalCode = _context.PostalCodes
                .FirstOrDefault(p => p.Code == zip && p.City == city);

            if (postalCode == null)
            {
                postalCode = new PostalCode
                {
                    Code = zip,
                    City = city,
                    Region = string.IsNullOrEmpty(region) ? "Nezadáno" : region,
                    Country = string.IsNullOrEmpty(country) ? "Nezadáno" : country
                };
                _context.PostalCodes.Add(postalCode);
                _context.SaveChanges(); // Musíme uložit, abychom mìli PostalCodeId
            }

            // B. Najdeme nebo vytvoøíme ADRESU
            // Zjednodušená kontrola: hledáme shodu v ulici, èísle a ID psè
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

        private void UpdateStaffRolesInDatabase(int staffId, List<RoleSelection> selectedRoles)
        {
            var currentRelations = _context.StaffRoles.Where(sr => sr.StaffId == staffId).ToList();
            var desiredRoleIds = selectedRoles.Select(r => r.RoleId).ToList();

            // Smazat nechtìné
            var toDelete = currentRelations.Where(sr => !desiredRoleIds.Contains(sr.RoleId)).ToList();
            if (toDelete.Any()) _context.StaffRoles.RemoveRange(toDelete);

            // Pøidat nové
            var existingRoleIds = currentRelations.Select(sr => sr.RoleId).ToList();
            var toAddIds = desiredRoleIds.Except(existingRoleIds).ToList();

            foreach (var roleId in toAddIds)
            {
                _context.StaffRoles.Add(new StaffRole { StaffId = staffId, RoleId = roleId });
            }
            _context.SaveChanges();
        }

        private void ClearForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";

            ClearAddressFields();

            foreach (var item in _roleSelections) item.IsSelected = false;
            lstRoles.Items.Refresh();

            dpHireDate.SelectedDate = DateTime.Now;
            chkActive.IsChecked = true;
            txtNotes.Text = "";
            _selectedStaff = null;
        }

        private void ClearAddressFields()
        {
            txtStreet.Text = "";
            txtHouseNumber.Text = "";
            txtCity.Text = "";
            txtZip.Text = "";
            txtRegion.Text = "Pardubický kraj";
            txtCountry.Text = "Èeská republika";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Pro zjednodušení vyhledávání (Search logic) znovu naèteme vše a filtrujeme v pamìti
            // nebo udìláme dotaz. Zde jen reload:
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText)) { LoadStaff(); return; }

            try
            {
                var staff = _context.Staff
                     .Include(s => s.Address).ThenInclude(a => a.PostalCode)
                     .Where(s => s.FirstName.ToLower().Contains(searchText) || s.LastName.ToLower().Contains(searchText))
                     .ToList();
                dgStaff.ItemsSource = staff;
            }
            catch { }
        }

        private void BtnAddStaff_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedStaff = null; txtFirstName.Focus(); }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }
        private void DgStaff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStaff = dgStaff.SelectedItem as Staff;
            if (_selectedStaff != null) LoadStaffDetails(_selectedStaff);
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStaff == null) return;
            if (MessageBox.Show("Opravdu smazat?", "Smazat", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Staff.Remove(_selectedStaff);
                    _context.SaveChanges();
                    LoadStaff();
                    ClearForm();
                }
                catch (Exception ex) { MessageBox.Show("Chyba pøi mazání: " + ex.Message); }
            }
        }
        protected override void OnClosed(EventArgs e) { _context?.Dispose(); base.OnClosed(e); }
    }

    public class RoleSelection
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}