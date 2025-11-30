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
            LoadTicketTypes();
            LoadTickets();
            ClearForm();
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
            catch { }
        }

        private void LoadTickets()
        {
            try
            {
                // Zde už voláme BookingItems, ne Tickets
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

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }

        private void DgTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedItem = dgTickets.SelectedItem as BookingItem;
            if (_selectedItem != null)
            {
                cmbTicketType.SelectedValue = _selectedItem.TicketTypeId;
                txtPricePaid.Text = _selectedItem.UnitPrice.ToString();
                // Ostatní pole ignorujeme (datumy atd.), protože v DB nejsou
            }
        }

        private void BtnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedItem = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbTicketType.SelectedValue == null) return;
                decimal.TryParse(txtPricePaid.Text, out decimal price);

                // Potøebujeme existující ID objednávky
                var defaultBooking = _context.Bookings.FirstOrDefault();
                int bookingId = defaultBooking?.BookingId ?? 0;

                if (bookingId == 0)
                {
                    MessageBox.Show("Create a Booking first!");
                    return;
                }

                if (_selectedItem == null)
                {
                    var newItem = new BookingItem
                    {
                        BookingId = bookingId,
                        TicketTypeId = (int)cmbTicketType.SelectedValue,
                        Quantity = 1,
                        UnitPrice = price
                    };
                    _context.BookingItems.Add(newItem);
                }
                else
                {
                    _selectedItem.TicketTypeId = (int)cmbTicketType.SelectedValue;
                    _selectedItem.UnitPrice = price;
                }

                _context.SaveChanges();
                LoadTickets();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            cmbTicketType.SelectedIndex = -1;
            txtPricePaid.Text = "";
            _selectedItem = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}