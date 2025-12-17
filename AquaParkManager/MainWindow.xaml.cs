using System.Linq;
using System.Windows;
using AquaParkManager.Models;
using AquaParkManager.Windows;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                ApplyRoles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při inicializaci hlavního okna: {ex.Message}\n\n{ex.StackTrace}", "Chyba aplikace");
                this.Close();
            }
        }

        private void ApplyRoles()
        {
            try
            {
                bool isAdmin = false;
                bool isStaff = false;

                // Bezpečná kontrola CurrentUser
                if (App.CurrentUser != null)
                {
                    using (var ctx = new AquaParkContext())
                    {
                        var roles = ctx.UserRoles.Include(ur => ur.Role)
                                       .Where(ur => ur.UserId == App.CurrentUser.UserId)
                                       .Select(ur => ur.Role != null ? ur.Role.RoleName : "")
                                       .ToList();
                        isAdmin = roles.Contains("ADMIN") || roles.Contains("MANAGER");
                        isStaff = isAdmin || roles.Contains("STAFF");
                        
                        // If no roles found, check if user is linked to a Staff record (staff are admins)
                        if (roles.Count == 0 && App.CurrentUser.StaffId.HasValue)
                        {
                            isStaff = true;
                            isAdmin = true; // Assume staff with no role are admins
                        }
                        
                        // Debug: Log roles for this user
                        System.Diagnostics.Debug.WriteLine($"User: {App.CurrentUser.Username}, Roles: {string.Join(", ", roles)}, isStaff: {isStaff}, isAdmin: {isAdmin}");
                    }
                }

                // Show/hide emulation status bar
                if (App.IsEmulated)
                {
                    pnlEmulationStatus.Visibility = Visibility.Visible;
                }
                else
                {
                    pnlEmulationStatus.Visibility = Visibility.Collapsed;
                }

                if (App.CurrentUser == null)
                {
                    // === GUEST MODE ===
                    this.Title = "Aqua Park - Host";
                    txtUserStatus.Text = "Prohlížíte jako host (zakupte si vstupenku nebo se zaregistrujte)";

                    // Guests see public options: Tickets, Pools, Slides, Bookings (for purchases)
                    btnTickets.Visibility = Visibility.Visible;
                    btnPools.Visibility = Visibility.Visible;
                    btnSlides.Visibility = Visibility.Visible;
                    btnBookings.Visibility = Visibility.Visible;  // For guest to purchase/book

                    // Hide ALL staff/management modules
                    pnlAdminModules.Visibility = Visibility.Collapsed;
                    
                    btnLogin.Content = "Přihlásit se / Registrace";
                }
                else if (!isStaff)
                {
                    // === REGISTERED VISITOR (NON-STAFF) MODE ===
                    this.Title = $"Aqua Park - Návštěvník: {App.CurrentUser.Username}";
                    txtUserStatus.Text = $"Návštěvník: {App.CurrentUser.Username}";

                    // Visitors see: Tickets, Pools, Slides, Bookings (their own reservations and purchases)
                    btnTickets.Visibility = Visibility.Visible;
                    btnPools.Visibility = Visibility.Visible;
                    btnSlides.Visibility = Visibility.Visible;
                    btnBookings.Visibility = Visibility.Visible;

                    // Hide ALL staff/management modules
                    pnlAdminModules.Visibility = Visibility.Collapsed;

                    btnLogin.Content = "Odhlásit se";
                }
                else
                {
                    // === STAFF/ADMIN MODE ===
                    this.Title = $"Aqua Park Manager - Zaměstnanec: {App.CurrentUser.Username}" + (isAdmin ? " (ADMIN)" : " (STAFF)");
                    txtUserStatus.Text = $"Zaměstnanec: {App.CurrentUser.Username}" + (isAdmin ? " (ADMIN)" : "");

                    // Staff sees everything
                    btnTickets.Visibility = Visibility.Visible;
                    btnPools.Visibility = Visibility.Visible;
                    btnSlides.Visibility = Visibility.Visible;
                    btnBookings.Visibility = Visibility.Visible;

                    // Show staff management modules
                    pnlAdminModules.Visibility = Visibility.Visible;

                    // Admin-only buttons
                    btnUsers.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

                    // DB Info available to staff
                    btnDbInfo.Visibility = Visibility.Visible;

                    btnLogin.Content = "Odhlásit se";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při nastavování rolí: {ex.Message}\n\n{ex.StackTrace}", "Chyba rolí");
                System.Diagnostics.Debug.WriteLine($"ApplyRoles Error: {ex}");
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                var login = new LoginWindow();
                if (login.ShowDialog() == true)
                {
                    // Refresh po přihlášení
                    var newMain = new MainWindow();
                    Application.Current.MainWindow = newMain;
                    newMain.Show();
                    this.Close();
                }
            }
            else
            {
                App.CurrentUser = null;
                var newMain = new MainWindow();
                Application.Current.MainWindow = newMain;
                newMain.Show();
                this.Close();
            }
        }

        // Navigace
        private void BtnTickets_Click(object sender, RoutedEventArgs e) => new TicketsManagementWindow().ShowDialog();
        private void BtnPools_Click(object sender, RoutedEventArgs e) => new PoolsManagementWindow().ShowDialog();
        private void BtnSlides_Click(object sender, RoutedEventArgs e) => new SlidesManagementWindow().ShowDialog();
        
        private void BtnBookings_Click(object sender, RoutedEventArgs e)
        {
            // All users (guests and members) use the friendly booking window
            // Only staff use the management window
            bool isStaff = false;
            
            if (App.CurrentUser != null)
            {
                using (var ctx = new AquaParkContext())
                {
                    var roles = ctx.UserRoles.Include(ur => ur.Role)
                                   .Where(ur => ur.UserId == App.CurrentUser.UserId)
                                   .Select(ur => ur.Role != null ? ur.Role.RoleName : "")
                                   .ToList();
                    isStaff = roles.Contains("STAFF") || roles.Contains("ADMIN") || roles.Contains("MANAGER") || App.CurrentUser.StaffId.HasValue;
                }
            }

            if (isStaff)
            {
                // Staff sees management window
                new BookingsManagementWindow().ShowDialog();
            }
            else
            {
                // Guests and members use friendly booking window
                new GuestBookingWindow().ShowDialog();
            }
        }
        
        private void BtnMembership_Click(object sender, RoutedEventArgs e)
        {
            // Only show membership window for registered users (not guests, not staff)
            if (App.CurrentUser == null)
            {
                MessageBox.Show("Musíte se nejdříve zaregistrovat pro nákup členství.", "Upozornění");
                return;
            }

            new MembershipWindow().ShowDialog();
        }

        // Navigace
        private void BtnStaff_Click(object sender, RoutedEventArgs e) => new StaffManagementWindow().ShowDialog();
        private void BtnVisitors_Click(object sender, RoutedEventArgs e) => new VisitorsManagementWindow().ShowDialog();
        private void BtnMaintenance_Click(object sender, RoutedEventArgs e) => new MaintenanceManagementWindow().ShowDialog();
        private void BtnInventory_Click(object sender, RoutedEventArgs e) => new InventoryManagementWindow().ShowDialog();
        private void BtnCertifications_Click(object sender, RoutedEventArgs e) => new CertificationsManagementWindow().ShowDialog();
        private void BtnScheduling_Click(object sender, RoutedEventArgs e) => new SchedulingWindow().ShowDialog();
        private void BtnReports_Click(object sender, RoutedEventArgs e) => new ReportsAnalyticsWindow().ShowDialog();
        private void BtnMedia_Click(object sender, RoutedEventArgs e) => new MediaManagementWindow().ShowDialog();

        // Nová okna (musí existovat)
        private void BtnUsers_Click(object sender, RoutedEventArgs e) => new UsersManagementWindow().ShowDialog();
        private void BtnDbInfo_Click(object sender, RoutedEventArgs e) => new DatabaseObjectsWindow().ShowDialog();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Only shutdown if this is the last main window
            if (Application.Current.MainWindow == this)
            {
                Application.Current.Shutdown();
            }
        }

        private void BtnReturnToAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.AdminUser != null)
                {
                    App.CurrentUser = App.AdminUser;
                    App.IsEmulated = false;
                    
                    // Refresh UI
                    var newMain = new MainWindow();
                    Application.Current.MainWindow = newMain;
                    newMain.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při návratu na admin: {ex.Message}", "Chyba");
            }
        }
    }
}