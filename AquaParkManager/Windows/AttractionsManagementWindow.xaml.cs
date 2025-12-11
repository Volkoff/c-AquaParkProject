using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

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
            LoadAreas();
            LoadAttractions();
            ClearForm();
        }

        private void LoadAreas()
        {
            try
            {
                cmbArea.ItemsSource = _context.Pools.ToList();
            }
            catch { }
        }

        private void LoadAttractions()
        {
            try
            {
                // Naèteme atrakce, které NEJSOU skluzavky (SlideTypeId IS NULL)
                // nebo všechny, záleží na preferenci. Zobrazíme všechny.
                var attractions = _context.Attractions
                    .Include(a => a.Pool)
                    .ToList();
                dgAttractions.ItemsSource = attractions;
                lblStatus.Text = $"Loaded {attractions.Count} attractions";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }

        private void DgAttractions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedAttraction = dgAttractions.SelectedItem as Attraction;
            if (_selectedAttraction != null)
            {
                txtName.Text = _selectedAttraction.Name;
                cmbStatus.Text = _selectedAttraction.Status;
                cmbArea.SelectedValue = _selectedAttraction.AreaId;
            }
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

                if (cmbArea.SelectedValue == null)
                {
                    MessageBox.Show("Please select a Location (Area).");
                    return;
                }
                int areaId = (int)cmbArea.SelectedValue;

                if (_selectedAttraction == null)
                {
                    var newAttraction = new Attraction
                    {
                        Name = txtName.Text.Trim(),
                        Status = cmbStatus.Text,
                        AreaId = areaId,
                        SlideTypeId = null // Není to skluzavka
                    };

                    _context.Attractions.Add(newAttraction);
                }
                else
                {
                    _selectedAttraction.Name = txtName.Text.Trim();
                    _selectedAttraction.Status = cmbStatus.Text;
                    _selectedAttraction.AreaId = areaId;
                }

                _context.SaveChanges();
                LoadAttractions();
                ClearForm();
                lblStatus.Text = "Attraction saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving attraction: {ex.Message}");
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
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedAttraction = null; }

        private void ClearForm()
        {
            txtName.Text = "";
            cmbStatus.SelectedIndex = 0;
            cmbArea.SelectedIndex = -1;
            _selectedAttraction = null;
        }
    }
}