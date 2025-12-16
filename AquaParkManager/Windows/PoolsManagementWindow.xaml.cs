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

            // APLIKACE OPRÁVNÌNÍ - Skrytí editaèních prvkù pro hosty
            ApplyPermissions();
        }

        private void ApplyPermissions()
        {
            // Pokud není nikdo pøihlášen (Host)
            if (App.CurrentUser == null)
            {
                this.Title += " (Pouze pro ètení)";

                // Skryjeme panel pro pøidání
                btnAddPool.Visibility = Visibility.Collapsed;

                // Skryjeme celý pravý panel s detaily (nebo jen tlaèítka)
                // Zde skryjeme tlaèítka pro uložení/smazání
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;

                // Nastavíme formuláø jako ReadOnly
                txtName.IsReadOnly = true;
                txtCapacity.IsReadOnly = true;
                chkIndoors.IsEnabled = false;
                txtNotes.IsReadOnly = true;

                // DataGrid pouze pro ètení (znemožní výbìr pro editaci, pokud chceme)
                // Nebo necháme výbìr, aby vidìli detaily, ale nemohli uložit.
                lblStatus.Text = "Prohlížíte jako host. Editace není povolena.";
            }
        }

        private void LoadPools()
        {
            try
            {
                var pools = _context.Pools.ToList();
                dgPools.ItemsSource = pools;
                // Pro hosta vypíšeme jen poèet, ne "Loaded..."
                if (App.CurrentUser != null) lblStatus.Text = $"Naèteno {pools.Count} bazénù";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba: {ex.Message}");
            }
        }

        // ... Zbytek metod (TxtSearch, SelectionChanged...) zùstává stejný ...
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText)) { LoadPools(); return; }
            dgPools.ItemsSource = _context.Pools.Where(p => p.Name.ToLower().Contains(searchText)).ToList();
        }

        private void DgPools_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPool = dgPools.SelectedItem as Pool;
            if (_selectedPool != null)
            {
                txtName.Text = _selectedPool.Name;
                txtCapacity.Text = _selectedPool.Capacity.ToString();
                chkIndoors.IsChecked = _selectedPool.Indoors == "Y";
                txtNotes.Text = _selectedPool.Notes ?? "";
            }
        }

        private void BtnAddPool_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedPool = null; }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Pojistka pro jistotu
            if (App.CurrentUser == null) { MessageBox.Show("Nemáte oprávnìní."); return; }

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
                        Indoors = chkIndoors.IsChecked == true ? "Y" : "N",
                        Notes = txtNotes.Text
                    };
                    _context.Pools.Add(newPool);
                }
                else
                {
                    _selectedPool.Name = txtName.Text;
                    _selectedPool.Capacity = cap;
                    _selectedPool.Indoors = chkIndoors.IsChecked == true ? "Y" : "N";
                    _selectedPool.Notes = txtNotes.Text;
                }
                _context.SaveChanges();
                LoadPools();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;
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
            txtName.Clear(); txtCapacity.Clear(); chkIndoors.IsChecked = false; txtNotes.Clear();
            _selectedPool = null;
        }
    }
}