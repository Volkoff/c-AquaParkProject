using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class SchedulingWindow : Window
    {
        private AquaParkContext _context;
        private Shift? _selectedShift;

        public SchedulingWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadShifts();
            LoadStaff();
            LoadStaffShifts();
            dpAssignDate.SelectedDate = DateTime.Today;
            dpFilterDate.SelectedDate = DateTime.Today;
        }

        private void LoadShifts()
        {
            try
            {
                var shifts = _context.Shifts.ToList();
                dgShifts.ItemsSource = shifts;
                cmbShiftAssign.ItemsSource = shifts;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading shifts: " + ex.Message);
                lblStatus.Text = $"Error loading shifts: {ex.Message}";
            }
        }

        private void LoadStaff()
        {
            try
            {
                var staff = _context.Staff.Where(s => s.Active == "Y").ToList();
                cmbStaffAssign.ItemsSource = staff;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading staff: " + ex.Message);
                lblStatus.Text = $"Error loading staff: {ex.Message}";
            }
        }

        private void LoadStaffShifts()
        {
            try
            {
                var filterDate = dpFilterDate.SelectedDate ?? DateTime.Today;
                var staffShifts = _context.StaffShifts
                    .Include(ss => ss.Staff)
                    .Include(ss => ss.Shift)
                    .Where(ss => ss.ShiftDate.Date == filterDate.Date)
                    .Select(ss => new StaffShiftViewModel
                    {
                        StaffShiftId = ss.StaffShiftId,
                        Staff = ss.Staff,
                        Shift = ss.Shift,
                        ShiftDate = ss.ShiftDate,
                        ShiftTime = ss.Shift != null ? $"{ss.Shift.StartTime:HH:mm} - {ss.Shift.EndTime:HH:mm}" : ""
                    })
                    .ToList();

                dgStaffShifts.ItemsSource = staffShifts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff shifts: {ex.Message}");
                lblStatus.Text = $"Error loading staff shifts: {ex.Message}";
            }
        }

        private void DgShifts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgShifts.SelectedItem is Shift shift)
            {
                _selectedShift = shift;
                txtShiftName.Text = shift.ShiftName;
                txtStartTime.Text = shift.StartTime.ToString("HH:mm");
                txtEndTime.Text = shift.EndTime.ToString("HH:mm");
            }
        }

        private void BtnAddShift_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtShiftName.Text))
            {
                MessageBox.Show("Please enter shift name.");
                lblStatus.Text = "Please enter shift name.";
                return;
            }

            try
            {
                var today = DateTime.Today;
                var shift = new Shift
                {
                    ShiftName = txtShiftName.Text,
                    StartTime = DateTime.Parse($"{today:yyyy-MM-dd} {txtStartTime.Text}"),
                    EndTime = DateTime.Parse($"{today:yyyy-MM-dd} {txtEndTime.Text}")
                };

                _context.Shifts.Add(shift);
                _context.SaveChanges();

                MessageBox.Show("Shift added successfully!");
                LoadShifts();
                txtShiftName.Clear();
                txtStartTime.Clear();
                txtEndTime.Clear();
                lblStatus.Text = "Shift added successfully!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding shift: {ex.Message}");
                lblStatus.Text = $"Error adding shift: {ex.Message}";
            }
        }

        private void BtnDeleteShift_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShift == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this shift?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Shifts.Remove(_selectedShift);
                    _context.SaveChanges();
                    MessageBox.Show("Shift deleted successfully!");
                    lblStatus.Text = "Shift deleted successfully!";
                    LoadShifts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting shift: {ex.Message}");
                    lblStatus.Text = $"Error deleting shift: {ex.Message}";
                }
            }
        }

        private void BtnAssignShift_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStaffAssign.SelectedValue == null || cmbShiftAssign.SelectedValue == null || dpAssignDate.SelectedDate == null)
            {
                MessageBox.Show("Please select staff, shift, and date.");
                return;
            }

            try
            {
                var staffShift = new StaffShift
                {
                    StaffId = (int)cmbStaffAssign.SelectedValue,
                    ShiftId = (int)cmbShiftAssign.SelectedValue,
                    ShiftDate = dpAssignDate.SelectedDate.Value
                };

                _context.StaffShifts.Add(staffShift);
                _context.SaveChanges();

                MessageBox.Show("Shift assigned successfully!");
                lblStatus.Text = "Shift assigned successfully!";
                LoadStaffShifts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning shift: {ex.Message}");
                lblStatus.Text = $"Error assigning shift: {ex.Message}";
            }
        }

        private void DpFilterDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStaffShifts();
        }
    }

    public class StaffShiftViewModel
    {
        public int StaffShiftId { get; set; }
        public Staff? Staff { get; set; }
        public Shift? Shift { get; set; }
        public DateTime ShiftDate { get; set; }
        public string ShiftTime { get; set; } = string.Empty;
    }
}
