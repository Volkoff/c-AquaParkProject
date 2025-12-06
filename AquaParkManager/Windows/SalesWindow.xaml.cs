using System;
using System.Linq;
using System.Windows;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class SalesWindow : Window
    {
        private AquaParkContext _ctx = new AquaParkContext();
        private Visitor? _selVis;
        private Booking? _selBook;

        public SalesWindow()
        {
            InitializeComponent();
            RefreshAll();
        }

        private void RefreshAll()
        {
            var visitors = _ctx.Visitors.ToList();
            dgVisitors.ItemsSource = visitors;
            cmbBookVisitor.ItemsSource = visitors;
            cmbMemVisitor.ItemsSource = visitors;
            cmbLoyVis.ItemsSource = visitors;

            dgBookings.ItemsSource = _ctx.Bookings.Include(b => b.Visitor).OrderByDescending(b => b.BookingId).ToList();
            cmbTicketType.ItemsSource = _ctx.TicketTypes.ToList();

            dgMemberships.ItemsSource = _ctx.Memberships.Include(m => m.Visitor).ToList();
            dgLoyalty.ItemsSource = _ctx.LoyaltyPoints.Include(l => l.Visitor).ToList();
        }

        private void DgVisitors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selVis = dgVisitors.SelectedItem as Visitor;
            if (_selVis != null) { txtVisFn.Text = _selVis.FirstName; txtVisLn.Text = _selVis.LastName; }
        }
        private void BtnNewVisitor_Click(object sender, RoutedEventArgs e) { _selVis = null; txtVisFn.Text = ""; }
        private void BtnSaveVisitor_Click(object sender, RoutedEventArgs e)
        {
            if (_selVis == null)
            {
                var addr = new Address { HouseNumber = "1", PostalCode = _ctx.PostalCodes.FirstOrDefault() ?? new PostalCode { Code = "000", City = "X", Country = "X", Region = "X" } };
                if (addr.PostalCodeId == 0) _ctx.PostalCodes.Add(addr.PostalCode);
                _selVis = new Visitor { Address = addr }; _ctx.Visitors.Add(_selVis);
            }
            _selVis.FirstName = txtVisFn.Text; _selVis.LastName = txtVisLn.Text;
            _ctx.SaveChanges(); RefreshAll();
        }

        private void DgBookings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { _selBook = dgBookings.SelectedItem as Booking; }
        private void BtnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBookVisitor.SelectedValue == null) return;
            _ctx.Bookings.Add(new Booking { VisitorId = (int)cmbBookVisitor.SelectedValue, BookingRef = "BK-" + DateTime.Now.Ticks, TotalAmount = 0 });
            _ctx.SaveChanges(); RefreshAll();
        }
        private void BtnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            if (_selBook == null || cmbTicketType.SelectedValue == null) return;
            decimal.TryParse(txtPrice.Text, out decimal p);
            _ctx.BookingItems.Add(new BookingItem { BookingId = _selBook.BookingId, TicketTypeId = (int)cmbTicketType.SelectedValue, Quantity = 1, UnitPrice = p });
            _ctx.SaveChanges(); _ctx.Entry(_selBook).Reload(); RefreshAll();
        }
        private void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            if (_selBook == null) return;
            decimal.TryParse(txtPayAmount.Text, out decimal a);
            _ctx.Payments.Add(new Payment { BookingId = _selBook.BookingId, Amount = a, PaymentMethod = "CASH" });
            _ctx.SaveChanges(); MessageBox.Show("Payment Recorded");
        }

        private void BtnAddMem_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMemVisitor.SelectedValue == null) return;
            _ctx.Memberships.Add(new Membership { VisitorId = (int)cmbMemVisitor.SelectedValue, MembershipType = txtMemType.Text, StartDate = DateTime.Now });
            _ctx.SaveChanges(); RefreshAll();
        }
        private void BtnSetPoints_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoyVis.SelectedValue == null) return;
            int.TryParse(txtPoints.Text, out int p);
            _ctx.LoyaltyPoints.Add(new LoyaltyPoint { VisitorId = (int)cmbLoyVis.SelectedValue, Earned = p });
            _ctx.SaveChanges(); RefreshAll();
        }
        private void BtnAddWaiver_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoyVis.SelectedValue == null) return;
            _ctx.Waivers.Add(new Waiver { VisitorId = (int)cmbLoyVis.SelectedValue, WaiverType = txtWaiver.Text, SignedDate = DateTime.Now });
            _ctx.SaveChanges(); MessageBox.Show("Waiver Signed");
        }
    }
}