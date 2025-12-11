using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class ReportsAnalyticsWindow : Window
    {
        private AquaParkContext _context;

        public ReportsAnalyticsWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            
            dpRevenueFrom.SelectedDate = DateTime.Today.AddMonths(-1);
            dpRevenueTo.SelectedDate = DateTime.Today;
            
            LoadAreas();
            LoadOccupancyData();
            LoadMemberships();
            LoadLoyaltyPoints();
            LoadWeatherData();
        }

        private void LoadAreas()
        {
            try
            {
                var areas = _context.Pools.ToList();
                cmbOccupancyArea.ItemsSource = areas;
                cmbWeatherArea.ItemsSource = areas;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading areas: " + ex.Message);
                lblStatus.Text = $"Error loading areas: {ex.Message}";
            }
        }

        private void BtnGenerateRevenue_Click(object sender, RoutedEventArgs e)
        {
            if (dpRevenueFrom.SelectedDate == null || dpRevenueTo.SelectedDate == null)
            {
                MessageBox.Show("Please select date range.");
                lblStatus.Text = "Please select date range.";
                return;
            }

            try
            {
                var from = dpRevenueFrom.SelectedDate.Value;
                var to = dpRevenueTo.SelectedDate.Value.AddDays(1);

                var bookingsRevenue = _context.Bookings
                    .Where(b => b.CreatedDate >= from && b.CreatedDate < to && b.Status == "CONFIRMED")
                    .Sum(b => (decimal?)b.TotalAmount) ?? 0;

                var concessionRevenue = _context.ConcessionSales
                    .Where(cs => cs.SaleDate >= from && cs.SaleDate < to)
                    .Sum(cs => (decimal?)cs.Amount) ?? 0;

                txtBookingsRevenue.Text = bookingsRevenue.ToString("C");
                txtConcessionRevenue.Text = concessionRevenue.ToString("C");
                txtTotalRevenue.Text = (bookingsRevenue + concessionRevenue).ToString("C");

                // Daily breakdown
                var dailyRevenue = Enumerable.Range(0, (to - from).Days)
                    .Select(offset => from.AddDays(offset))
                    .Select(date => new RevenueViewModel
                    {
                        Date = date,
                        BookingsRevenue = _context.Bookings
                            .Where(b => b.CreatedDate.Date == date.Date && b.Status == "CONFIRMED")
                            .Sum(b => (decimal?)b.TotalAmount) ?? 0,
                        ConcessionsRevenue = _context.ConcessionSales
                            .Where(cs => cs.SaleDate.Date == date.Date)
                            .Sum(cs => (decimal?)cs.Amount) ?? 0
                    })
                    .ToList();

                dgRevenueDetails.ItemsSource = dailyRevenue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating revenue report: {ex.Message}");
                lblStatus.Text = $"Error generating revenue report: {ex.Message}";
            }
        }

        private void BtnRecordOccupancy_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOccupancyArea.SelectedValue == null || string.IsNullOrWhiteSpace(txtVisitorCount.Text))
            {
                MessageBox.Show("Please select area and enter visitor count.");
                lblStatus.Text = "Please select area and enter visitor count.";
                return;
            }

            try
            {
                var areaId = (int)cmbOccupancyArea.SelectedValue;
                var visitorCount = int.Parse(txtVisitorCount.Text);
                var area = _context.Pools.Find(areaId);
                var capacityPercentage = area != null && area.Capacity > 0 
                    ? (int)((visitorCount / (double)area.Capacity) * 100) 
                    : 0;

                var occupancy = new OccupancyTracking
                {
                    AreaId = areaId,
                    VisitorCount = visitorCount,
                    CapacityPercentage = capacityPercentage
                };

                _context.OccupancyTracking.Add(occupancy);
                _context.SaveChanges();

                MessageBox.Show("Occupancy recorded successfully!");
                lblStatus.Text = "Occupancy recorded successfully!";
                LoadOccupancyData();
                txtVisitorCount.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording occupancy: {ex.Message}");
                lblStatus.Text = $"Error recording occupancy: {ex.Message}";
            }
        }

        private void BtnRefreshOccupancy_Click(object sender, RoutedEventArgs e)
        {
            LoadOccupancyData();
        }

        private void LoadOccupancyData()
        {
            try
            {
                var today = DateTime.Today;
                var occupancy = _context.OccupancyTracking
                    .Include(o => o.Area)
                    .Where(o => o.RecordedTime >= today)
                    .OrderByDescending(o => o.RecordedTime)
                    .Select(o => new OccupancyViewModel
                    {
                        OccupancyId = o.OccupancyId,
                        Area = o.Area,
                        RecordedTime = o.RecordedTime,
                        VisitorCount = o.VisitorCount,
                        CapacityPercentage = o.CapacityPercentage ?? 0
                    })
                    .ToList();

                dgOccupancy.ItemsSource = occupancy;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading occupancy data: {ex.Message}");
                lblStatus.Text = $"Error loading occupancy data: {ex.Message}";
            }
        }

        private void LoadMemberships()
        {
            try
            {
                var memberships = _context.Memberships
                    .Include(m => m.Visitor)
                    .Where(m => m.Status == "ACTIVE")
                    .ToList();

                dgMemberships.ItemsSource = memberships;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading memberships: {ex.Message}");
                lblStatus.Text = $"Error loading memberships: {ex.Message}";
            }
        }

        private void LoadLoyaltyPoints()
        {
            try
            {
                var loyaltyPoints = _context.LoyaltyPoints
                    .Include(lp => lp.Visitor)
                    .Select(lp => new LoyaltyViewModel
                    {
                        PointsId = lp.PointsId,
                        Visitor = lp.Visitor,
                        PointsEarned = lp.PointsEarned,
                        PointsRedeemed = lp.PointsRedeemed,
                        AvailablePoints = lp.PointsEarned - lp.PointsRedeemed,
                        Tier = lp.Tier
                    })
                    .OrderByDescending(lp => lp.AvailablePoints)
                    .ToList();

                dgLoyaltyPoints.ItemsSource = loyaltyPoints;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loyalty points: {ex.Message}");
                lblStatus.Text = $"Error loading loyalty points: {ex.Message}";
            }
        }

        private void BtnRecordWeather_Click(object sender, RoutedEventArgs e)
        {
            if (cmbWeatherArea.SelectedValue == null)
            {
                MessageBox.Show("Please select an area.");
                lblStatus.Text = "Please select an area.";
                return;
            }

            try
            {
                var weather = new WeatherCondition
                {
                    AreaId = (int?)cmbWeatherArea.SelectedValue,
                    Temperature = string.IsNullOrWhiteSpace(txtTemperature.Text) ? null : decimal.Parse(txtTemperature.Text),
                    WeatherType = (cmbWeatherType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    OperationalStatus = (cmbOperationalStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "NORMAL"
                };

                _context.WeatherConditions.Add(weather);
                _context.SaveChanges();

                MessageBox.Show("Weather condition recorded successfully!");
                lblStatus.Text = "Weather condition recorded successfully!";
                LoadWeatherData();
                txtTemperature.Clear();
                cmbWeatherType.SelectedIndex = -1;
                cmbOperationalStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording weather: {ex.Message}");
                lblStatus.Text = $"Error recording weather: {ex.Message}";
            }
        }

        private void LoadWeatherData()
        {
            try
            {
                var today = DateTime.Today;
                var weather = _context.WeatherConditions
                    .Include(w => w.Area)
                    .Where(w => w.RecordDate >= today.AddDays(-7))
                    .OrderByDescending(w => w.RecordDate)
                    .ToList();

                dgWeather.ItemsSource = weather;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading weather data: {ex.Message}");
                lblStatus.Text = $"Error loading weather data: {ex.Message}";
            }
        }
    }

    public class RevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal BookingsRevenue { get; set; }
        public decimal ConcessionsRevenue { get; set; }
        public decimal TotalRevenue => BookingsRevenue + ConcessionsRevenue;
    }

    public class OccupancyViewModel
    {
        public int OccupancyId { get; set; }
        public Pool? Area { get; set; }
        public DateTime RecordedTime { get; set; }
        public int VisitorCount { get; set; }
        public int CapacityPercentage { get; set; }

        public string StatusText => CapacityPercentage >= 90 ? "⚠ FULL" : CapacityPercentage >= 70 ? "BUSY" : "OK";
        public Brush StatusColor => CapacityPercentage >= 90 ? Brushes.Red : CapacityPercentage >= 70 ? Brushes.Orange : Brushes.Green;
    }

    public class LoyaltyViewModel
    {
        public int PointsId { get; set; }
        public Visitor? Visitor { get; set; }
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public int AvailablePoints { get; set; }
        public string Tier { get; set; } = string.Empty;
    }
}
