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

        // OPRAVA 1: Inicializace seznamu, aby nebyl null (øeší varování "Promìnná pole _roleSelections...")
        private List<RoleSelection> _roleSelections = new List<RoleSelection>();

        public StaffManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();

            LoadRoles(); // Nejdøív naèteme definice rolí
            LoadStaff();
            ClearForm();
        }

        private void LoadRoles()
        {
            try
            {
                // Naèteme všechny role z DB a pøevedeme je na naši pomocnou tøídu
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
                var staff = _context.Staff.ToList();
                dgStaff.ItemsSource = staff;
                lblStatus.Text = $"Loaded {staff.Count} staff members";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff: {ex.Message}");
            }
        }

        private void LoadStaffDetails(Staff staff)
        {
            txtFirstName.Text = staff.FirstName;
            txtLastName.Text = staff.LastName;
            txtEmail.Text = staff.Email ?? "";
            txtPhone.Text = staff.Phone ?? "";

            dpHireDate.SelectedDate = staff.HireDate;
            chkActive.IsChecked = staff.Active == "Y";
            txtNotes.Text = staff.Notes ?? "";

            // 1. Zjistíme ID rolí, které tento zamìstnanec má v tabulce STAFF_ROLES
            var assignedRoleIds = _context.StaffRoles
                                    .Where(sr => sr.StaffId == staff.StaffId)
                                    .Select(sr => sr.RoleId)
                                    .ToList();

            // 2. Projdeme náš seznam pro ListBox a zaškrtneme ty správné
            foreach (var roleItem in _roleSelections)
            {
                roleItem.IsSelected = assignedRoleIds.Contains(roleItem.RoleId);
            }

            // 3. Obnovíme zobrazení ListBoxu
            lstRoles.Items.Refresh();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validace jména
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("Jméno a pøíjmení jsou povinné.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Získáme všechny role, které uživatel zaškrtnul
                var selectedRoles = _roleSelections.Where(r => r.IsSelected).ToList();
                if (!selectedRoles.Any())
                {
                    MessageBox.Show("Zamìstnanec musí mít alespoò jednu roli.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Vytvoøíme textový øetìzec rolí (napø. "Plavèík, Manažer") pro sloupeèek JobTitle
                string jobTitleString = string.Join(", ", selectedRoles.Select(r => r.RoleName));

                // Oprava chybìjící adresy (aby nepadala aplikace na Foreign Key)
                var defaultAddress = _context.Address.FirstOrDefault();
                int defaultAddressId = defaultAddress?.AddressId ?? 1;

                int staffId = 0;

                if (_selectedStaff == null)
                {
                    // --- NOVÝ ZAMÌSTNANEC ---
                    var newStaff = new Staff
                    {
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        JobTitle = jobTitleString, // Uložíme textový seznam rolí
                        HireDate = dpHireDate.SelectedDate ?? DateTime.Now,
                        Active = chkActive.IsChecked == true ? "Y" : "N",
                        Notes = txtNotes.Text.Trim(),
                        AddressId = defaultAddressId // Povinné pole
                    };

                    _context.Staff.Add(newStaff);
                    _context.SaveChanges(); // Uložíme hned, abychom získali StaffId
                    staffId = newStaff.StaffId;
                    lblStatus.Text = "Zamìstnanec pøidán.";
                }
                else
                {
                    // --- UPDATE ZAMÌSTNANCE ---
                    _selectedStaff.FirstName = txtFirstName.Text.Trim();
                    _selectedStaff.LastName = txtLastName.Text.Trim();
                    _selectedStaff.Email = txtEmail.Text.Trim();
                    _selectedStaff.Phone = txtPhone.Text.Trim();
                    _selectedStaff.JobTitle = jobTitleString; // Aktualizujeme text
                    _selectedStaff.HireDate = dpHireDate.SelectedDate ?? _selectedStaff.HireDate;
                    _selectedStaff.Active = chkActive.IsChecked == true ? "Y" : "N";
                    _selectedStaff.Notes = txtNotes.Text.Trim();

                    _context.SaveChanges();
                    staffId = _selectedStaff.StaffId;
                    lblStatus.Text = "Zamìstnanec aktualizován.";
                }

                // --- KLÍÈOVÁ ÈÁST: Aktualizace tabulky STAFF_ROLES ---
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

        // Metoda, která inteligentnì srovná tabulku STAFF_ROLES
        private void UpdateStaffRolesInDatabase(int staffId, List<RoleSelection> selectedRoles)
        {
            // 1. Naèteme aktuální vazby z databáze
            var currentRelations = _context.StaffRoles.Where(sr => sr.StaffId == staffId).ToList();

            // 2. Získáme seznam ID, která chceme mít
            // OPRAVA 2: Tím, že je tøída RoleSelection definována, kompilátor už ví, co je 'RoleId', a typy budou sedìt.
            var desiredRoleIds = selectedRoles.Select(r => r.RoleId).ToList();

            // 3. Najdeme ty, co musíme SMAZAT (jsou v DB, ale už nejsou zaškrtnuté)
            var toDelete = currentRelations.Where(sr => !desiredRoleIds.Contains(sr.RoleId)).ToList();
            if (toDelete.Any())
            {
                _context.StaffRoles.RemoveRange(toDelete);
            }

            // 4. Najdeme ty, co musíme PØIDAT (jsou zaškrtnuté, ale nejsou v DB)
            var existingRoleIds = currentRelations.Select(sr => sr.RoleId).ToList();
            var toAddIds = desiredRoleIds.Except(existingRoleIds).ToList();

            foreach (var roleId in toAddIds)
            {
                _context.StaffRoles.Add(new StaffRole
                {
                    StaffId = staffId,
                    RoleId = roleId
                });
            }

            _context.SaveChanges();
        }

        private void ClearForm()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            // Resetujeme CheckBoxy
            foreach (var item in _roleSelections) item.IsSelected = false;
            lstRoles.Items.Refresh();

            dpHireDate.SelectedDate = DateTime.Now;
            chkActive.IsChecked = true;
            txtNotes.Text = "";
            _selectedStaff = null;
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
            }
            catch { }
        }

        private void BtnAddStaff_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedStaff = null;
            txtFirstName.Focus();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void DgStaff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStaff = dgStaff.SelectedItem as Staff;
            if (_selectedStaff != null) LoadStaffDetails(_selectedStaff);
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
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }

    // OPRAVA 3: Tøída RoleSelection je nyní SPRÁVNÌ definovaná zde
    public class RoleSelection
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}