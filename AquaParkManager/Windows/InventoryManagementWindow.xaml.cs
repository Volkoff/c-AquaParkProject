using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class InventoryManagementWindow : Window
    {
        private AquaParkContext _context;
        private Inventory? _selectedItem;

        public InventoryManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadSuppliers();
            LoadInventory();
            ClearForm();
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = _context.Suppliers.ToList();
                cmbSupplier.ItemsSource = suppliers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading suppliers: " + ex.Message);
                lblStatus.Text = $"Error loading suppliers: {ex.Message}";
            }
        }

        private void LoadInventory()
        {
            try
            {
                // Materialize first to avoid translation of boolean expressions to Oracle SQL (no boolean literals).
                var entities = _context.Inventory
                    .Include(i => i.Supplier)
                    .ToList();

                var items = entities
                    .Select(i => new InventoryViewModel
                    {
                        ItemId = i.ItemId,
                        Name = i.Name,
                        Category = i.Category,
                        Quantity = i.Quantity,
                        MinStock = i.MinStock,
                        UnitPrice = i.UnitPrice,
                        Notes = i.Notes,
                        Supplier = i.Supplier,
                        IsLowStock = i.Quantity <= i.MinStock
                    })
                    .ToList();

                dgInventory.ItemsSource = items;
                lblStatus.Text = $"Loaded {items.Count} inventory items";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory: {ex.Message}");
                lblStatus.Text = $"Error loading inventory: {ex.Message}";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterInventory();
        }

        private void ChkLowStock_Checked(object sender, RoutedEventArgs e) => FilterInventory();
        private void ChkLowStock_Unchecked(object sender, RoutedEventArgs e) => FilterInventory();

        private void FilterInventory()
        {
            var searchText = txtSearch.Text.ToLower();
            var showLowStockOnly = chkLowStock.IsChecked == true;

            try
            {
                var entities = _context.Inventory
                    .Include(i => i.Supplier)
                    .Where(i => string.IsNullOrEmpty(searchText) ||
                               (i.Name != null && i.Name.ToLower().Contains(searchText)) ||
                               (i.Category != null && i.Category.ToLower().Contains(searchText)))
                    .ToList();

                var items = entities.Select(i => new InventoryViewModel
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    MinStock = i.MinStock,
                    UnitPrice = i.UnitPrice,
                    Notes = i.Notes,
                    Supplier = i.Supplier,
                    IsLowStock = i.Quantity <= i.MinStock
                }).ToList();

                if (showLowStockOnly)
                    items = items.Where(i => i.IsLowStock).ToList();

                dgInventory.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering inventory: {ex.Message}");
                lblStatus.Text = $"Error filtering inventory: {ex.Message}";
            }
        }

        private void DgInventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgInventory.SelectedItem is InventoryViewModel vm)
            {
                _selectedItem = _context.Inventory.Find(vm.ItemId);
                if (_selectedItem != null)
                {
                    txtItemName.Text = _selectedItem.Name;
                    txtCategory.Text = _selectedItem.Category;
                    cmbSupplier.SelectedValue = _selectedItem.SupplierId;
                    txtQuantity.Text = _selectedItem.Quantity.ToString();
                    txtMinStock.Text = _selectedItem.MinStock.ToString();
                    txtUnitPrice.Text = _selectedItem.UnitPrice?.ToString() ?? "";
                    txtNotes.Text = _selectedItem.Notes;

                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    lblStatus.Text = "Editing selected inventory item";
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var item = new Inventory
                {
                    Name = txtItemName.Text,
                    Category = txtCategory.Text,
                    SupplierId = cmbSupplier.SelectedValue as int?,
                    Quantity = int.Parse(txtQuantity.Text),
                    MinStock = int.Parse(txtMinStock.Text),
                    UnitPrice = string.IsNullOrWhiteSpace(txtUnitPrice.Text) ? null : decimal.Parse(txtUnitPrice.Text),
                    Notes = txtNotes.Text
                };

                _context.Inventory.Add(item);
                _context.SaveChanges();

                MessageBox.Show("Item added successfully!");
                lblStatus.Text = "Item added successfully!";
                LoadInventory();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}");
                lblStatus.Text = "Error adding item";
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null || !ValidateForm()) return;

            try
            {
                _selectedItem.Name = txtItemName.Text;
                _selectedItem.Category = txtCategory.Text;
                _selectedItem.SupplierId = cmbSupplier.SelectedValue as int?;
                _selectedItem.Quantity = int.Parse(txtQuantity.Text);
                _selectedItem.MinStock = int.Parse(txtMinStock.Text);
                _selectedItem.UnitPrice = string.IsNullOrWhiteSpace(txtUnitPrice.Text) ? null : decimal.Parse(txtUnitPrice.Text);
                _selectedItem.Notes = txtNotes.Text;

                _context.SaveChanges();

                MessageBox.Show("Item updated successfully!");
                lblStatus.Text = "Item updated successfully!";
                LoadInventory();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating item: {ex.Message}");
                lblStatus.Text = "Error updating item";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Inventory.Remove(_selectedItem);
                    _context.SaveChanges();

                    MessageBox.Show("Item deleted successfully!");
                    lblStatus.Text = "Item deleted successfully!";
                    LoadInventory();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting item: {ex.Message}");
                    lblStatus.Text = "Error deleting item";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            lblStatus.Text = "Form cleared";
        }

        private void ClearForm()
        {
            _selectedItem = null;
            txtItemName.Clear();
            txtCategory.Clear();
            cmbSupplier.SelectedIndex = -1;
            txtQuantity.Clear();
            txtMinStock.Clear();
            txtUnitPrice.Clear();
            txtNotes.Clear();
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                MessageBox.Show("Please enter item name.");
                lblStatus.Text = "Please enter item name.";
                return false;
            }
            if (!int.TryParse(txtQuantity.Text, out _))
            {
                MessageBox.Show("Please enter a valid quantity.");
                lblStatus.Text = "Please enter a valid quantity.";
                return false;
            }
            if (!int.TryParse(txtMinStock.Text, out _))
            {
                MessageBox.Show("Please enter a valid minimum stock.");
                lblStatus.Text = "Please enter a valid minimum stock.";
                return false;
            }
            return true;
        }
    }

    public class InventoryViewModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Notes { get; set; }
        public Supplier? Supplier { get; set; }
        public bool IsLowStock { get; set; }

        public string StockStatus => IsLowStock ? "⚠ LOW STOCK" : "✓ OK";
        public Brush StatusColor => IsLowStock ? Brushes.Red : Brushes.Green;
    }
}
