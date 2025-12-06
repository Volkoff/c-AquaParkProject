using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

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
            LoadVisitors(); // Nová metoda
            LoadBookings();
            ClearForm();
        }

        // --- NOVÉ: Naètení návštìvníkù pro ComboBox ---
        private void LoadVisitors()
        {
            try
            {
                var visitors = _context.Visitors.ToList();
                cmbVisitor.ItemsSource = visitors;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading visitors: " + ex.Message);
            }
        }

        private void LoadBookings()
        {
            try
            {
                // Musíme naèíst i data o Visitorovi (Include), abychom vidìli jméno
                var bookings = _context.Bookings
                    .Include(b => b.Visitor)
                    .ToList();
                dgBookings.ItemsSource = bookings;
                lblStatus.Text = $"Loaded {bookings.Count} bookings";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText)) { LoadBookings(); return; }

            try
            {
                // Filtrujeme podle referenèního èísla
                var filteredBookings = _context.Bookings
                    .Include(b => b.Visitor)
                    .Where(b => (b.BookingRef != null && b.BookingRef.ToLower().Contains(searchText)))
                    .ToList();

                dgBookings.ItemsSource = filteredBookings;
            }
            catch { }
        }

        private void DgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBooking = dgBookings.SelectedItem as Booking;
            if (_selectedBooking != null)
            {
                txtBookingRef.Text = _selectedBooking.BookingRef ?? "";

                // --- NOVÉ: Nastavení vybraného návštìvníka ---
                cmbVisitor.SelectedValue = _selectedBooking.VisitorId;

                dpCreatedDate.SelectedDate = _selectedBooking.CreatedDate;
                txtTotalAmount.Text = _selectedBooking.TotalAmount.ToString();
                cmbStatus.Text = _selectedBooking.Status;
                txtNotes.Text = _selectedBooking.Notes ?? "";
            }
        }

        private void BtnAddBooking_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedBooking = null;
            cmbVisitor.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtTotalAmount.Text, out decimal totalAmount)) totalAmount = 0;

                // Validace: Musí být vybrán návštìvník
                if (cmbVisitor.SelectedValue == null)
                {
                    MessageBox.Show("Please select a visitor.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int visitorId = (int)cmbVisitor.SelectedValue;

                if (_selectedBooking == null)
                {
                    var newBooking = new Booking
                    {
                        BookingRef = txtBookingRef.Text.Trim(),
                        VisitorId = visitorId, // --- ZDE SE UKLÁDÁ ID ---
                        CreatedDate = dpCreatedDate.SelectedDate ?? DateTime.Now,
                        TotalAmount = totalAmount,
                        Status = cmbStatus.Text,
                        Notes = txtNotes.Text.Trim()
                    };

                    _context.Bookings.Add(newBooking);
                }
                else
                {
                    _selectedBooking.BookingRef = txtBookingRef.Text.Trim();
                    _selectedBooking.VisitorId = visitorId; // --- ZDE SE UKLÁDÁ ID ---
                    _selectedBooking.CreatedDate = dpCreatedDate.SelectedDate ?? _selectedBooking.CreatedDate;
                    _selectedBooking.TotalAmount = totalAmount;
                    _selectedBooking.Status = cmbStatus.Text;
                    _selectedBooking.Notes = txtNotes.Text.Trim();
                }

                _context.SaveChanges();
                LoadBookings();
                ClearForm();
                lblStatus.Text = "Booking saved successfully";
            }
            catch (Exception ex)
            {
                // Výpis detailní chyby (èasto Oracle chyb)
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Error saving booking: {msg}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBooking == null) return;
            try
            {
                _context.Bookings.Remove(_selectedBooking);
                _context.SaveChanges();
                LoadBookings();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedBooking = null; }

        private void ClearForm()
        {
            txtBookingRef.Text = "";
            cmbVisitor.SelectedIndex = -1;
            dpCreatedDate.SelectedDate = DateTime.Now;
            txtTotalAmount.Text = "";
            cmbStatus.SelectedIndex = 0;
            txtNotes.Text = "";
        }
    }
}