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
            ApplyPermissions(); // <--- ZABEZPEČENÍ
        }

        private void ApplyPermissions()
        {
            if (App.CurrentUser == null)
            {
                this.Title += " (Pouze pro čtení)";
                btnAddSlide.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnClear.Visibility = Visibility.Collapsed;

                txtName.IsReadOnly = true;
                cmbSlideType.IsEnabled = false;
                cmbPool.IsEnabled = false;
                cmbStatus.IsEnabled = false;

                lblStatus.Text = "Host: Editace zakázána.";
            }
        }

        // ... Zbytek metod beze změny (LoadSlides, LoadPools, BtnSave_Click atd.) ...
        // PRO KOMPLETNOST UVÁDÍM ZKRÁCENĚ:
        private void LoadSlideTypes() { cmbSlideType.ItemsSource = _context.SlideTypes.ToList(); }
        private void LoadPools() { cmbPool.ItemsSource = _context.Pools.ToList(); }
        private void LoadSlides() { dgSlides.ItemsSource = _context.Attractions.Include(s => s.SlideType).Include(s => s.Pool).Where(s => s.SlideTypeId != null).ToList(); }
        private void DgSlides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSlide = dgSlides.SelectedItem as Attraction;
            if (_selectedSlide != null)
            {
                txtName.Text = _selectedSlide.Name;
                cmbSlideType.SelectedValue = _selectedSlide.SlideTypeId;
                cmbStatus.Text = _selectedSlide.Status;
                cmbPool.SelectedValue = _selectedSlide.AreaId;
            }
        }
        private void BtnAddSlide_Click(object sender, RoutedEventArgs e) { ClearForm(); _selectedSlide = null; }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;
            // ... (Původní logika ukládání) ...
            try
            {
                if (_selectedSlide == null) { /* Add logic */ _context.Attractions.Add(new Attraction { Name = txtName.Text, AreaId = (int)cmbPool.SelectedValue, SlideTypeId = (int?)cmbSlideType.SelectedValue, Status = cmbStatus.Text }); }
                else { /* Update logic */ _selectedSlide.Name = txtName.Text; _selectedSlide.AreaId = (int)cmbPool.SelectedValue; _selectedSlide.SlideTypeId = (int?)cmbSlideType.SelectedValue; _selectedSlide.Status = cmbStatus.Text; }
                _context.SaveChanges(); LoadSlides(); ClearForm();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e) { if (App.CurrentUser != null && _selectedSlide != null) { _context.Attractions.Remove(_selectedSlide); _context.SaveChanges(); LoadSlides(); } }
        private void BtnClear_Click(object sender, RoutedEventArgs e) { ClearForm(); }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) { }
        private void ClearForm() { txtName.Clear(); cmbSlideType.SelectedIndex = -1; cmbPool.SelectedIndex = -1; _selectedSlide = null; }
    }
}