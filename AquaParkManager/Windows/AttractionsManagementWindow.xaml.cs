using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;

namespace AquaParkManager.Windows
{
    public partial class AttractionsManagementWindow : Window
    {
        private AquaParkContext _context;
        private Attraction? _selectedAttraction;

        public AttractionsManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadAttractions();
            ClearForm();
        }

        private void LoadAttractions()
        {
            try
            {
                var attractions = _context.Attractions.ToList();
                dgAttractions.ItemsSource = attractions;
                lblStatus.Text = $"Loaded {attractions.Count} attractions";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attractions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading attractions";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }

        private void DgAttractions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedAttraction = dgAttractions.SelectedItem as Attraction;
            if (_selectedAttraction != null)
            {
                LoadAttractionDetails(_selectedAttraction);
            }
        }

        private void LoadAttractionDetails(Attraction attraction)
        {
            txtName.Text = attraction.Name;
            cmbAttractionType.Text = attraction.AttractionType;
            txtObjectId.Text = attraction.ObjectId?.ToString() ?? "";
            cmbStatus.Text = attraction.Status;
            txtCapacity.Text = attraction.Capacity?.ToString() ?? "";
            txtNotes.Text = attraction.Notes ?? "";
        }

        private void BtnAddAttraction_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedAttraction = null;
            txtName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;

                if (_selectedAttraction == null)
                {
                    // Add new attraction
                    var newAttraction = new Attraction
                    {
                        Name = txtName.Text.Trim(),
                        AttractionType = cmbAttractionType.Text,
                        ObjectId = int.TryParse(txtObjectId.Text, out int objectId) ? objectId : null,
                        Status = cmbStatus.Text,
                        Capacity = int.TryParse(txtCapacity.Text, out int capacity) ? capacity : null,
                        Notes = txtNotes.Text.Trim()
                    };

                    _context.Attractions.Add(newAttraction);
                    _context.SaveChanges();
                    lblStatus.Text = "Attraction added successfully";
                }
                else
                {
                    // Update existing attraction
                    _selectedAttraction.Name = txtName.Text.Trim();
                    _selectedAttraction.AttractionType = cmbAttractionType.Text;
                    _selectedAttraction.ObjectId = int.TryParse(txtObjectId.Text, out int objectId) ? objectId : null;
                    _selectedAttraction.Status = cmbStatus.Text;
                    _selectedAttraction.Capacity = int.TryParse(txtCapacity.Text, out int capacity) ? capacity : null;
                    _selectedAttraction.Notes = txtNotes.Text.Trim();

                    _context.SaveChanges();
                    lblStatus.Text = "Attraction updated successfully";
                }

                LoadAttractions();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving attraction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving attraction";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAttraction == null) return;
            try
            {
                _context.Attractions.Remove(_selectedAttraction);
                _context.SaveChanges();
                LoadAttractions();
                ClearForm();
                lblStatus.Text = "Attraction deleted successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting attraction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedAttraction = null;
            dgAttractions.SelectedItem = null;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            cmbAttractionType.SelectedIndex = -1;
            txtObjectId.Text = "";
            cmbStatus.SelectedIndex = 0;
            txtCapacity.Text = "";
            txtNotes.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}