using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace AquaParkManager.Windows
{
    public partial class FacilitiesWindow : Window
    {
        private AquaParkContext _ctx = new AquaParkContext();
        private Attraction? _selAttr;

        public FacilitiesWindow()
        {
            InitializeComponent();
            dgInv.LoadingRow += DgInv_LoadingRow;
            RefreshAll();
        }

        private void DgInv_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            var item = e.Row.DataContext as InventoryItem;
            if (item != null && item.Quantity <= item.MinStock)
            {
                e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));
            }
        }

        private void RefreshAll()
        {
            try
            {
                // Používáme ParkArea namísto Pool
                dgAttractions.ItemsSource = _ctx.Attractions.Include(a => a.SlideType).Include(a => a.ParkArea).ToList();
                cmbAttrPool.ItemsSource = _ctx.ParkAreas.ToList();
                cmbAttrType.ItemsSource = _ctx.SlideTypes.ToList();

                dgMaint.ItemsSource = _ctx.MaintenanceRecords.Include(m => m.Staff).OrderByDescending(m => m.ReportDate).ToList();
                cmbMaintStaff.ItemsSource = _ctx.Staff.ToList();

                dgInv.ItemsSource = _ctx.Inventory.Include(i => i.Supplier).ToList();
                cmbInvSup.ItemsSource = _ctx.Suppliers.ToList();

                dgConcessions.ItemsSource = _ctx.Concessions.ToList();
                cmbConcArea.ItemsSource = _ctx.ParkAreas.ToList();

                dgSafety.ItemsSource = _ctx.SafetyStandards.ToList();
                dgWeather.ItemsSource = _ctx.WeatherConditions.OrderByDescending(w => w.RecordDate).ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void DgAttractions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selAttr = dgAttractions.SelectedItem as Attraction;
            if (_selAttr != null)
            {
                txtAttrName.Text = _selAttr.Name;
                cmbAttrPool.SelectedValue = _selAttr.AreaId;
                cmbAttrStatus.Text = _selAttr.Status;
                cmbAttrType.SelectedValue = _selAttr.SlideTypeId;
            }
        }
        private void BtnLoadAttr_Click(object sender, RoutedEventArgs e) => RefreshAll();
        private void BtnAddAttr_Click(object sender, RoutedEventArgs e) { _selAttr = null; txtAttrName.Text = ""; }

        private void BtnSaveAttr_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAttrPool.SelectedValue == null || string.IsNullOrWhiteSpace(txtAttrName.Text)) { MessageBox.Show("Name and Area required"); return; }

            try
            {
                bool isNew = _selAttr == null;
                string newStatus = cmbAttrStatus.Text;

                if (isNew)
                {
                    _selAttr = new Attraction();
                    _ctx.Attractions.Add(_selAttr);
                    _selAttr.Name = txtAttrName.Text;
                    _selAttr.AreaId = (int)cmbAttrPool.SelectedValue;
                    _selAttr.Status = newStatus;
                    _selAttr.SlideTypeId = cmbAttrType.SelectedValue as int?;
                    _ctx.SaveChanges();
                }
                else
                {
                    // Volání procedury SP_SET_ATTRACTION_STATUS při změně stavu
                    if (_selAttr.Status != newStatus)
                    {
                        if (App.CurrentUser?.StaffId == null)
                        {
                            MessageBox.Show("Current user is not linked to staff. Cannot log status change.");
                            return;
                        }

                        var pId = new OracleParameter("p_attraction_id", OracleDbType.Int32, _selAttr.AttractionId, ParameterDirection.Input);
                        var pStatus = new OracleParameter("p_new_status", OracleDbType.Varchar2, newStatus, ParameterDirection.Input);
                        var pStaff = new OracleParameter("p_staff_id", OracleDbType.Int32, App.CurrentUser.StaffId, ParameterDirection.Input);

                        _ctx.Database.ExecuteSqlRaw("BEGIN SP_SET_ATTRACTION_STATUS(:p_attraction_id, :p_new_status, :p_staff_id); END;", pId, pStatus, pStaff);

                        // Synchronizace stavu v EF pro další práci
                        _selAttr.Status = newStatus;
                    }

                    _selAttr.Name = txtAttrName.Text;
                    _selAttr.AreaId = (int)cmbAttrPool.SelectedValue;
                    _selAttr.SlideTypeId = cmbAttrType.SelectedValue as int?;
                    _ctx.SaveChanges();
                }

                MessageBox.Show("Attraction Saved!");
                RefreshAll();
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        private void BtnAddMaint_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMaintStaff.SelectedValue == null) return;
            try
            {
                _ctx.MaintenanceRecords.Add(new MaintenanceRecord { ReportDate = DateTime.Now, ReportedBy = (int)cmbMaintStaff.SelectedValue, ProblemDescription = txtMaintDesc.Text });
                _ctx.SaveChanges(); RefreshAll(); txtMaintDesc.Text = "";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddInv_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtInvQty.Text, out int q) || !int.TryParse(txtInvMin.Text, out int m) || !decimal.TryParse(txtInvPrice.Text, out decimal p)) return;
            try
            {
                _ctx.Inventory.Add(new InventoryItem { Name = txtInvName.Text, Quantity = q, MinStock = m, UnitPrice = p, SupplierId = cmbInvSup.SelectedValue as int? });
                _ctx.SaveChanges(); RefreshAll(); txtInvName.Text = "";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddConc_Click(object sender, RoutedEventArgs e)
        {
            if (cmbConcArea.SelectedValue == null) { MessageBox.Show("Select Location!"); return; }
            try
            {
                _ctx.Concessions.Add(new Concession { Name = txtConcName.Text, AreaId = (int)cmbConcArea.SelectedValue, Status = cmbConcStatus.Text });
                _ctx.SaveChanges();
                RefreshAll();
                txtConcName.Text = "";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddSafe_Click(object sender, RoutedEventArgs e)
        {
            try { _ctx.SafetyStandards.Add(new SafetyStandard { Name = txtSafeName.Text, Description = txtSafeDesc.Text }); _ctx.SaveChanges(); RefreshAll(); } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnAddWeather_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtWeatherTemp.Text, out decimal t)) return;
            try { _ctx.WeatherConditions.Add(new WeatherCondition { RecordDate = DateTime.Now, Temperature = t }); _ctx.SaveChanges(); RefreshAll(); } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}