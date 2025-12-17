using AquaParkManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class MembershipWindow : Window
    {
        private Dictionary<string, decimal> membershipPrices = new Dictionary<string, decimal>();
        private string selectedMembershipType = null;

        public MembershipWindow()
        {
            InitializeComponent();
            LoadMembershipTypes();
            sliderMonths.ValueChanged += SliderMonths_ValueChanged;
        }

        private void LoadMembershipTypes()
        {
            try
            {
                // Hardcoded membership types with prices
                membershipPrices["BASIC"] = 500;      // 500 Kč per month
                membershipPrices["PREMIUM"] = 800;    // 800 Kč per month
                membershipPrices["VIP"] = 1200;       // 1200 Kč per month

                var memberships = new List<dynamic>
                {
                    new { Type = "BASIC", Name = "Základní", Description = "Přístup k bazénům a tobogánům", Price = "500 Kč/měsíc" },
                    new { Type = "PREMIUM", Name = "Premium", Description = "Bazény, tobogány + 10% sleva na vstupenky", Price = "800 Kč/měsíc" },
                    new { Type = "VIP", Name = "VIP", Description = "Vše + priority queue + gratis parkování", Price = "1200 Kč/měsíc" }
                };

                icMemberships.ItemsSource = memberships;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání typů členství: {ex.Message}", "Chyba");
            }
        }

        private void RbMembership_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is RadioButton rb && rb.Tag != null)
                {
                    // Get the Type property from the Tag object
                    var tagType = rb.Tag.GetType();
                    var typeProp = tagType.GetProperty("Type");
                    if (typeProp != null)
                    {
                        selectedMembershipType = typeProp.GetValue(rb.Tag) as string;
                        btnPurchase.IsEnabled = true;
                        UpdateSummary();
                    }
                }
            }
            catch { }
        }

        private void Border_PreviewMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // When clicking anywhere on the border, select the corresponding radio button
            if (sender is Border border && border.Child is Grid grid)
            {
                var radioButton = FindRadioButton(grid);
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }

        private System.Windows.Controls.RadioButton FindRadioButton(DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is System.Windows.Controls.RadioButton rb)
                {
                    return rb;
                }
                var result = FindRadioButton(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void SliderMonths_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int months = (int)sliderMonths.Value;
            lblMonthsDisplay.Text = months == 1 ? "1 měsíc" : $"{months} měsíců";
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            try
            {
                if (selectedMembershipType == null)
                {
                    lblMembershipSelected.Text = "Typu: Nebylo vybráno";
                    lblTotalPrice.Text = "Cena celkem: 0 Kč";
                    lblBigPrice.Text = "0 Kč";
                    return;
                }

                int months = (int)sliderMonths.Value;
                decimal monthlyPrice = membershipPrices[selectedMembershipType];
                decimal totalPrice = monthlyPrice * months;

                // Get membership name - hardcode names
                string membershipName = selectedMembershipType switch
                {
                    "BASIC" => "Základní",
                    "PREMIUM" => "Premium",
                    "VIP" => "VIP",
                    _ => selectedMembershipType
                };

                lblMembershipSelected.Text = $"Typu: {membershipName}";
                lblDurationSelected.Text = months == 1 ? "Doba: 1 měsíc" : $"Doba: {months} měsíců";
                lblTotalPrice.Text = $"Cena celkem: {totalPrice:F0} Kč";
                lblBigPrice.Text = $"{totalPrice:F0} Kč";
            }
            catch { }
        }

        private void BtnPurchase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMembershipType == null)
                {
                    MessageBox.Show("Vyberte prosím typ členství.", "Upozornění");
                    return;
                }

                int months = (int)sliderMonths.Value;
                decimal price = membershipPrices[selectedMembershipType];

                using (var ctx = new AquaParkContext())
                {
                    // Get or create visitor for current user
                    Visitor visitor = null;
                    
                    if (App.CurrentUser != null)
                    {
                        visitor = ctx.Visitors
                            .FirstOrDefault(v => v.FirstName == App.CurrentUser.Username);

                        if (visitor == null)
                        {
                            // Create visitor
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
                                LastName = "Member",
                                Email = App.CurrentUser.Email ?? "member@example.com",
                                AddressId = address.AddressId
                            };
                            ctx.Visitors.Add(visitor);
                            ctx.SaveChanges();
                        }
                    }

                    if (visitor == null)
                    {
                        MessageBox.Show("Nelze vytvořit členství bez visitor informací.", "Chyba");
                        return;
                    }

                    // Create membership
                    var membership = new Membership
                    {
                        VisitorId = visitor.VisitorId,
                        MembershipType = selectedMembershipType,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(months),
                        Recurring = "N",
                        Status = "ACTIVE"
                    };

                    ctx.Memberships.Add(membership);
                    ctx.SaveChanges();

                    MessageBox.Show(
                        $"✅ Členství úspěšně zakoupeno!\n\n" +
                        $"Typ: {selectedMembershipType}\n" +
                        $"Platnost: {months} měsíc(ů)\n" +
                        $"Cena: {price * months:F0} Kč\n\n" +
                        $"Platné od: {membership.StartDate:dd.MM.yyyy}\n" +
                        $"Platné do: {membership.EndDate:dd.MM.yyyy}",
                        "Nákup Úspěšný");

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při nákupu členství: {ex.Message}", "Chyba");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
