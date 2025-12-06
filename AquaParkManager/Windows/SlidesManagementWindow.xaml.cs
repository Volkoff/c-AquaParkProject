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
        private Attraction? _selectedSlide;

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
                cmbSlideType.ItemsSource = _context.SlideTypes.ToList();
                cmbSlideType.DisplayMemberPath = "Name";
                cmbSlideType.SelectedValuePath = "SlideTypeId";
            }
            catch { }
        }

        private void LoadPools()
        {
            try
            {
                cmbPool.ItemsSource = _context.Pools.ToList();
                cmbPool.DisplayMemberPath = "Name";
                cmbPool.SelectedValuePath = "PoolId";
            }
            catch { }
        }

        private void LoadSlides()
        {
            try
            {
                var slides = _context.Attractions
                    .Include(s => s.SlideType)
                    .Include(s => s.Pool)
                    .Where(s => s.SlideTypeId != null)
                    .ToList();
                dgSlides.ItemsSource = slides;
                lblStatus.Text = $"Loaded {slides.Count} slides";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void DgSlides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSlide = dgSlides.SelectedItem as Attraction;
            if (_selectedSlide != null)
            {
                txtName.Text = _selectedSlide.Name;
                cmbSlideType.SelectedValue = _selectedSlide.SlideTypeId;
                cmbStatus.Text = _selectedSlide.Status;
                cmbPool.SelectedValue = _selectedSlide.AreaId;

                // Vyèistíme pole, která se neukládají do DB
                txtLength.Text = "";
                txtHeight.Text = "";
                txtMinHeight.Text = "";
                txtMaxWeight.Text = "";
                txtNotes.Text = "";
            }
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
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;

                if (cmbPool.SelectedValue == null)
                {
                    MessageBox.Show("Select a Pool first.");
                    return;
                }
                int areaId = (int)cmbPool.SelectedValue;

                if (_selectedSlide == null)
                {
                    var newSlide = new Attraction
                    {
                        Name = txtName.Text.Trim(),
                        SlideTypeId = (int?)cmbSlideType.SelectedValue,
                        Status = cmbStatus.Text,
                        AreaId = areaId
                    };
                    _context.Attractions.Add(newSlide);
                }
                else
                {
                    _selectedSlide.Name = txtName.Text.Trim();
                    _selectedSlide.SlideTypeId = (int?)cmbSlideType.SelectedValue;
                    _selectedSlide.Status = cmbStatus.Text;
                    _selectedSlide.AreaId = areaId;
                }

                _context.SaveChanges();
                LoadSlides();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving slide: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSlide != null)
            {
                _context.Attractions.Remove(_selectedSlide);
                _context.SaveChanges();
                LoadSlides();
                ClearForm();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }

        private void ClearForm()
        {
            txtName.Text = "";
            cmbSlideType.SelectedIndex = -1;
            cmbStatus.SelectedIndex = 0;
            cmbPool.SelectedIndex = -1;
            txtLength.Text = "";
            txtHeight.Text = "";
            txtMinHeight.Text = "";
            txtMaxWeight.Text = "";
            txtNotes.Text = "";
            _selectedSlide = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}