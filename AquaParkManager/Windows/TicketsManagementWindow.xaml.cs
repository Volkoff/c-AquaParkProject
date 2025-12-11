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
        }

        private void LoadBookings()
        {
            try
            {
                var bookings = _context.Bookings.OrderByDescending(b => b.BookingId).ToList();
                cmbBooking.ItemsSource = bookings;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error loading bookings: {ex.Message}";
            }
        }

        private void LoadTicketTypes()
        {
            try
            {
                var types = _context.TicketTypes.ToList();
                cmbTicketType.ItemsSource = types;
                cmbTicketType.DisplayMemberPath = "Name";
                cmbTicketType.SelectedValuePath = "TicketTypeId";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error loading ticket types: {ex.Message}";
            }
        }

        private void LoadTickets()
        {
            try
            {
                var items = _context.BookingItems
                    .Include(t => t.TicketType)
                    .ToList();
                dgTickets.ItemsSource = items;
                lblStatus.Text = $"Loaded {items.Count} items";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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
                lblStatus.Text = "Editing selected ticket item";
            }
        }

        private void BtnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedItem = null;
            lblStatus.Text = "Ready to add new ticket";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbTicketType.SelectedValue == null) return;
                if (cmbBooking.SelectedValue == null)
                {
                    MessageBox.Show("Please select a Booking ID first!");
                    lblStatus.Text = "Please select a Booking ID first!";
                    return;
                }

                decimal.TryParse(txtPricePaid.Text, out decimal price);
                int.TryParse(txtQuantity.Text, out int qty);
                if (qty < 1) qty = 1;

                if (_selectedItem == null)
                {
                    var newItem = new BookingItem
                    {
                        BookingId = (int)cmbBooking.SelectedValue,
                        TicketTypeId = (int)cmbTicketType.SelectedValue,
                        Quantity = qty,
                        UnitPrice = price
                    };
                    _context.BookingItems.Add(newItem);
                }
                else
                {
                    _selectedItem.BookingId = (int)cmbBooking.SelectedValue;
                    _selectedItem.TicketTypeId = (int)cmbTicketType.SelectedValue;
                    _selectedItem.Quantity = qty;
                    _selectedItem.UnitPrice = price;
                }

                _context.SaveChanges();
                LoadTickets();
                ClearForm();
                lblStatus.Text = "Ticket saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                lblStatus.Text = $"Error saving ticket: {ex.Message}";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem != null)
            {
                _context.BookingItems.Remove(_selectedItem);
                _context.SaveChanges();
                LoadTickets();
                ClearForm();
                lblStatus.Text = "Ticket deleted successfully";
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            cmbBooking.SelectedIndex = -1;
            cmbTicketType.SelectedIndex = -1;
            txtQuantity.Text = "1";
            txtPricePaid.Text = "";
            _selectedItem = null;
            lblStatus.Text = "Form cleared";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}