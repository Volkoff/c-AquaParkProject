using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class TicketsManagementWindow : Window
    {
        private AquaParkContext _context;
        private BookingItem? _selectedItem;

        public TicketsManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadBookings();
            LoadTicketTypes();
            LoadTickets();
            ClearForm();
            ApplyPermissions(); // <--- ZABEZPE�EN�
            
            // Refresh data when window is activated (brought to focus)
            this.Activated += (s, e) => LoadTickets();
        }

        private void ApplyPermissions()
        {
            if (App.CurrentUser == null)
            {
                // === GUEST MODE ===
                this.Title += " - Nákup (Host)";
                btnAddTicket.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;

                cmbBooking.IsEnabled = false;
                cmbTicketType.IsEnabled = true;  // Guest CAN select ticket type
                txtQuantity.IsReadOnly = false;  // Guest CAN select quantity
                txtPricePaid.IsReadOnly = true;  // Price is auto-filled

                lblStatus.Text = "Host: Vyberte vstupenku a množství pro nákup.";
            }
            else
            {
                // === REGISTERED USER/STAFF MODE ===
                this.Title += " - Správa";
                
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
                    btnAddTicket.Visibility = Visibility.Collapsed;
                    btnSave.Visibility = Visibility.Visible;  // Visitor can purchase
                    btnDelete.Visibility = Visibility.Collapsed;
                    btnClear.Visibility = Visibility.Visible;

                    cmbBooking.IsEnabled = false;  // Auto-select their booking
                    cmbTicketType.IsEnabled = true;
                    txtQuantity.IsReadOnly = false;
                    txtPricePaid.IsReadOnly = true;

                    lblStatus.Text = "Návštěvník: Vyberte vstupenku a pak klikněte Uložit pro nákup.";
                }
                else
                {
                    // === STAFF/ADMIN MODE ===
                    btnAddTicket.Visibility = Visibility.Visible;
                    btnSave.Visibility = Visibility.Visible;
                    btnDelete.Visibility = Visibility.Visible;
                    btnClear.Visibility = Visibility.Visible;

                    cmbBooking.IsEnabled = true;
                    cmbTicketType.IsEnabled = true;
                    txtQuantity.IsReadOnly = false;
                    txtPricePaid.IsReadOnly = false;

                    lblStatus.Text = "Zaměstnanec: Úplný přístup.";
                }
            }
        }

        // Load data from database
        private void LoadBookings() 
        { 
            cmbBooking.ItemsSource = _context.Bookings.ToList(); 
        }

        private void LoadTicketTypes() 
        { 
            cmbTicketType.ItemsSource = _context.TicketTypes.ToList(); 
        }

        private void LoadTickets() 
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

                // Get fresh data from database
                var tickets = _context.BookingItems.Include(t => t.TicketType).ToList();

                // Filter to only user's tickets if not staff
                if (App.CurrentUser != null && !isStaff && App.CurrentUser.VisitorId.HasValue)
                {
                    var userVisitorId = App.CurrentUser.VisitorId.Value;
                    var userBookingIds = _context.Bookings
                        .Where(b => b.VisitorId == userVisitorId)
                        .Select(b => b.BookingId)
                        .ToList();
                    tickets = tickets.Where(t => userBookingIds.Contains(t.BookingId)).ToList();
                }

                // Refresh the DataGrid binding
                dgTickets.ItemsSource = null;
                dgTickets.ItemsSource = tickets;
                
                lblStatus.Text = $"Načteno {tickets.Count} vstupenek.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Chyba při načítání vstupenek: " + ex.Message;
            }
        }

        // Auto-populate price when ticket type is selected
        private void CmbTicketType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTicketType.SelectedValue != null && int.TryParse(cmbTicketType.SelectedValue.ToString(), out int ticketTypeId))
            {
                // Get the current price for this ticket type from PriceListItem
                var priceItem = _context.PriceListItems
                    .Where(pli => pli.TicketTypeId == ticketTypeId && 
                                  pli.ValidFrom <= DateTime.Now && 
                                  (pli.ValidTo == null || pli.ValidTo >= DateTime.Now))
                    .FirstOrDefault();

                if (priceItem != null)
                {
                    txtPricePaid.Text = priceItem.UnitPrice.ToString("F2");
                }
                else
                {
                    txtPricePaid.Text = "0.00";
                }
            }
        }
        private void DgTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedItem = dgTickets.SelectedItem as BookingItem;
            if (_selectedItem != null)
            {
                cmbBooking.SelectedValue = _selectedItem.BookingId;
                cmbTicketType.SelectedValue = _selectedItem.TicketTypeId;
                txtQuantity.Text = _selectedItem.Quantity.ToString();
                txtPricePaid.Text = _selectedItem.UnitPrice.ToString();
            }
        }
        private void BtnAddTicket_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedItem = null; }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate ticket type selection
                if (cmbTicketType.SelectedValue == null)
                {
                    MessageBox.Show("Prosím vyberte typ vstupenky.");
                    return;
                }

                if (!decimal.TryParse(txtPricePaid.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Chyba: Cena musí být kladné číslo.");
                    return;
                }

                int bookingId;

                if (cmbBooking.SelectedValue != null && cmbBooking.SelectedValue != DBNull.Value)
                {
                    // Staff or visitor with existing booking
                    bookingId = (int)cmbBooking.SelectedValue;
                }
                else if (App.CurrentUser != null)
                {
                    // Guest/Visitor - create a new booking for purchase
                    var visitor = _context.Visitors.FirstOrDefault(v => v.VisitorId == App.CurrentUser.VisitorId);
                    if (visitor == null)
                    {
                        MessageBox.Show("Chyba: Nelze najít návštěvníka.");
                        return;
                    }

                    var newBooking = new Booking
                    {
                        VisitorId = visitor.VisitorId,
                        BookingRef = $"BOOK-{DateTime.Now:yyyyMMddHHmmss}",
                        CreatedDate = DateTime.Now,
                        Status = "PENDING",
                        TotalAmount = 0
                    };
                    _context.Bookings.Add(newBooking);
                    _context.SaveChanges();
                    bookingId = newBooking.BookingId;
                }
                else
                {
                    MessageBox.Show("Chyba: Musíte vybrat nebo vytvořit rezervaci.");
                    return;
                }

                // Create or update booking item
                if (_selectedItem == null)
                {
                    _context.BookingItems.Add(new BookingItem
                    {
                        BookingId = bookingId,
                        TicketTypeId = (int)cmbTicketType.SelectedValue,
                        Quantity = int.Parse(txtQuantity.Text),
                        UnitPrice = price
                    });
                }
                else
                {
                    _selectedItem.Quantity = int.Parse(txtQuantity.Text);
                    _selectedItem.UnitPrice = price;
                }

                _context.SaveChanges();
                LoadBookings();
                LoadTickets();
                MessageBox.Show("Vstupenka úspěšně přidána!");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba: {ex.Message}");
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e) { if (App.CurrentUser != null && _selectedItem != null) { _context.BookingItems.Remove(_selectedItem); _context.SaveChanges(); LoadTickets(); } }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }
        private void ClearForm() { cmbBooking.SelectedIndex = -1; cmbTicketType.SelectedIndex = -1; txtQuantity.Text = "1"; txtPricePaid.Clear(); _selectedItem = null; }
    }
}