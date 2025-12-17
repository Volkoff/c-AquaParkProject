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
            LoadVisitors();
            LoadTicketTypes();
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

                cmbVisitor.IsEnabled = false;
                cmbStatus.IsEnabled = false;
                txtNotes.IsReadOnly = true;
                dpCreatedDate.IsEnabled = false;
                
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

                    cmbVisitor.IsEnabled = false;  // Auto-select their own visitor record
                    cmbStatus.IsEnabled = false;   // Can't change status
                    txtNotes.IsReadOnly = false;
                    dpCreatedDate.IsEnabled = false;
                    
                    // Show ticket creation fields for visitors
                    cmbTicketType.IsEnabled = true;
                    txtTicketQuantity.IsReadOnly = false;
                    dpReservationDate.IsEnabled = true;
                    txtReservationTime.IsReadOnly = false;

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

                    cmbVisitor.IsEnabled = true;
                    cmbStatus.IsEnabled = true;
                    txtNotes.IsReadOnly = false;
                    dpCreatedDate.IsEnabled = true;

                    lblStatus.Text = "Zaměstnanec: Úplný přístup na správu rezervací.";
                }
            }
        }

        private void LoadVisitors()
        {
            try { cmbVisitor.ItemsSource = _context.Visitors.ToList(); }
            catch (Exception ex) { lblStatus.Text = "Chyba: " + ex.Message; }
        }

        private void LoadTicketTypes()
        {
            try { cmbTicketType.ItemsSource = _context.TicketTypes.ToList(); }
            catch (Exception ex) { lblStatus.Text = "Chyba při načítání typů vstupenek: " + ex.Message; }
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
                txtBookingRef.Text = _selectedBooking.BookingRef ?? "";
                cmbVisitor.SelectedValue = _selectedBooking.VisitorId;
                dpCreatedDate.SelectedDate = _selectedBooking.CreatedDate;
                txtTotalAmount.Text = _selectedBooking.TotalAmount.ToString("C");
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
                if (cmbVisitor.SelectedValue == null)
                {
                    MessageBox.Show("Vyberte návštěvníka.");
                    return;
                }

                if (_selectedBooking == null)
                {
                    // Creating NEW reservation with automatic ticket creation
                    int visitorId = (int)cmbVisitor.SelectedValue;

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

                    if (!isStaff && App.CurrentUser?.VisitorId.HasValue == true)
                    {
                        visitorId = App.CurrentUser.VisitorId.Value;
                    }

                    // Create the booking
                    var newBooking = new Booking
                    {
                        VisitorId = visitorId,
                        BookingRef = $"BOOK-{DateTime.Now:yyyyMMddHHmmss}",
                        CreatedDate = DateTime.Now,
                        Status = "PENDING",
                        Notes = txtNotes.Text ?? "",
                        TotalAmount = 0
                    };
                    _context.Bookings.Add(newBooking);
                    _context.SaveChanges();

                    // Auto-create tickets if ticket type and quantity are specified
                    if (cmbTicketType.SelectedValue != null && !string.IsNullOrWhiteSpace(txtTicketQuantity.Text))
                    {
                        if (int.TryParse(txtTicketQuantity.Text, out int quantity) && quantity > 0)
                        {
                            int ticketTypeId = (int)cmbTicketType.SelectedValue;

                            // Get the price for this ticket type
                            var priceItem = _context.PriceListItems
                                .Where(pli => pli.TicketTypeId == ticketTypeId &&
                                              pli.ValidFrom <= DateTime.Now &&
                                              (pli.ValidTo == null || pli.ValidTo >= DateTime.Now))
                                .FirstOrDefault();

                            decimal unitPrice = priceItem?.UnitPrice ?? 0;

                            // Create booking items (tickets)
                            for (int i = 0; i < quantity; i++)
                            {
                                var bookingItem = new BookingItem
                                {
                                    BookingId = newBooking.BookingId,
                                    TicketTypeId = ticketTypeId,
                                    Quantity = 1,
                                    UnitPrice = unitPrice
                                };
                                _context.BookingItems.Add(bookingItem);
                            }
                            _context.SaveChanges();

                            MessageBox.Show($"Rezervace vytvořena! {quantity} vstupenek přidáno.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Rezervace vytvořena!");
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
            txtBookingRef.Clear();
            cmbVisitor.SelectedIndex = -1;
            txtNotes.Clear();
            dpCreatedDate.SelectedDate = DateTime.Now;
            txtTotalAmount.Clear();
            cmbStatus.SelectedIndex = 0; // CONFIRMED
            dpReservationDate.SelectedDate = DateTime.Now;
            txtReservationTime.Clear();
            cmbTicketType.SelectedIndex = -1;
            txtTicketQuantity.Text = "1";
            _selectedBooking = null;

            // Pre-fill visitor for non-staff users
            if (App.CurrentUser?.VisitorId.HasValue == true)
            {
                cmbVisitor.SelectedValue = App.CurrentUser.VisitorId;
            }
        }
    }
}