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
            LoadBookings();
            ClearForm();
        }

        private void LoadVisitors()
        {
            try { cmbVisitor.ItemsSource = _context.Visitors.ToList(); }
            catch (Exception ex) { lblStatus.Text = "Chyba: " + ex.Message; }
        }

        private void LoadBookings()
        {
            try
            {
                // Místo tabulky Bookings načítáme POHLED BookingOverviews
                // Tím plníme bod 3 (Využití pohledů v aplikaci)
                var views = _context.BookingOverviews.ToList();

                // Pozor: DataGrid v XAML musí mít Binding na sloupce z BookingOverview (např. VisitorName místo Visitor.FullName)
                dgBookings.ItemsSource = views;
            }
            catch (Exception ex) { /*...*/ }
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
                    // VKLÁDÁNÍ PŘES PROCEDURU
                    var pVisitor = new OracleParameter("p_visitor_id", OracleDbType.Int32) { Value = (int)cmbVisitor.SelectedValue };
                    var pNotes = new OracleParameter("p_notes", OracleDbType.Varchar2) { Value = txtNotes.Text ?? "" };
                    var pOutId = new OracleParameter("p_booking_id", OracleDbType.Int32) { Direction = ParameterDirection.Output };

                    _context.Database.ExecuteSqlRaw(
                        "BEGIN SP_CREATE_BOOKING(:p_visitor_id, :p_notes, :p_booking_id); END;",
                        pVisitor, pNotes, pOutId
                    );

                    MessageBox.Show($"Rezervace vytvořena procedurou! ID: {pOutId.Value}");
                }
                else
                {
                    // Update přes EF (nebo dopsat proceduru)
                    _selectedBooking.Status = cmbStatus.Text;
                    _selectedBooking.Notes = txtNotes.Text;
                    _context.SaveChanges();
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
            txtBookingRef.Clear(); cmbVisitor.SelectedIndex = -1; txtNotes.Clear();
            dpCreatedDate.SelectedDate = DateTime.Now; txtTotalAmount.Clear();
        }
    }
}