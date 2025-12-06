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
            var staff = _ctx.Staff.Include(s => s.Address).ThenInclude(a => a.PostalCode).ToList();
            dgStaff.ItemsSource = staff;
            cmbShiftStaff.ItemsSource = staff;
            cmbCertStaff.ItemsSource = staff;
            cmbTrainStaff.ItemsSource = staff;

            dgShifts.ItemsSource = _ctx.StaffShifts.Include(s => s.Staff).Include(s => s.Shift).OrderByDescending(s => s.ShiftDate).ToList();
            cmbShiftType.ItemsSource = _ctx.Shifts.ToList();

            dgCerts.ItemsSource = _ctx.Certifications.Include(c => c.Staff).ToList();
            dgTraining.ItemsSource = _ctx.Training.Include(t => t.Staff).ToList();
        }

        private void DgStaff_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selStaff = dgStaff.SelectedItem as Staff;
            if (_selStaff != null)
            {
                txtFn.Text = _selStaff.FirstName;
                txtLn.Text = _selStaff.LastName;
                txtEmail.Text = _selStaff.Email;
                txtPhone.Text = _selStaff.Phone; // Nové
                txtJob.Text = _selStaff.JobTitle; // Nové

                if (_selStaff.Address != null)
                {
                    txtHouse.Text = _selStaff.Address.HouseNumber;
                    txtCity.Text = _selStaff.Address.PostalCode?.City;
                }
            }
        }

        private void BtnAddStaff_Click(object sender, RoutedEventArgs e)
        {
            _selStaff = null;
            txtFn.Text = ""; txtLn.Text = ""; txtEmail.Text = ""; txtPhone.Text = ""; txtJob.Text = ""; txtHouse.Text = ""; txtCity.Text = "";
        }

        private void BtnSaveStaff_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHouse.Text)) { MessageBox.Show("Address required!"); return; }

            // Jednoduchá logika pro přiřazení PSČ podle města
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

            _ctx.SaveChanges(); RefreshAll();
        }

        private void BtnAssignShift_Click(object sender, RoutedEventArgs e)
        {
            if (dpShiftDate.SelectedDate == null || cmbShiftStaff.SelectedValue == null || cmbShiftType.SelectedValue == null) return;
            _ctx.StaffShifts.Add(new StaffShift { ShiftDate = dpShiftDate.SelectedDate.Value, StaffId = (int)cmbShiftStaff.SelectedValue, ShiftId = (int)cmbShiftType.SelectedValue });
            _ctx.SaveChanges(); RefreshAll();
        }

        private void BtnAddCert_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCertStaff.SelectedValue == null) return;
            _ctx.Certifications.Add(new Certification { StaffId = (int)cmbCertStaff.SelectedValue, CertName = txtCertName.Text });
            _ctx.SaveChanges(); RefreshAll();
        }

        private void BtnAddTrain_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTrainStaff.SelectedValue == null) return;
            _ctx.Training.Add(new Training { StaffId = (int)cmbTrainStaff.SelectedValue, TrainingName = txtTrainName.Text });
            _ctx.SaveChanges(); RefreshAll();
        }
    }
}