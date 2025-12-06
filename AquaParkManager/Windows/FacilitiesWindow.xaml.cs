using System;
using System.Linq;
using System.Windows;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class FacilitiesWindow : Window
    {
        private AquaParkContext _ctx = new AquaParkContext();
        private Attraction? _selAttr;

        public FacilitiesWindow()
        {
            InitializeComponent();
            RefreshAll();
        }

        private void RefreshAll()
        {
            dgAttractions.ItemsSource = _ctx.Attractions.Include(a => a.SlideType).Include(a => a.Pool).ToList();
            cmbAttrPool.ItemsSource = _ctx.Pools.ToList();
            cmbAttrType.ItemsSource = _ctx.SlideTypes.ToList();

            dgMaint.ItemsSource = _ctx.MaintenanceRecords.Include(m => m.Staff).OrderByDescending(m => m.ReportDate).ToList();
            cmbMaintStaff.ItemsSource = _ctx.Staff.ToList();

            dgInv.ItemsSource = _ctx.Inventory.Include(i => i.Supplier).ToList();
            cmbInvSup.ItemsSource = _ctx.Suppliers.ToList();

            dgConcessions.ItemsSource = _ctx.Concessions.ToList();
            cmbConcArea.ItemsSource = _ctx.Pools.ToList();

            dgSafety.ItemsSource = _ctx.SafetyStandards.ToList();
            dgWeather.ItemsSource = _ctx.WeatherConditions.OrderByDescending(w => w.RecordDate).ToList();
        }

        // Attractions
        private void DgAttractions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selAttr = dgAttractions.SelectedItem as Attraction;
            if (_selAttr != null) { txtAttrName.Text = _selAttr.Name; cmbAttrPool.SelectedValue = _selAttr.AreaId; cmbAttrStatus.Text = _selAttr.Status; cmbAttrType.SelectedValue = _selAttr.SlideTypeId; }
        }
        private void BtnLoadAttr_Click(object sender, RoutedEventArgs e) => RefreshAll();
        private void BtnAddAttr_Click(object sender, RoutedEventArgs e) { _selAttr = null; txtAttrName.Text = ""; }
        private void BtnSaveAttr_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAttrPool.SelectedValue == null) { MessageBox.Show("Pool is required!"); return; }
            if (_selAttr == null) { _selAttr = new Attraction(); _ctx.Attractions.Add(_selAttr); }
            _selAttr.Name = txtAttrName.Text;
            _selAttr.AreaId = (int)cmbAttrPool.SelectedValue;
            _selAttr.Status = cmbAttrStatus.Text;
            _selAttr.SlideTypeId = cmbAttrType.SelectedValue as int?;
            _ctx.SaveChanges(); RefreshAll();
        }

        // Maintenance
        private void BtnAddMaint_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMaintStaff.SelectedValue == null) return;
            _ctx.MaintenanceRecords.Add(new MaintenanceRecord { ReportDate = DateTime.Now, ReportedBy = (int)cmbMaintStaff.SelectedValue, ProblemDescription = txtMaintDesc.Text });
            _ctx.SaveChanges(); RefreshAll(); txtMaintDesc.Text = "";
        }

        // Inventory
        private void BtnAddInv_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(txtInvQty.Text, out int q);
            _ctx.Inventory.Add(new InventoryItem { Name = txtInvName.Text, Quantity = q, SupplierId = cmbInvSup.SelectedValue as int? });
            _ctx.SaveChanges(); RefreshAll(); txtInvName.Text = "";
        }

        // Concessions
        private void BtnAddConc_Click(object sender, RoutedEventArgs e)
        {
            if (cmbConcArea.SelectedValue == null) { MessageBox.Show("Select Location!"); return; }
            _ctx.Concessions.Add(new Concession { Name = txtConcName.Text, AreaId = (int)cmbConcArea.SelectedValue, Status = cmbConcStatus.Text });
            _ctx.SaveChanges(); RefreshAll(); txtConcName.Text = "";
        }

        // Safety
        private void BtnAddSafe_Click(object sender, RoutedEventArgs e)
        {
            _ctx.SafetyStandards.Add(new SafetyStandard { Name = txtSafeName.Text, Description = txtSafeDesc.Text });
            _ctx.SaveChanges(); RefreshAll(); txtSafeName.Text = "";
        }

        // Weather
        private void BtnAddWeather_Click(object sender, RoutedEventArgs e)
        {
            decimal.TryParse(txtWeatherTemp.Text, out decimal t);
            _ctx.WeatherConditions.Add(new WeatherCondition { RecordDate = DateTime.Now, Temperature = t });
            _ctx.SaveChanges(); RefreshAll(); txtWeatherTemp.Text = "";
        }
    }
}