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
                MessageBox.Show($"Error loading pools: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText)) { LoadPools(); return; }

            try
            {
                dgPools.ItemsSource = _context.Pools.Where(p => p.Name.ToLower().Contains(searchText)).ToList();
            }
            catch { }
        }

        private void DgPools_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPool = dgPools.SelectedItem as Pool;
            if (_selectedPool != null)
            {
                txtName.Text = _selectedPool.Name;
                txtCapacity.Text = _selectedPool.Capacity.ToString();
                chkIndoors.IsChecked = _selectedPool.Indoors == "Y";
                // DepthMin/Max nejsou v DB, neøešíme
            }
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
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;
                int.TryParse(txtCapacity.Text, out int cap);

                if (_selectedPool == null)
                {
                    var newPool = new Pool
                    {
                        Name = txtName.Text,
                        Capacity = cap,
                        Indoors = chkIndoors.IsChecked == true ? "Y" : "N"
                    };
                    _context.Pools.Add(newPool);
                }
                else
                {
                    _selectedPool.Name = txtName.Text;
                    _selectedPool.Capacity = cap;
                    _selectedPool.Indoors = chkIndoors.IsChecked == true ? "Y" : "N";
                }

                _context.SaveChanges();
                LoadPools();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving pool: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPool != null)
            {
                _context.Pools.Remove(_selectedPool);
                _context.SaveChanges();
                LoadPools();
                ClearForm();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            txtName.Text = "";
            txtCapacity.Text = "";
            txtDepthMin.Text = ""; // Ignorováno, jen pro UI
            txtDepthMax.Text = "";
            chkIndoors.IsChecked = false;
            txtNotes.Text = "";
            _selectedPool = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}