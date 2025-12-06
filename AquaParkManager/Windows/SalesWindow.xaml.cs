using System;
using System.Data;
using System.Linq;
using System.Windows;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client; // Nutné pro Oracle parametry

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
            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                // Volání DB procedury pro statistiky
                LoadVisitorStats(_selVis.VisitorId);
            }
        }

        private void LoadVisitorStats(int visitorId)
        {
            try
            {
                var pId = new OracleParameter("p_visitor_id", OracleDbType.Int32, visitorId, ParameterDirection.Input);
                var pSpent = new OracleParameter("o_total_spent", OracleDbType.Decimal, ParameterDirection.Output);
                var pCount = new OracleParameter("o_visit_count", OracleDbType.Int32, ParameterDirection.Output);
                var pLast = new OracleParameter("o_last_visit", OracleDbType.Date, ParameterDirection.Output);

                _ctx.Database.ExecuteSqlRaw("BEGIN SP_GET_VISITOR_STATS(:p_visitor_id, :o_total_spent, :o_visit_count, :o_last_visit); END;", pId, pSpent, pCount, pLast);

                string spent = pSpent.Value != DBNull.Value ? $"{pSpent.Value} EUR" : "0 EUR";
                string count = pCount.Value != DBNull.Value ? pCount.Value.ToString() : "0";

                txtVisStats.Text = $"Total Spent: {spent}\nVisits: {count}";
            }
            catch (Exception ex)
            {
                txtVisStats.Text = "No stats available";
                Console.WriteLine(ex.Message);
            }
        }

        private void BtnNewVisitor_Click(object sender, RoutedEventArgs e)
        {
            _selVis = null;
            txtVisFn.Text = ""; txtVisLn.Text = ""; txtVisEmail.Text = ""; txtVisPhone.Text = ""; txtVisAddr.Text = ""; dpVisBirth.SelectedDate = null;
            txtVisStats.Text = "New Record";
        }

        private void BtnSaveVisitor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVisFn.Text) || string.IsNullOrWhiteSpace(txtVisLn.Text))
            {
                MessageBox.Show("Name required!"); return;
            }

            try
            {
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
                    if (_selVis.Address == null) _selVis.Address = new Address { HouseNumber = "1", PostalCode = postalCode };
                    else _selVis.Address.PostalCode = postalCode;
                }

                _selVis.FirstName = txtVisFn.Text;
                _selVis.LastName = txtVisLn.Text;
                _selVis.Email = txtVisEmail.Text;
                _selVis.Phone = txtVisPhone.Text;
                _selVis.DateOfBirth = dpVisBirth.SelectedDate;

                _ctx.SaveChanges();
                MessageBox.Show("Visitor saved!");
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        private void DgBookings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { _selBook = dgBookings.SelectedItem as Booking; }

        private void BtnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBookVisitor.SelectedValue == null) { MessageBox.Show("Select Customer!"); return; }

            try
            {
                int visitorId = (int)cmbBookVisitor.SelectedValue;

                // Volání procedury SP_CREATE_BOOKING
                var pVisitorId = new OracleParameter("p_visitor_id", OracleDbType.Int32, visitorId, ParameterDirection.Input);
                var pNotes = new OracleParameter("p_notes", OracleDbType.Varchar2, "App Booking", ParameterDirection.Input);
                var pBookingId = new OracleParameter("p_booking_id", OracleDbType.Int32, ParameterDirection.Output);

                _ctx.Database.ExecuteSqlRaw("BEGIN SP_CREATE_BOOKING(:p_visitor_id, :p_notes, :p_booking_id); END;", pVisitorId, pNotes, pBookingId);

                MessageBox.Show("Booking Created (DB Proc)");
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        private void BtnCancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (_selBook == null) { MessageBox.Show("Select booking!"); return; }
            if (_selBook.Status == "CANCELLED") { MessageBox.Show("Already cancelled."); return; }

            try
            {
                // Volání procedury SP_CANCEL_BOOKING
                var pId = new OracleParameter("p_booking_id", OracleDbType.Int32, _selBook.BookingId, ParameterDirection.Input);
                var pReason = new OracleParameter("p_reason", OracleDbType.Varchar2, "User Request via App", ParameterDirection.Input);

                _ctx.Database.ExecuteSqlRaw("BEGIN SP_CANCEL_BOOKING(:p_booking_id, :p_reason); END;", pId, pReason);

                MessageBox.Show("Booking Cancelled");
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            if (_selBook == null || cmbTicketType.SelectedValue == null) return;
            if (!decimal.TryParse(txtPrice.Text, out decimal price)) return;

            try
            {
                _ctx.BookingItems.Add(new BookingItem
                {
                    BookingId = _selBook.BookingId,
                    TicketTypeId = (int)cmbTicketType.SelectedValue,
                    Quantity = 1,
                    UnitPrice = price
                });
                _ctx.SaveChanges();

                // Reload entity pro načtení ceny vypočítané triggerem v DB
                _ctx.Entry(_selBook).Reload();

                MessageBox.Show($"Ticket added. Total: {_selBook.TotalAmount:C}");
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            if (_selBook == null) return;
            if (!decimal.TryParse(txtPayAmount.Text, out decimal amount)) return;

            try
            {
                _ctx.Payments.Add(new Payment { BookingId = _selBook.BookingId, Amount = amount, PaymentMethod = "CASH" });
                _ctx.SaveChanges();
                MessageBox.Show("Payment Recorded");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddMem_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMemVisitor.SelectedValue == null) return;
            try
            {
                _ctx.Memberships.Add(new Membership { VisitorId = (int)cmbMemVisitor.SelectedValue, MembershipType = txtMemType.Text, StartDate = DateTime.Now });
                _ctx.SaveChanges(); RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnSetPoints_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoyVis.SelectedValue == null) return;
            if (!int.TryParse(txtPoints.Text, out int p)) return;
            try
            {
                _ctx.LoyaltyPoints.Add(new LoyaltyPoint { VisitorId = (int)cmbLoyVis.SelectedValue, Earned = p });
                _ctx.SaveChanges(); RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnUpdateTiers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ctx.Database.ExecuteSqlRaw("BEGIN SP_UPDATE_LOYALTY_TIERS; END;");
                MessageBox.Show("Loyalty Tiers Updated Successfully!", "Batch Job", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show($"Error running batch job: {ex.Message}"); }
        }

        private void BtnAddWaiver_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoyVis.SelectedValue == null) return;
            try
            {
                _ctx.Waivers.Add(new Waiver { VisitorId = (int)cmbLoyVis.SelectedValue, WaiverType = txtWaiver.Text, SignedDate = DateTime.Now });
                _ctx.SaveChanges();
                MessageBox.Show("Waiver Signed");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}