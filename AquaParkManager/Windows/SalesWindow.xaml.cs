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
            // Načítáme i adresu pro zobrazení
            var visitors = _ctx.Visitors.Include(v => v.Address).ThenInclude(a => a.PostalCode).ToList();
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
            if (_selVis != null)
            {
                txtVisFn.Text = _selVis.FirstName;
                txtVisLn.Text = _selVis.LastName;
                txtVisEmail.Text = _selVis.Email;
                txtVisPhone.Text = _selVis.Phone;
                dpVisBirth.SelectedDate = _selVis.DateOfBirth;
                txtVisAddr.Text = _selVis.Address?.PostalCode?.City ?? "";
            }
        }

        private void BtnNewVisitor_Click(object sender, RoutedEventArgs e)
        {
            _selVis = null;
            txtVisFn.Text = ""; txtVisLn.Text = ""; txtVisEmail.Text = ""; txtVisPhone.Text = ""; txtVisAddr.Text = ""; dpVisBirth.SelectedDate = null;
        }

        private void BtnSaveVisitor_Click(object sender, RoutedEventArgs e)
        {
            // Jednoduchá logika pro přiřazení PSČ podle města
            string city = string.IsNullOrWhiteSpace(txtVisAddr.Text) ? "Unknown" : txtVisAddr.Text;
            var postalCode = _ctx.PostalCodes.FirstOrDefault(p => p.City == city)
                             ?? new PostalCode { Code = "00000", City = city, Country = "CR", Region = "Region" };

            if (postalCode.PostalCodeId == 0) _ctx.PostalCodes.Add(postalCode);

            if (_selVis == null)
            {
                var addr = new Address { HouseNumber = "1", PostalCode = postalCode };
                _selVis = new Visitor { Address = addr };
                _ctx.Visitors.Add(_selVis);
            }
            else
            {
                if (_selVis.Address == null)
                    _selVis.Address = new Address { HouseNumber = "1", PostalCode = postalCode };
                else
                    _selVis.Address.PostalCode = postalCode;
            }

            _selVis.FirstName = txtVisFn.Text;
            _selVis.LastName = txtVisLn.Text;
            _selVis.Email = txtVisEmail.Text;
            _selVis.Phone = txtVisPhone.Text;
            _selVis.DateOfBirth = dpVisBirth.SelectedDate;

            _ctx.SaveChanges();
            RefreshAll();
        }

        private void DgBookings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { _selBook = dgBookings.SelectedItem as Booking; }

        private void BtnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBookVisitor.SelectedValue == null) return;

            // Generování referenčního čísla podobně jako v DB proceduře
            string bookingRef = $"BK-{DateTime.Now.Year}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            _ctx.Bookings.Add(new Booking
            {
                VisitorId = (int)cmbBookVisitor.SelectedValue,
                BookingRef = bookingRef,
                TotalAmount = 0,
                CreatedDate = DateTime.Now
            });
            _ctx.SaveChanges();
            RefreshAll();
        }

        private void BtnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            if (_selBook == null || cmbTicketType.SelectedValue == null) return;
            decimal.TryParse(txtPrice.Text, out decimal p);

            _ctx.BookingItems.Add(new BookingItem
            {
                BookingId = _selBook.BookingId,
                TicketTypeId = (int)cmbTicketType.SelectedValue,
                Quantity = 1,
                UnitPrice = p
            });

            _ctx.SaveChanges();

            // Reload pro aktualizaci celkové ceny (pokud by to řešil trigger nebo aplikace)
            _ctx.Entry(_selBook).Reload();
            RefreshAll();
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