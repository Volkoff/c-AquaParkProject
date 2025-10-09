using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;

namespace AquaParkManager.Windows
{
    public partial class PoolsManagementWindow : Window
    {
        private AquaParkContext _context;
        private Pool? _selectedPool;

        public PoolsManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadPools();
            ClearForm();
        }

        private void LoadPools()
        {
            try
            {
                var pools = _context.Pools.ToList();
                dgPools.ItemsSource = pools;
                lblStatus.Text = $"Loaded {pools.Count} pools";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading pools";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadPools();
                return;
            }

            try
            {
                var filteredPools = _context.Pools
                    .Where(p => p.Name.ToLower().Contains(searchText))
                    .ToList();
                
                dgPools.ItemsSource = filteredPools;
                lblStatus.Text = $"Found {filteredPools.Count} pools";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching pools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgPools_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPool = dgPools.SelectedItem as Pool;
            if (_selectedPool != null)
            {
                LoadPoolDetails(_selectedPool);
            }
        }

        private void LoadPoolDetails(Pool pool)
        {
            txtName.Text = pool.Name;
            txtDepthMin.Text = pool.DepthMin.ToString();
            txtDepthMax.Text = pool.DepthMax.ToString();
            txtCapacity.Text = pool.Capacity.ToString();
            chkIndoors.IsChecked = pool.Indoors == "Y";
            txtNotes.Text = pool.Notes ?? "";
        }

        private void BtnAddPool_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedPool = null;
            txtName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Pool name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtDepthMin.Text, out decimal minDepth) || minDepth < 0)
                {
                    MessageBox.Show("Please enter a valid minimum depth (>= 0).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtDepthMax.Text, out decimal maxDepth) || maxDepth < 0)
                {
                    MessageBox.Show("Please enter a valid maximum depth (>= 0).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (maxDepth < minDepth)
                {
                    MessageBox.Show("Maximum depth must be greater than or equal to minimum depth.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity < 0)
                {
                    MessageBox.Show("Please enter a valid capacity (>= 0).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedPool == null)
                {
                    // Add new pool
                    var newPool = new Pool
                    {
                        Name = txtName.Text.Trim(),
                        DepthMin = minDepth,
                        DepthMax = maxDepth,
                        Capacity = capacity,
                        Indoors = chkIndoors.IsChecked == true ? "Y" : "N",
                        Notes = txtNotes.Text.Trim()
                    };

                    _context.Pools.Add(newPool);
                    _context.SaveChanges();
                    lblStatus.Text = "Pool added successfully";
                }
                else
                {
                    // Update existing pool
                    _selectedPool.Name = txtName.Text.Trim();
                    _selectedPool.DepthMin = minDepth;
                    _selectedPool.DepthMax = maxDepth;
                    _selectedPool.Capacity = capacity;
                    _selectedPool.Indoors = chkIndoors.IsChecked == true ? "Y" : "N";
                    _selectedPool.Notes = txtNotes.Text.Trim();

                    _context.SaveChanges();
                    lblStatus.Text = "Pool updated successfully";
                }

                LoadPools();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving pool: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving pool";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPool == null)
            {
                MessageBox.Show("Please select a pool to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the pool '{_selectedPool.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Pools.Remove(_selectedPool);
                    _context.SaveChanges();
                    LoadPools();
                    ClearForm();
                    lblStatus.Text = "Pool deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting pool: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error deleting pool";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedPool = null;
            dgPools.SelectedItem = null;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtDepthMin.Text = "";
            txtDepthMax.Text = "";
            txtCapacity.Text = "";
            chkIndoors.IsChecked = false;
            txtNotes.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
