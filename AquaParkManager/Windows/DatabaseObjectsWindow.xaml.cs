using System.Linq;
using System.Windows;
using AquaParkManager.Models;

namespace AquaParkManager.Windows
{
    public partial class DatabaseObjectsWindow : Window
    {
        public DatabaseObjectsWindow()
        {
            InitializeComponent();
            using (var ctx = new AquaParkContext())
            {
                // Čtení z pohledu V_DB_OBJECTS
                dgObjects.ItemsSource = ctx.DbObjects.ToList();
            }
        }
    }
}