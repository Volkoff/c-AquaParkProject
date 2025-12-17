using AquaParkManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class GuestBookingWindow : Window
    {
        private List<TicketType> ticketTypes = new List<TicketType>();
        private Dictionary<int, int> quantities = new Dictionary<int, int>(); // TicketTypeId -> Quantity
        private Dictionary<int, decimal> prices = new Dictionary<int, decimal>(); // TicketTypeId -> Current Price

        public GuestBookingWindow()
        {
            try{
                InitializeComponent();
                LoadTicketTypes();
                SetDefaultDate();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Chyba při inicializaci okna rezervace: {ex.Message}", "Chyba aplikace");
                this.Close();
            }
            
        }

        private void SetDefaultDate()
        {
            dpReservationDate.SelectedDate = DateTime.Now;
            txtReservationTime.Text = DateTime.Now.AddHours(1).ToString("HH:mm");
        }

        private void LoadTicketTypes()
        {
            try
            {
                using (var ctx = new AquaParkContext())
                {
                    ticketTypes = ctx.TicketTypes.ToList();
                    
                    // Initialize quantities and prices
                    foreach (var ticketType in ticketTypes)
                    {
                        quantities[ticketType.TicketTypeId] = 0;
                        
                        // Get current price from PriceListItems
                        var priceItem = ctx.PriceListItems
                            .Where(p => p.TicketTypeId == ticketType.TicketTypeId &&
                                       p.ValidFrom <= DateTime.Now &&
                                       (p.ValidTo == null || p.ValidTo >= DateTime.Now))
                            .FirstOrDefault();
                        
                        prices[ticketType.TicketTypeId] = priceItem?.UnitPrice ?? 0;
                    }

                    icTickets.ItemsSource = ticketTypes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání vstupenek: {ex.Message}", "Chyba");
            }
        }

        private void TxtQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow digits
            e.Handled = !int.TryParse(e.Text, out _);
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            try
            {
                int totalTickets = 0;
                decimal totalPrice = 0;

                // Get all ItemsControl children and read their quantities
                for (int i = 0; i < icTickets.Items.Count; i++)
                {
                    var container = icTickets.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (container != null)
                    {
                        var txtQuantity = FindChild<TextBox>(container, "txtQuantity");
                        var lblLinePrice = FindChild<TextBlock>(container, "lblLinePrice");
                        
                        if (txtQuantity != null && int.TryParse(txtQuantity.Text, out int qty))
                        {
                            var ticketType = ticketTypes[i];
                            quantities[ticketType.TicketTypeId] = qty;
                            
                            decimal itemPrice = prices[ticketType.TicketTypeId] * qty;
                            totalTickets += qty;
                            totalPrice += itemPrice;

                            if (lblLinePrice != null)
                            {
                                lblLinePrice.Text = $"{itemPrice:F0} Kč";
                            }
                        }
                    }
                }

                lblTicketCount.Text = $"Počet vstupenek: {totalTickets}";
                lblTotalPrice.Text = $"Cena celkem: {totalPrice:F0} Kč";
                lblBigPrice.Text = $"{totalPrice:F0} Kč";

                // Update price display for each ticket
                foreach (var item in icTickets.Items)
                {
                    if (item is TicketType ticketType)
                    {
                        var container = icTickets.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                        if (container != null)
                        {
                            var lblPrice = FindChild<TextBlock>(container, "lblPrice");
                            if (lblPrice != null)
                            {
                                lblPrice.Text = $"Cena: {prices[ticketType.TicketTypeId]:F0} Kč";
                            }
                        }
                    }
                }
            }
            catch { /* Ignore */ }
        }

        private T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    if (string.IsNullOrEmpty(childName))
                        return typedChild;

                    if (child is FrameworkElement fe && fe.Name == childName)
                        return typedChild;
                }

                var result = FindChild<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void BtnPurchase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear previous errors
                lblDateError.Text = "";
                lblTimeError.Text = "";

                // Validate date
                if (!dpReservationDate.SelectedDate.HasValue)
                {
                    lblDateError.Text = "Vyberte prosím datum.";
                    return;
                }

                // Validate and parse time
                string timeInput = txtReservationTime.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(timeInput))
                {
                    lblTimeError.Text = "Zadejte prosím čas.";
                    return;
                }

                TimeSpan reservationTime;
                // Try to parse time in HH:mm format
                // TimeSpan needs format like "HH:mm:ss", so if user enters just "HH:mm", add seconds
                if (!timeInput.Contains(":"))
                {
                    lblTimeError.Text = "Neplatný formát. Použijte HH:mm (např. 14:30)";
                    return;
                }

                // Add seconds if not provided
                var timeParts = timeInput.Split(':');
                if (timeParts.Length == 2)
                {
                    timeInput = $"{timeParts[0]}:{timeParts[1]}:00";
                }
                else if (timeParts.Length != 3)
                {
                    lblTimeError.Text = "Neplatný formát. Použijte HH:mm (např. 14:30)";
                    return;
                }

                if (!TimeSpan.TryParse(timeInput, out reservationTime))
                {
                    lblTimeError.Text = "Neplatný formát. Použijte HH:mm (např. 14:30)";
                    return;
                }

                // Validate time is reasonable (0:00 to 23:59)
                if (reservationTime.TotalHours >= 24 || reservationTime.TotalHours < 0)
                {
                    lblTimeError.Text = "Čas musí být mezi 00:00 a 23:59";
                    return;
                }

                // Check if any tickets selected
                if (quantities.Values.Sum() == 0)
                {
                    MessageBox.Show("Vyberte prosím alespoň jednu vstupenku.", "Upozornění");
                    return;
                }

                // Check if user has active membership (if logged in)
                if (App.CurrentUser != null)
                {
                    using (var ctx = new AquaParkContext())
                    {
                        var visitor = ctx.Visitors.FirstOrDefault(v => v.FirstName == App.CurrentUser.Username);
                        if (visitor != null)
                        {
                            // Check for active membership - Status == ACTIVE and EndDate is null or in future
                            var activeMembership = ctx.Memberships
                                .Where(m => m.VisitorId == visitor.VisitorId && 
                                           m.Status == "ACTIVE" && 
                                           (m.EndDate == null || m.EndDate > DateTime.Now))
                                .FirstOrDefault();

                            if (activeMembership == null)
                            {
                                var result = MessageBox.Show(
                                    "Nemáte aktivní členství. Chcete si ho koupit nyní?\n\nBez členství budete mít zvýšené ceny vstupenek.",
                                    "Varování - Není členství",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (result == MessageBoxResult.Yes)
                                {
                                    new MembershipWindow().ShowDialog();
                                    return;
                                }
                            }
                        }
                    }
                }

                using (var ctx = new AquaParkContext())
                {
                    // For guests, we need to get or create a Visitor record
                    // Guests have CurrentUser but it might not be linked to a Visitor
                    Visitor? visitor = null;
                    
                    if (App.CurrentUser != null)
                    {
                        // Try to find existing visitor for this user
                        visitor = ctx.Visitors
                            .Include(v => v.Address)
                            .FirstOrDefault(v => v.FirstName == App.CurrentUser.Username);
                    }
                    
                    if (visitor == null && App.CurrentUser != null)
                    {
                        // Get or create a default postal code
                        var postalCode = ctx.PostalCodes.FirstOrDefault();
                        if (postalCode == null)
                        {
                            postalCode = new PostalCode
                            {
                                City = "Unknown",
                                Code = "00000",
                                Country = "CZ",
                                Region = "Unknown"
                            };
                            ctx.PostalCodes.Add(postalCode);
                            ctx.SaveChanges();
                        }

                        // Create a visitor record for this guest
                        var address = new Address
                        {
                            Street = "Unknown",
                            HouseNumber = "0",
                            PostalCodeId = postalCode.PostalCodeId
                        };
                        ctx.Address.Add(address);
                        ctx.SaveChanges();

                        visitor = new Visitor
                        {
                            FirstName = App.CurrentUser.Username,
                            LastName = "Guest",
                            Email = App.CurrentUser.Email ?? "guest@example.com",
                            AddressId = address.AddressId
                        };
                        ctx.Visitors.Add(visitor);
                        ctx.SaveChanges();
                    }

                    decimal totalAmount = 0;
                    var booking = new Booking
                    {
                        VisitorId = visitor?.VisitorId ?? 1, // Fallback to default visitor if none found
                        Status = "CONFIRMED",
                        Notes = $"Rezervace na {dpReservationDate.SelectedDate:dd.MM.yyyy} v {reservationTime:hh\\:mm}"
                    };

                    ctx.Bookings.Add(booking);
                    ctx.SaveChanges();

                    // Create BookingItems (tickets) for each selected ticket type
                    foreach (var qty in quantities.Where(q => q.Value > 0))
                    {
                        var ticketTypeId = qty.Key;
                        var quantity = qty.Value;
                        var price = prices[ticketTypeId];

                        var bookingItem = new BookingItem
                        {
                            BookingId = booking.BookingId,
                            TicketTypeId = ticketTypeId,
                            Quantity = quantity,
                            UnitPrice = price
                        };

                        ctx.BookingItems.Add(bookingItem);
                        totalAmount += price * quantity;
                    }

                    booking.TotalAmount = totalAmount;
                    ctx.SaveChanges();

                    MessageBox.Show(
                        $"✅ Rezervace úspěšně vytvořena!\n\n" +
                        $"Datum: {dpReservationDate.SelectedDate:dd.MM.yyyy} {reservationTime:HH\\:mm}\n" +
                        $"Počet vstupenek: {quantities.Values.Sum()}\n" +
                        $"Cena: {totalAmount:F0} Kč",
                        "Nákup Úspěšný");

                    this.Close();
                }
            }
            catch (FormatException fex)
            {
                MessageBox.Show($"Chyba formátu: {fex.Message}\n\nProsím zkontrolujte, že jste zadali čas ve správném formátu (HH:mm).", "Chyba formátu");
                System.Diagnostics.Debug.WriteLine($"Format Error: {fex}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při vytváření rezervace: {ex.Message}\n\n{ex.InnerException?.Message}", "Chyba");
                System.Diagnostics.Debug.WriteLine($"Purchase Error: {ex}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
