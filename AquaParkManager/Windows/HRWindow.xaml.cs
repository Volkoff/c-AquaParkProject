using System;
using System.Linq;
using System.Windows;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class HRWindow : Window
    {
        private AquaParkContext _ctx = new AquaParkContext();
        private Staff? _selStaff;

        public HRWindow()
        {
            InitializeComponent();
            RefreshAll();
        }

        private void RefreshAll()
        {
            try
            {
                // Načítání s vazbami
                var staff = _ctx.Staff
                    .Include(s => s.Address).ThenInclude(a => a.PostalCode)
                    .Include(s => s.StaffRoles).ThenInclude(sr => sr.Role) // Důležité pro role
                    .ToList();

                dgStaff.ItemsSource = staff;
                cmbShiftStaff.ItemsSource = staff;
                cmbCertStaff.ItemsSource = staff;
                cmbTrainStaff.ItemsSource = staff;

                cmbAvailableRoles.ItemsSource = _ctx.Roles.ToList();

                dgShifts.ItemsSource = _ctx.StaffShifts.Include(s => s.Staff).Include(s => s.Shift).OrderByDescending(s => s.ShiftDate).ToList();
                cmbShiftType.ItemsSource = _ctx.Shifts.ToList();

                dgCerts.ItemsSource = _ctx.Certifications.Include(c => c.Staff).ToList();
                dgTraining.ItemsSource = _ctx.Training.Include(t => t.Staff).ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void DgStaff_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selStaff = dgStaff.SelectedItem as Staff;
            if (_selStaff != null)
            {
                txtFn.Text = _selStaff.FirstName;
                txtLn.Text = _selStaff.LastName;
                txtEmail.Text = _selStaff.Email;
                txtPhone.Text = _selStaff.Phone;
                txtJob.Text = _selStaff.JobTitle;

                if (_selStaff.Address != null)
                {
                    txtHouse.Text = _selStaff.Address.HouseNumber;
                    txtCity.Text = _selStaff.Address.PostalCode?.City;
                }

                // Zobrazení rolí
                lstRoles.ItemsSource = _selStaff.StaffRoles.ToList();
            }
        }

        private void BtnAddRole_Click(object sender, RoutedEventArgs e)
        {
            if (_selStaff == null || cmbAvailableRoles.SelectedValue == null) return;
            try
            {
                int roleId = (int)cmbAvailableRoles.SelectedValue;

                if (_selStaff.StaffRoles.Any(sr => sr.RoleId == roleId))
                {
                    MessageBox.Show("Staff already has this role.");
                    return;
                }

                _ctx.StaffRoles.Add(new StaffRole
                {
                    StaffId = _selStaff.StaffId,
                    RoleId = roleId
                });

                _ctx.SaveChanges();
                RefreshAll();
                MessageBox.Show("Role assigned!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddStaff_Click(object sender, RoutedEventArgs e)
        {
            _selStaff = null;
            txtFn.Text = ""; txtLn.Text = ""; txtEmail.Text = ""; txtPhone.Text = ""; txtJob.Text = ""; txtHouse.Text = ""; txtCity.Text = "";
            lstRoles.ItemsSource = null;
        }

        private void BtnSaveStaff_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHouse.Text)) { MessageBox.Show("Address required!"); return; }
            if (string.IsNullOrWhiteSpace(txtFn.Text)) { MessageBox.Show("Name required!"); return; }

            try
            {
                string city = string.IsNullOrWhiteSpace(txtCity.Text) ? "Unknown" : txtCity.Text;
                var pc = _ctx.PostalCodes.FirstOrDefault(p => p.City == city) ?? new PostalCode { Code = "00000", City = city, Country = "CR", Region = "X" };

                if (pc.PostalCodeId == 0) _ctx.PostalCodes.Add(pc);

                if (_selStaff == null)
                {
                    var addr = new Address { HouseNumber = txtHouse.Text, PostalCode = pc };
                    _selStaff = new Staff { Address = addr, HireDate = DateTime.Now };
                    _ctx.Staff.Add(_selStaff);
                }
                else
                {
                    if (_selStaff.Address == null)
                        _selStaff.Address = new Address { HouseNumber = txtHouse.Text, PostalCode = pc };
                    else
                    {
                        _selStaff.Address.HouseNumber = txtHouse.Text;
                        _selStaff.Address.PostalCode = pc;
                    }
                }

                _selStaff.FirstName = txtFn.Text;
                _selStaff.LastName = txtLn.Text;
                _selStaff.Email = txtEmail.Text;
                _selStaff.Phone = txtPhone.Text;
                _selStaff.JobTitle = txtJob.Text;

                _ctx.SaveChanges();
                RefreshAll();
                MessageBox.Show("Staff saved!");
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        private void BtnAssignShift_Click(object sender, RoutedEventArgs e)
        {
            if (dpShiftDate.SelectedDate == null || cmbShiftStaff.SelectedValue == null || cmbShiftType.SelectedValue == null) return;
            try
            {
                _ctx.StaffShifts.Add(new StaffShift { ShiftDate = dpShiftDate.SelectedDate.Value, StaffId = (int)cmbShiftStaff.SelectedValue, ShiftId = (int)cmbShiftType.SelectedValue });
                _ctx.SaveChanges(); RefreshAll();
                MessageBox.Show("Shift Assigned!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddCert_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCertStaff.SelectedValue == null) return;
            try
            {
                _ctx.Certifications.Add(new Certification { StaffId = (int)cmbCertStaff.SelectedValue, CertName = txtCertName.Text });
                _ctx.SaveChanges(); RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddTrain_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTrainStaff.SelectedValue == null) return;
            try
            {
                _ctx.Training.Add(new Training { StaffId = (int)cmbTrainStaff.SelectedValue, TrainingName = txtTrainName.Text });
                _ctx.SaveChanges(); RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}