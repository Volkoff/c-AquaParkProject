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
            LoadPools(); // Potøebujeme pro AreaId (Park Area)
            LoadSlideTypes(); // Potøebujeme pro SlideType
            LoadAttractions();
            ClearForm();
        }

        // --- NOVÉ: Naètení oblastí (Pools/Areas) ---
        // V DB je vazba PARK_AREAS_AREA_ID povinná (NOT NULL)
        private void LoadPools()
        {
            try
            {
                // Doèasnì použijeme existující UI prvek nebo vytvoøíme nový, 
                // ale pro rychlou opravu bez zmìny XAML pøiøadíme natvrdo ID=1 nebo první nalezenou,
                // pokud v XAML není ComboBox pro Area.
                // V pùvodním XAML nebyl ComboBox pro AreaId, což je PROBLÉM, protože DB to vyžaduje.
                // Prozatím to v Save metodì ošetøíme defaultní hodnotou.
            }
            catch { }
        }

        private void LoadSlideTypes()
        {
            // Pokud chceme nastavit typ skluzavky, potøebovali bychom ComboBox.
            // Pùvodní XAML mìl 'cmbAttractionType' s textovými hodnotami, což neodpovídá DB.
        }

        private void LoadAttractions()
        {
            try
            {
                var attractions = _context.Attractions
                    .Include(a => a.Pool)      // Area
                    .Include(a => a.SlideType) // Typ skluzavky
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
                // Ostatní pole (AttractionType, ObjectId, Capacity) nelze naèíst z DB, protože tam nejsou.
                // Necháme je prázdná nebo je v XAML ideálnì smaž.
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

                // HACK: Protože v UI nemáme výbìr 'AreaId' (Park Area),
                // musíme najít nìjaké existující ID, jinak DB vyhodí chybu (Constraint NOT NULL).
                var defaultArea = _context.Pools.FirstOrDefault();
                int areaId = defaultArea?.PoolId ?? 1;

                if (_selectedAttraction == null)
                {
                    var newAttraction = new Attraction
                    {
                        Name = txtName.Text.Trim(),
                        Status = cmbStatus.Text,
                        AreaId = areaId, // Povinné pole v DB!
                        // SlideTypeId necháme null (není to skluzavka), pokud to v UI nevyøešíme
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
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Error saving attraction: {msg}");
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
            // Vymazání polí, která se neukládají, aby to uživatele nepletlo
            txtObjectId.Text = "";
            txtCapacity.Text = "";
            txtNotes.Text = "";
            cmbAttractionType.SelectedIndex = -1;
        }
    }
}