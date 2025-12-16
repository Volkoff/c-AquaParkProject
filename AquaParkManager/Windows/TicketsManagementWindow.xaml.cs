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
            ApplyPermissions(); // <--- ZABEZPEÈENÍ
        }

        private void ApplyPermissions()
        {
            if (App.CurrentUser == null)
            {
                this.Title += " (Pouze pro ètení)";
                btnAddTicket.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;

                cmbBooking.IsEnabled = false;
                cmbTicketType.IsEnabled = false;
                txtQuantity.IsReadOnly = true;
                txtPricePaid.IsReadOnly = true;

                lblStatus.Text = "Host: Editace zakázána.";
            }
        }

        // ... Zbytek metod beze zmìny ...
        private void LoadBookings() { cmbBooking.ItemsSource = _context.Bookings.ToList(); }
        private void LoadTicketTypes() { cmbTicketType.ItemsSource = _context.TicketTypes.ToList(); }
        private void LoadTickets() { dgTickets.ItemsSource = _context.BookingItems.Include(t => t.TicketType).ToList(); }
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
            if (App.CurrentUser == null) return;
            // ... Pùvodní logika ukládání ...
            try
            {
                if (_selectedItem == null) { _context.BookingItems.Add(new BookingItem { BookingId = (int)cmbBooking.SelectedValue, TicketTypeId = (int)cmbTicketType.SelectedValue, Quantity = int.Parse(txtQuantity.Text), UnitPrice = decimal.Parse(txtPricePaid.Text) }); }
                else { _selectedItem.Quantity = int.Parse(txtQuantity.Text); _selectedItem.UnitPrice = decimal.Parse(txtPricePaid.Text); }
                _context.SaveChanges(); LoadTickets();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e) { if (App.CurrentUser != null && _selectedItem != null) { _context.BookingItems.Remove(_selectedItem); _context.SaveChanges(); LoadTickets(); } }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }
        private void ClearForm() { cmbBooking.SelectedIndex = -1; cmbTicketType.SelectedIndex = -1; txtQuantity.Text = "1"; txtPricePaid.Clear(); _selectedItem = null; }
    }
}