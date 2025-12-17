using AquaParkManager.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AquaParkManager.Windows
{
    public partial class MediaManagementWindow : Window
    {
        private AquaParkContext _context;

        public MediaManagementWindow()
        {
            InitializeComponent();
            _context = new AquaParkContext();
            LoadMedia();
        }

        private void LoadMedia()
        {
            try
            {
                // Načteme seznam, ale zatím NE data (abychom nebrzdili aplikaci)
                var list = _context.Media.Select(m => new { m.MediaId, m.FileName, m.MimeType }).ToList();
                dgMedia.ItemsSource = list;
                lblStatus.Text = $"Loaded {list.Count} media items";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading media: " + ex.Message);
                lblStatus.Text = "Error loading media";
            }
        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(dlg.FileName);
                    string fileName = Path.GetFileName(dlg.FileName);

                    var firstAttraction = _context.Attractions.FirstOrDefault();
                    if (firstAttraction == null)
                    {
                        MessageBox.Show("Nelze nahrát: V databázi není žádná atrakce (ATTRACTIONS), ke které bych obrázek přiřadil.");
                        return;
                    }

                    var newMedia = new Media
                    {
                        FileName = fileName,
                        MimeType = "image/jpeg",
                        MediaData = fileBytes,
                        RelatedTable = "ATTRACTIONS",
                        RelatedId = firstAttraction.AttractionId // Použijeme existující ID
                    }; ;

                    _context.Media.Add(newMedia);
                    _context.SaveChanges();
                    LoadMedia();
                    lblStatus.Text = "Image saved to database BLOB successfully!";
                    MessageBox.Show("Image saved to database BLOB successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving BLOB: " + ex.Message);
                    lblStatus.Text = "Error saving BLOB";
                }
            }
        }

        private void DgMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Když klikneme na řádek, stáhneme plný BLOB a zobrazíme
            if (dgMedia.SelectedItem == null) return;

            dynamic selectedItem = dgMedia.SelectedItem;
            int id = selectedItem.MediaId;

            var fullRecord = _context.Media.Find(id);
            if (fullRecord != null && fullRecord.MediaData != null)
            {
                using (var ms = new MemoryStream(fullRecord.MediaData))
                {
                    try
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = ms;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        imgPreview.Source = image;
                        lblStatus.Text = $"Previewing: {fullRecord.FileName}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error displaying image: " + ex.Message);
                        lblStatus.Text = "Error displaying image";

                    }
                }
            }
        }
    }
}