using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;

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
        }

        private void LoadBookings()
        {
            try
            {
                var bookings = _context.Bookings.ToList();
                dgBookings.ItemsSource = bookings;
                lblStatus.Text = $"Loaded {bookings.Count} bookings";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading bookings";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadBookings();
                return;
            }

            try
            {
                var filteredBookings = _context.Bookings
                    .Where(b => (b.BookingRef != null && b.BookingRef.ToLower().Contains(searchText)) ||
                               (b.CustomerName != null && b.CustomerName.ToLower().Contains(searchText)) ||
                               b.Status.ToLower().Contains(searchText))
                    .ToList();
                
                dgBookings.ItemsSource = filteredBookings;
                lblStatus.Text = $"Found {filteredBookings.Count} bookings";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBooking = dgBookings.SelectedItem as Booking;
            if (_selectedBooking != null)
            {
                LoadBookingDetails(_selectedBooking);
            }
        }

        private void LoadBookingDetails(Booking booking)
        {
            txtBookingRef.Text = booking.BookingRef ?? "";
            txtCustomerName.Text = booking.CustomerName ?? "";
            dpCreatedDate.SelectedDate = booking.CreatedDate;
            txtTotalAmount.Text = booking.TotalAmount.ToString();
            cmbStatus.Text = booking.Status;
            txtNotes.Text = booking.Notes ?? "";
        }

        private void BtnAddBooking_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedBooking = null;
            txtCustomerName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtTotalAmount.Text, out decimal totalAmount) || totalAmount < 0)
                {
                    MessageBox.Show("Please enter a valid total amount (>= 0).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedBooking == null)
                {
                    // Add new booking
                    var newBooking = new Booking
                    {
                        BookingRef = txtBookingRef.Text.Trim(),
                        CustomerName = txtCustomerName.Text.Trim(),
                        CreatedDate = dpCreatedDate.SelectedDate ?? DateTime.Now,
                        TotalAmount = totalAmount,
                        Status = cmbStatus.Text,
                        Notes = txtNotes.Text.Trim()
                    };

                    _context.Bookings.Add(newBooking);
                    _context.SaveChanges();
                    lblStatus.Text = "Booking added successfully";
                }
                else
                {
                    // Update existing booking
                    _selectedBooking.BookingRef = txtBookingRef.Text.Trim();
                    _selectedBooking.CustomerName = txtCustomerName.Text.Trim();
                    _selectedBooking.CreatedDate = dpCreatedDate.SelectedDate ?? _selectedBooking.CreatedDate;
                    _selectedBooking.TotalAmount = totalAmount;
                    _selectedBooking.Status = cmbStatus.Text;
                    _selectedBooking.Notes = txtNotes.Text.Trim();

                    _context.SaveChanges();
                    lblStatus.Text = "Booking updated successfully";
                }

                LoadBookings();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving booking";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBooking == null)
            {
                MessageBox.Show("Please select a booking to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var bookingName = _selectedBooking.CustomerName ?? "this booking";
            var result = MessageBox.Show(
                $"Are you sure you want to delete the booking for {bookingName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Bookings.Remove(_selectedBooking);
                    _context.SaveChanges();
                    LoadBookings();
                    ClearForm();
                    lblStatus.Text = "Booking deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error deleting booking";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedBooking = null;
            dgBookings.SelectedItem = null;
        }

        private void ClearForm()
        {
            txtBookingRef.Text = "";
            txtCustomerName.Text = "";
            dpCreatedDate.SelectedDate = DateTime.Now;
            txtTotalAmount.Text = "";
            cmbStatus.SelectedIndex = 0;
            txtNotes.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
