using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AquaParkManager.Windows
{
    public partial class BookingsManagementWindow : Window
    {
        private AquaParkContext _context;
        private Booking? _selectedBooking;

        public BookingsManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadBookings();
            ClearForm();
            ApplyPermissions(); // Apply role-based access control
            
            // Refresh data when window is activated (brought to focus)
            this.Activated += (s, e) => LoadBookings();
        }

        private void ApplyPermissions()
        {
            if (App.CurrentUser == null)
            {
                // === GUEST MODE ===
                this.Title += " - Prohlížení (Host)";
                btnAddBooking.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;

                cmbStatus.IsEnabled = false;
                txtNotes.IsReadOnly = true;

                
                lblStatus.Text = "Host: Prohlížíte dostupné rezervace (čtení).";
            }
            else
            {
                // Check if user is staff/admin
                bool isStaff = false;
                using (var ctx = new AquaParkContext())
                {
                    var userRoles = ctx.UserRoles.Include(ur => ur.Role)
                        .Where(ur => ur.UserId == App.CurrentUser.UserId)
                        .Select(ur => ur.Role != null ? ur.Role.RoleName : "")
                        .ToList();
                    isStaff = userRoles.Contains("ADMIN") || userRoles.Contains("MANAGER") || userRoles.Contains("STAFF");
                }

                if (!isStaff)
                {
                    // === VISITOR (NON-STAFF) MODE ===
                    this.Title += " - Moje rezervace";
                    btnAddBooking.Visibility = Visibility.Visible;
                    btnSave.Visibility = Visibility.Visible;
                    btnDelete.Visibility = Visibility.Visible;
                    btnClear.Visibility = Visibility.Visible;

                    cmbStatus.IsEnabled = false;   // Can't change status
                    txtNotes.IsReadOnly = false;

                    


                    lblStatus.Text = "Návštěvník: Vytvářejte a spravujte své vlastní rezervace s vstupenkami.";
                }
                else
                {
                    // === STAFF/ADMIN MODE ===
                    this.Title += " - Správa";
                    btnAddBooking.Visibility = Visibility.Visible;
                    btnSave.Visibility = Visibility.Visible;
                    btnDelete.Visibility = Visibility.Visible;
                    btnClear.Visibility = Visibility.Visible;

                    cmbStatus.IsEnabled = true;
                    txtNotes.IsReadOnly = false;


                    lblStatus.Text = "Zaměstnanec: Úplný přístup na správu rezervací.";
                }
            }
        }





        private void LoadBookings()
        {
            try
            {
                // Check if user is staff
                bool isStaff = false;
                if (App.CurrentUser != null)
                {
                    var userRoles = _context.UserRoles.Include(ur => ur.Role)
                        .Where(ur => ur.UserId == App.CurrentUser.UserId)
                        .Select(ur => ur.Role != null ? ur.Role.RoleName : "")
                        .ToList();
                    isStaff = userRoles.Contains("ADMIN") || userRoles.Contains("MANAGER") || userRoles.Contains("STAFF");
                }

                // Load fresh bookings from database
                var bookings = _context.Bookings.Include(b => b.Visitor).ToList();

                // Filter by current user if not staff
                if (App.CurrentUser != null && !isStaff && App.CurrentUser.VisitorId.HasValue)
                {
                    bookings = bookings.Where(b => b.VisitorId == App.CurrentUser.VisitorId.Value).ToList();
                }

                // Refresh the DataGrid binding
                dgBookings.ItemsSource = null;
                dgBookings.ItemsSource = bookings;
                lblStatus.Text = $"Načteno {bookings.Count} rezervací.";
            }
            catch (Exception ex) { lblStatus.Text = "Chyba: " + ex.Message; }
        }

        private void DgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBooking = dgBookings.SelectedItem as Booking;
            if (_selectedBooking != null)
            {
                cmbStatus.Text = _selectedBooking.Status;
                txtNotes.Text = _selectedBooking.Notes ?? "";
            }
        }

        private void BtnAddBooking_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedBooking = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
             

                if (_selectedBooking == null)
                {
                    // Creating NEW reservation with automatic ticket creation

                    // For visitors, auto-set their own visitor ID
                    bool isStaff = false;
                    if (App.CurrentUser != null)
                    {
                        var userRoles = _context.UserRoles.Include(ur => ur.Role)
                            .Where(ur => ur.UserId == App.CurrentUser.UserId)
                            .Select(ur => ur.Role != null ? ur.Role.RoleName : "")
                            .ToList();
                        isStaff = userRoles.Contains("ADMIN") || userRoles.Contains("MANAGER") || userRoles.Contains("STAFF");
                    }


                }
                else
                {
                    // Update existing booking (non-staff only)
                    bool isStaff = false;
                    if (App.CurrentUser != null)
                    {
                        var userRoles = _context.UserRoles.Include(ur => ur.Role)
                            .Where(ur => ur.UserId == App.CurrentUser.UserId)
                            .Select(ur => ur.Role != null ? ur.Role.RoleName : "")
                            .ToList();
                        isStaff = userRoles.Contains("ADMIN") || userRoles.Contains("MANAGER") || userRoles.Contains("STAFF");
                    }

                    if (isStaff)
                    {
                        _selectedBooking.Status = cmbStatus.Text;
                    }
                    _selectedBooking.Notes = txtNotes.Text;
                    _context.SaveChanges();
                    MessageBox.Show("Rezervace aktualizována!");
                }

                LoadBookings();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBooking != null)
            {
                var pId = new OracleParameter("p_booking_id", _selectedBooking.BookingId);
                var pReason = new OracleParameter("p_reason", "Smazáno uživatelem");
                _context.Database.ExecuteSqlRaw("BEGIN SP_CANCEL_BOOKING(:p_booking_id, :p_reason); END;", pId, pReason);
                LoadBookings();
                ClearForm();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedBooking = null; }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }
        private void ClearForm()
        {

            txtNotes.Clear();


            cmbStatus.SelectedIndex = 0; // CONFIRMED

            _selectedBooking = null;


        }
    }
}