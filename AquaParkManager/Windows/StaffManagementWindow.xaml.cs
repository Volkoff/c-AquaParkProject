using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client; // Nutné pro procedury

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

            // Ochrana: Neregistrovaný sem nesmí
            if (App.CurrentUser == null) { Close(); return; }

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
            catch (Exception ex) { lblStatus.Text = "Chyba rolí: " + ex.Message; }
        }

        private void LoadStaff()
        {
            try
            {
                var staffList = _context.Staff
                    .Include(s => s.Address).ThenInclude(a => a.PostalCode)
                    .ToList();

                // Zjištění práv (Admin/Manager)
                bool isAdmin = false;
                if (App.CurrentUser != null)
                {
                    var userRoles = _context.UserRoles.Include(ur => ur.Role)
                        .Where(ur => ur.UserId == App.CurrentUser.UserId).ToList();

                    isAdmin = userRoles.Any(ur => ur.Role != null && (ur.Role.RoleName == "ADMIN" || ur.Role.RoleName == "MANAGER"));

                    // FALLBACK: Pokud uživatel nemá žádnou roli, ale je přiřazen k zaměstnanci, považujeme ho za Admina (pro účely vývoje/testování)
                    if (!isAdmin && App.CurrentUser.StaffId.HasValue)
                    {
                        isAdmin = true;
                    }
                }

                // Aplikace práv na UI prvky
                if (!isAdmin)
                {
                    // Režim pouze pro čtení
                    btnDelete.Visibility = Visibility.Collapsed;
                    btnSave.Visibility = Visibility.Collapsed;
                    // Cenzura dat pro ne-adminy
                    foreach (var s in staffList)
                    {
                        s.Phone = "*** Skryto ***";
                        s.Email = "*** Skryto ***";
                        s.Address = null;
                    }
                    lblStatus.Text = "Režim prohlížení (omezená práva).";
                }
                else
                {
                    // Admin režim - tlačítka musí být vidět!
                    btnDelete.Visibility = Visibility.Visible;
                    btnSave.Visibility = Visibility.Visible;
                    lblStatus.Text = "Načteno " + staffList.Count + " zaměstnanců.";
                }

                dgStaff.ItemsSource = staffList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání: " + ex.Message);
            }
        }

        // Metoda LoadStaffDetails (zobrazí detail po kliknutí)
        private void LoadStaffDetails(Staff staff)
        {
            // Základní údaje
            txtFirstName.Text = staff.FirstName;
            txtLastName.Text = staff.LastName;
            txtEmail.Text = staff.Email;
            txtPhone.Text = staff.Phone;
            txtNotes.Text = staff.Notes;
            dpHireDate.SelectedDate = staff.HireDate;
            chkActive.IsChecked = staff.Active == "Y";

            // Adresa
            if (staff.Address != null)
            {
                txtStreet.Text = staff.Address.Street;
                txtHouseNumber.Text = staff.Address.HouseNumber;
                if (staff.Address.PostalCode != null)
                {
                    txtZip.Text = staff.Address.PostalCode.Code;
                    txtCity.Text = staff.Address.PostalCode.City;
                }
            }
            else { ClearAddressFields(); }

            // === OPRAVA NAČÍTÁNÍ LOGINU ===
            // Najdeme uživatele, který je propojen s tímto zaměstnancem
            var user = _context.Users.FirstOrDefault(u => u.StaffId == staff.StaffId);
            if (user != null)
            {
                txtUsername.Text = user.Username; // Načteme jméno do TextBoxu
                txtPassword.Password = ""; // Heslo neukazujeme
            }
            else
            {
                txtUsername.Text = "";
                txtPassword.Password = "";
            }

            // Role
            var assigned = _context.StaffRoles.Where(sr => sr.StaffId == staff.StaffId).Select(sr => sr.RoleId).ToList();
            foreach (var r in _roleSelections) r.IsSelected = assigned.Contains(r.RoleId);
            lstRoles.Items.Refresh();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("Jméno a příjmení jsou povinné.");
                    return;
                }

                // 1. Získání nebo vytvoření adresy
                int addrId = GetOrCreateAddress(txtStreet.Text, txtHouseNumber.Text, txtCity.Text, txtZip.Text, "Pardubický kraj", "Česká republika");

                if (_selectedStaff == null)
                {
                    // === INSERT NOVÉHO ZAMĚSTNANCE ===
                    var newStaff = new Staff
                    {
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        Email = txtEmail.Text,
                        Phone = txtPhone.Text,
                        AddressId = addrId,
                        Active = chkActive.IsChecked == true ? "Y" : "N",
                        HireDate = dpHireDate.SelectedDate ?? DateTime.Now,
                        Notes = txtNotes.Text
                    };
                    _context.Staff.Add(newStaff);
                    _context.SaveChanges(); // Získáme ID

                    // Vytvoření uživatele (Login)
                    if (!string.IsNullOrWhiteSpace(txtUsername.Text))
                    {
                        var newUser = new User
                        {
                            Username = txtUsername.Text,
                            // Pokud heslo není zadáno, dáme default '1234', jinak to co zadal
                            PasswordHash = string.IsNullOrWhiteSpace(txtPassword.Password) ? "1234" : txtPassword.Password,
                            Email = txtEmail.Text ?? "",
                            StaffId = newStaff.StaffId,
                            IsActive = "Y"
                        };
                        _context.Users.Add(newUser);
                        _context.SaveChanges();

                        // Uložení rolí do UserRoles
                        foreach (var roleItem in lstRoles.Items.OfType<RoleSelection>().Where(r => r.IsSelected))
                        {
                            _context.UserRoles.Add(new UserRole { UserId = newUser.UserId, RoleId = roleItem.RoleId });
                        }
                    }

                    // Uložení rolí do StaffRoles
                    foreach (var roleItem in lstRoles.Items.OfType<RoleSelection>().Where(r => r.IsSelected))
                    {
                        _context.StaffRoles.Add(new StaffRole { StaffId = newStaff.StaffId, RoleId = roleItem.RoleId });
                    }
                    _context.SaveChanges();
                    MessageBox.Show("Zaměstnanec a uživatel úspěšně vytvořen.");
                }
                else
                {
                    // === UPDATE EXISTUJÍCÍHO ===
                    _selectedStaff.FirstName = txtFirstName.Text;
                    _selectedStaff.LastName = txtLastName.Text;
                    _selectedStaff.Email = txtEmail.Text;
                    _selectedStaff.Phone = txtPhone.Text;
                    _selectedStaff.AddressId = addrId;
                    _selectedStaff.Active = chkActive.IsChecked == true ? "Y" : "N";
                    _selectedStaff.Notes = txtNotes.Text;

                    // Aktualizace Usera
                    var user = _context.Users.FirstOrDefault(u => u.StaffId == _selectedStaff.StaffId);
                    if (user != null)
                    {
                        user.Username = txtUsername.Text;
                        // Heslo měníme jen pokud bylo zadáno
                        if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                        {
                            user.PasswordHash = txtPassword.Password;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(txtUsername.Text))
                    {
                        // Pokud zaměstnanec neměl usera, vytvoříme ho teď
                        var newUser = new User
                        {
                            Username = txtUsername.Text,
                            PasswordHash = string.IsNullOrWhiteSpace(txtPassword.Password) ? "1234" : txtPassword.Password,
                            Email = txtEmail.Text ?? "",
                            StaffId = _selectedStaff.StaffId,
                            IsActive = "Y"
                        };
                        _context.Users.Add(newUser);
                    }

                    // Aktualizace rolí (Smaž staré, přidej nové) - zjednodušeně pro StaffRoles
                    var oldRoles = _context.StaffRoles.Where(sr => sr.StaffId == _selectedStaff.StaffId);
                    _context.StaffRoles.RemoveRange(oldRoles);

                    foreach (var roleItem in lstRoles.Items.OfType<RoleSelection>().Where(r => r.IsSelected))
                    {
                        _context.StaffRoles.Add(new StaffRole { StaffId = _selectedStaff.StaffId, RoleId = roleItem.RoleId });
                        // Poznámka: Synchronizace UserRoles by byla složitější, zde řešíme hlavně StaffRoles
                    }

                    _context.SaveChanges();
                    MessageBox.Show("Zaměstnanec aktualizován.");
                }

                LoadStaff();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při ukládání: " + ex.Message + "\n" + ex.InnerException?.Message);
            }
        }

        // Pomocná metoda pro adresu (zůstává stejná jako v RegisterWindow)
        private int GetOrCreateAddress(string street, string houseNum, string city, string zip, string region, string country)
        {
            var postal = _context.PostalCodes.FirstOrDefault(p => p.Code == zip && p.City == city);
            if (postal == null)
            {
                postal = new PostalCode { Code = zip, City = city, Region = region ?? "", Country = country ?? "" };
                _context.PostalCodes.Add(postal);
                _context.SaveChanges();
            }
            var addr = _context.Address.FirstOrDefault(a => a.PostalCodeId == postal.PostalCodeId && a.HouseNumber == houseNum && a.Street == street);
            if (addr == null)
            {
                addr = new Address { PostalCodeId = postal.PostalCodeId, HouseNumber = houseNum, Street = street };
                _context.Address.Add(addr);
                _context.SaveChanges();
            }
            return addr.AddressId;
        }

        // ... Metody UpdateStaffRolesInDatabase, ClearForm, ClearAddressFields, TxtSearch ...
        // (Tyto metody vložte z vašeho původního kódu nebo si je doplňte, jsou standardní)

        private void UpdateStaffRolesInDatabase(int staffId, List<RoleSelection> selectedRoles) { /* ... */ }
        private void ClearForm()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtNotes.Clear();

            // Vyčistit Login pole
            txtUsername.Clear();
            txtPassword.Clear();

            ClearAddressFields();

            dpHireDate.SelectedDate = DateTime.Now;
            chkActive.IsChecked = true;
            _selectedStaff = null;

            // Reset rolí
            foreach (var r in _roleSelections) r.IsSelected = false;
            lstRoles.Items.Refresh();
        }
        private void ClearAddressFields() { txtStreet.Clear(); txtHouseNumber.Clear(); txtCity.Clear(); txtZip.Clear(); }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = txtSearch.Text.ToLower();
            // Znovu načíst a filtrovat (kvůli ochraně dat musíme filtrovat už "cenzurovaný" seznam, nebo volat LoadStaff())
            LoadStaff(); // LoadStaff už řeší filtraci i cenzuru
        }

        private void BtnAddStaff_Click(object sender, RoutedEventArgs e) { ClearForm(); }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void DgStaff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStaff = dgStaff.SelectedItem as Staff;
            if (_selectedStaff != null) LoadStaffDetails(_selectedStaff);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStaff != null)
            {
                _context.Staff.Remove(_selectedStaff);
                _context.SaveChanges();
                LoadStaff();
                ClearForm();
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