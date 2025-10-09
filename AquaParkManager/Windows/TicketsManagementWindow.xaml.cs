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
        private Ticket? _selectedTicket;

        public TicketsManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadTicketTypes();
            LoadVisitors();
            LoadTickets();
            ClearForm();
        }

        private void LoadTicketTypes()
        {
            try
            {
                var ticketTypes = _context.TicketTypes.ToList();
                cmbTicketType.ItemsSource = ticketTypes;
                cmbTicketType.DisplayMemberPath = "Name";
                cmbTicketType.SelectedValuePath = "TicketTypeId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ticket types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadVisitors()
        {
            try
            {
                var visitors = _context.Visitors.ToList();
                cmbVisitor.ItemsSource = visitors;
                cmbVisitor.DisplayMemberPath = "FirstName";
                cmbVisitor.SelectedValuePath = "VisitorId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading visitors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTickets()
        {
            try
            {
                var tickets = _context.Tickets
                    .Include(t => t.TicketType)
                    .Include(t => t.Visitor)
                    .ToList();
                dgTickets.ItemsSource = tickets;
                lblStatus.Text = $"Loaded {tickets.Count} tickets";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tickets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading tickets";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadTickets();
                return;
            }

            try
            {
                var filteredTickets = _context.Tickets
                    .Include(t => t.TicketType)
                    .Include(t => t.Visitor)
                    .Where(t => t.TicketType.Name.ToLower().Contains(searchText) ||
                               (t.Visitor != null && t.Visitor.FirstName.ToLower().Contains(searchText)) ||
                               t.Status.ToLower().Contains(searchText))
                    .ToList();
                
                dgTickets.ItemsSource = filteredTickets;
                lblStatus.Text = $"Found {filteredTickets.Count} tickets";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching tickets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTicket = dgTickets.SelectedItem as Ticket;
            if (_selectedTicket != null)
            {
                LoadTicketDetails(_selectedTicket);
            }
        }

        private void LoadTicketDetails(Ticket ticket)
        {
            cmbTicketType.SelectedValue = ticket.TicketTypeId;
            cmbVisitor.SelectedValue = ticket.VisitorId;
            dpPurchaseDate.SelectedDate = ticket.PurchaseDate;
            dpValidFrom.SelectedDate = ticket.ValidFrom;
            dpValidTo.SelectedDate = ticket.ValidTo;
            txtPricePaid.Text = ticket.PricePaid.ToString();
            cmbStatus.Text = ticket.Status;
        }

        private void BtnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedTicket = null;
            cmbTicketType.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbTicketType.SelectedValue == null)
                {
                    MessageBox.Show("Please select a ticket type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPricePaid.Text, out decimal pricePaid) || pricePaid < 0)
                {
                    MessageBox.Show("Please enter a valid price (>= 0).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedTicket == null)
                {
                    // Add new ticket
                    var newTicket = new Ticket
                    {
                        TicketTypeId = (int)cmbTicketType.SelectedValue,
                        VisitorId = cmbVisitor.SelectedValue as int?,
                        PurchaseDate = dpPurchaseDate.SelectedDate ?? DateTime.Now,
                        ValidFrom = dpValidFrom.SelectedDate,
                        ValidTo = dpValidTo.SelectedDate,
                        PricePaid = pricePaid,
                        Status = cmbStatus.Text
                    };

                    _context.Tickets.Add(newTicket);
                    _context.SaveChanges();
                    lblStatus.Text = "Ticket added successfully";
                }
                else
                {
                    // Update existing ticket
                    _selectedTicket.TicketTypeId = (int)cmbTicketType.SelectedValue;
                    _selectedTicket.VisitorId = cmbVisitor.SelectedValue as int?;
                    _selectedTicket.PurchaseDate = dpPurchaseDate.SelectedDate ?? _selectedTicket.PurchaseDate;
                    _selectedTicket.ValidFrom = dpValidFrom.SelectedDate;
                    _selectedTicket.ValidTo = dpValidTo.SelectedDate;
                    _selectedTicket.PricePaid = pricePaid;
                    _selectedTicket.Status = cmbStatus.Text;

                    _context.SaveChanges();
                    lblStatus.Text = "Ticket updated successfully";
                }

                LoadTickets();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving ticket";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTicket == null)
            {
                MessageBox.Show("Please select a ticket to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete this ticket?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Tickets.Remove(_selectedTicket);
                    _context.SaveChanges();
                    LoadTickets();
                    ClearForm();
                    lblStatus.Text = "Ticket deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error deleting ticket";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedTicket = null;
            dgTickets.SelectedItem = null;
        }

        private void ClearForm()
        {
            cmbTicketType.SelectedIndex = -1;
            cmbVisitor.SelectedIndex = -1;
            dpPurchaseDate.SelectedDate = DateTime.Now;
            dpValidFrom.SelectedDate = null;
            dpValidTo.SelectedDate = null;
            txtPricePaid.Text = "";
            cmbStatus.SelectedIndex = 0;
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
