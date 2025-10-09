using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager.Windows
{
    public partial class SlidesManagementWindow : Window
    {
        private AquaParkContext _context;
        private Slide? _selectedSlide;

        public SlidesManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadSlideTypes();
            LoadPools();
            LoadSlides();
            ClearForm();
        }

        private void LoadSlideTypes()
        {
            try
            {
                var slideTypes = _context.SlideTypes.ToList();
                cmbSlideType.ItemsSource = slideTypes;
                cmbSlideType.DisplayMemberPath = "Name";
                cmbSlideType.SelectedValuePath = "SlideTypeId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading slide types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPools()
        {
            try
            {
                var pools = _context.Pools.ToList();
                cmbPool.ItemsSource = pools;
                cmbPool.DisplayMemberPath = "Name";
                cmbPool.SelectedValuePath = "PoolId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSlides()
        {
            try
            {
                var slides = _context.Slides
                    .Include(s => s.SlideType)
                    .Include(s => s.Pool)
                    .ToList();
                dgSlides.ItemsSource = slides;
                lblStatus.Text = $"Loaded {slides.Count} slides";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading slides: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading slides";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadSlides();
                return;
            }

            try
            {
                var filteredSlides = _context.Slides
                    .Include(s => s.SlideType)
                    .Include(s => s.Pool)
                    .Where(s => s.Name.ToLower().Contains(searchText) ||
                               s.SlideType.Name.ToLower().Contains(searchText))
                    .ToList();
                
                dgSlides.ItemsSource = filteredSlides;
                lblStatus.Text = $"Found {filteredSlides.Count} slides";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching slides: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgSlides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSlide = dgSlides.SelectedItem as Slide;
            if (_selectedSlide != null)
            {
                LoadSlideDetails(_selectedSlide);
            }
        }

        private void LoadSlideDetails(Slide slide)
        {
            txtName.Text = slide.Name;
            cmbSlideType.SelectedValue = slide.SlideTypeId;
            txtLength.Text = slide.LengthM?.ToString() ?? "";
            txtHeight.Text = slide.HeightM?.ToString() ?? "";
            txtMinHeight.Text = slide.MinHeightCm?.ToString() ?? "";
            txtMaxWeight.Text = slide.MaxWeightKg?.ToString() ?? "";
            cmbStatus.Text = slide.Status;
            cmbPool.SelectedValue = slide.PoolId;
            dpInstallationDate.SelectedDate = slide.InstallationDate;
            txtNotes.Text = slide.Notes ?? "";
        }

        private void BtnAddSlide_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedSlide = null;
            txtName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Slide name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbSlideType.SelectedValue == null)
                {
                    MessageBox.Show("Please select a slide type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedSlide == null)
                {
                    // Add new slide
                    var newSlide = new Slide
                    {
                        Name = txtName.Text.Trim(),
                        SlideTypeId = (int)cmbSlideType.SelectedValue,
                        LengthM = decimal.TryParse(txtLength.Text, out decimal length) ? length : null,
                        HeightM = decimal.TryParse(txtHeight.Text, out decimal height) ? height : null,
                        MinHeightCm = int.TryParse(txtMinHeight.Text, out int minHeight) ? minHeight : null,
                        MaxWeightKg = int.TryParse(txtMaxWeight.Text, out int maxWeight) ? maxWeight : null,
                        Status = cmbStatus.Text,
                        PoolId = cmbPool.SelectedValue as int?,
                        InstallationDate = dpInstallationDate.SelectedDate,
                        Notes = txtNotes.Text.Trim()
                    };

                    _context.Slides.Add(newSlide);
                    _context.SaveChanges();
                    lblStatus.Text = "Slide added successfully";
                }
                else
                {
                    // Update existing slide
                    _selectedSlide.Name = txtName.Text.Trim();
                    _selectedSlide.SlideTypeId = (int)cmbSlideType.SelectedValue;
                    _selectedSlide.LengthM = decimal.TryParse(txtLength.Text, out decimal length) ? length : null;
                    _selectedSlide.HeightM = decimal.TryParse(txtHeight.Text, out decimal height) ? height : null;
                    _selectedSlide.MinHeightCm = int.TryParse(txtMinHeight.Text, out int minHeight) ? minHeight : null;
                    _selectedSlide.MaxWeightKg = int.TryParse(txtMaxWeight.Text, out int maxWeight) ? maxWeight : null;
                    _selectedSlide.Status = cmbStatus.Text;
                    _selectedSlide.PoolId = cmbPool.SelectedValue as int?;
                    _selectedSlide.InstallationDate = dpInstallationDate.SelectedDate;
                    _selectedSlide.Notes = txtNotes.Text.Trim();

                    _context.SaveChanges();
                    lblStatus.Text = "Slide updated successfully";
                }

                LoadSlides();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving slide: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error saving slide";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSlide == null)
            {
                MessageBox.Show("Please select a slide to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the slide '{_selectedSlide.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Slides.Remove(_selectedSlide);
                    _context.SaveChanges();
                    LoadSlides();
                    ClearForm();
                    lblStatus.Text = "Slide deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting slide: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error deleting slide";
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            _selectedSlide = null;
            dgSlides.SelectedItem = null;
        }

        private void ClearForm()
        {
            txtName.Text = "";
            cmbSlideType.SelectedIndex = -1;
            txtLength.Text = "";
            txtHeight.Text = "";
            txtMinHeight.Text = "";
            txtMaxWeight.Text = "";
            cmbStatus.SelectedIndex = 0;
            cmbPool.SelectedIndex = -1;
            dpInstallationDate.SelectedDate = null;
            txtNotes.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}
