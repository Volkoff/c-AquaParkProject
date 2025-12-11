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
            LoadVisitors();
            LoadBookings();
            ClearForm();
        }

        private void LoadVisitors()
        {
            try
            {
                var visitors = _context.Visitors.ToList();
                cmbVisitor.ItemsSource = visitors;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error loading visitors: {ex.Message}";
            }
        }

        private void LoadBookings()
        {
            try
            {
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
                lblStatus.Text = $"Selected booking: {_selectedBooking.BookingRef}";
            }
        }

        private void BtnAddBooking_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedBooking = null;
            cmbVisitor.Focus();
            lblStatus.Text = "Ready to add new booking";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbVisitor.SelectedValue == null)
                {
                    MessageBox.Show("Please select a visitor.");
                    lblStatus.Text = "Please select a visitor.";
                    return;
                }
                int visitorId = (int)cmbVisitor.SelectedValue;

                if (_selectedBooking == null)
                {
                    var newBooking = new Booking
                    {
                        BookingRef = txtBookingRef.Text.Trim(),
                        VisitorId = visitorId,
                        CreatedDate = dpCreatedDate.SelectedDate ?? DateTime.Now,
                        Status = cmbStatus.Text,
                        Notes = txtNotes.Text.Trim(),
                        TotalAmount = 0 // Inicializace na 0, DB si to dopo��t� podle polo�ek
                    };

                    _context.Bookings.Add(newBooking);
                }
                else
                {
                    _selectedBooking.BookingRef = txtBookingRef.Text.Trim();
                    _selectedBooking.VisitorId = visitorId;
                    _selectedBooking.CreatedDate = dpCreatedDate.SelectedDate ?? _selectedBooking.CreatedDate;
                    _selectedBooking.Status = cmbStatus.Text;
                    _selectedBooking.Notes = txtNotes.Text.Trim();
                    // TotalAmount neaktualizujeme, d�l� to DB
                }

                _context.SaveChanges();
                LoadBookings();
                ClearForm();
                lblStatus.Text = "Booking saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving booking: {ex.Message}");
                lblStatus.Text = $"Error saving booking: {ex.Message}";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBooking != null)
            {
                _context.Bookings.Remove(_selectedBooking);
                _context.SaveChanges();
                LoadBookings();
                ClearForm();
                lblStatus.Text = "Booking deleted successfully";
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedBooking = null; }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText)) { LoadBookings(); return; }
            try
            {
                dgBookings.ItemsSource = _context.Bookings.Include(b => b.Visitor)
                    .Where(b => (b.BookingRef != null && b.BookingRef.ToLower().Contains(searchText)))
                    .ToList();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error filtering bookings: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            txtBookingRef.Text = "";
            cmbVisitor.SelectedIndex = -1;
            dpCreatedDate.SelectedDate = DateTime.Now;
            txtTotalAmount.Text = "";
            cmbStatus.SelectedIndex = 0;
            txtNotes.Text = "";
            lblStatus.Text = "Form cleared";
        }
    }
}