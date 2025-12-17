using System;
using System.Linq;
using System.Windows;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace AquaParkManager.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow() { InitializeComponent(); }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtCity.Text) || string.IsNullOrWhiteSpace(txtZip.Text))
            {
                MessageBox.Show("Vyplňte prosím všechna pole.");
                return;
            }

            // Verify connection string is set
            if (string.IsNullOrWhiteSpace(App.ConnectionString))
            {
                MessageBox.Show("Chyba: Připojení k databázi není nastaveno. Zkontrolujte, zda jste přihlášeni.");
                return;
            }

            using (var ctx = new AquaParkContext())
            {
                using (var trans = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // Address resolution
                        var zip = txtZip.Text.Trim();
                        var city = txtCity.Text.Trim();
                        var postal = ctx.PostalCodes.FirstOrDefault(p => p.Code == zip && p.City == city);
                        if (postal == null)
                        {
                            postal = new PostalCode { Code = zip, City = city, Region = "Nezadáno", Country = "Nezadáno" };
                            ctx.PostalCodes.Add(postal);
                            ctx.SaveChanges();
                        }

                        var street = txtStreet.Text.Trim();
                        var num = txtHouseNumber.Text.Trim();
                        var addr = ctx.Address.FirstOrDefault(a => a.PostalCodeId == postal.PostalCodeId && a.HouseNumber == num && a.Street == street);
                        if (addr == null)
                        {
                            addr = new Address { PostalCodeId = postal.PostalCodeId, HouseNumber = num, Street = street };
                            ctx.Address.Add(addr);
                            ctx.SaveChanges();
                        }

                        // Call stored procedure — or use direct entity creation if procedure doesn't exist
                        try
                        {
                            ctx.Database.ExecuteSqlRaw(
                                "BEGIN SP_REGISTER_VISITOR(:p_fn, :p_ln, :p_em, :p_aid, :p_dob); END;",
                                new OracleParameter("p_fn", txtFirstName.Text),
                                new OracleParameter("p_ln", txtLastName.Text),
                                new OracleParameter("p_em", txtEmail.Text),
                                new OracleParameter("p_aid", addr.AddressId),
                                new OracleParameter("p_dob", dpDob.SelectedDate ?? DateTime.Now)
                            );
                        }
                        catch (OracleException ex) when (ex.Number == 6550 || ex.Number == 942) // PLS-00306 or ORA-00942 (procedure not found)
                        {
                            // Fallback: create visitor directly if stored procedure doesn't exist
                            var visitor_new = new Visitor
                            {
                                FirstName = txtFirstName.Text,
                                LastName = txtLastName.Text,
                                Email = txtEmail.Text,
                                AddressId = addr.AddressId,
                                DateOfBirth = dpDob.SelectedDate ?? DateTime.Now,
                            };
                            ctx.Visitors.Add(visitor_new);
                            ctx.SaveChanges();
                        }

                        // Create user record
                        var visitor = ctx.Visitors.OrderByDescending(v => v.VisitorId).FirstOrDefault(v => v.Email == txtEmail.Text);
                        if (visitor == null) throw new Exception("Návštěvník nebyl vytvořen.");

                        var user = new User
                        {
                            Username = txtEmail.Text,
                            Email = txtEmail.Text,
                            PasswordHash = txtPassword.Password, // Hash in production
                            VisitorId = visitor.VisitorId,
                            IsActive = "Y"
                        };
                        ctx.Users.Add(user);
                        ctx.SaveChanges(); // Ensure user is saved before adding roles

                        // Role assignment
                        var role = ctx.Roles.FirstOrDefault(r => r.RoleName == "VISITOR");
                        if (role == null)
                        {
                            role = new Role { RoleName = "VISITOR" };
                            ctx.Roles.Add(role);
                            ctx.SaveChanges();
                        }

                        ctx.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
                        ctx.SaveChanges();

                        trans.Commit();
                        MessageBox.Show("Registrace úspěšná! Přihlašuji vás...");
                        
                        // Auto-login after successful registration
                        AutoLoginNewUser(user.Username, txtPassword.Password);
                        
                        Close();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        trans.Rollback();
                        MessageBox.Show("Chyba při aktualizaci databáze: " + dbEx.InnerException?.Message);
                    }
                    catch (OracleException oracleEx)
                    {
                        trans.Rollback();
                        // Log the full error including ORA code for debugging
                        string errorMsg = $"Chyba Oracle ({oracleEx.Number}): {oracleEx.Message}";
                        if (oracleEx.Number == 1017)
                        {
                            errorMsg += "\n\nORA-01017 znamená neplatné přihlašovací údaje.\n" +
                                       "Zkontrolujte, zda je databázové připojení aktivní.";
                        }
                        MessageBox.Show(errorMsg);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Chyba: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Auto-login the newly registered user and open the main application.
        /// </summary>
        private void AutoLoginNewUser(string username, string password)
        {
            try
            {
                using (var ctx = new AquaParkContext())
                {
                    // Verify user credentials
                    var user = ctx.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password && u.IsActive == "Y");
                    if (user == null)
                    {
                        MessageBox.Show("Chyba: Uživatel nemůže být ověřen. Prosím přihlaste se ručně.");
                        return;
                    }

                    // Set the current user in App
                    App.CurrentUser = user;

                    // Create and show the main window
                    var main = new MainWindow();
                    Application.Current.MainWindow = main;
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    main.Show();

                    // Close the register window (and login window if it's still open)
                    var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                    if (loginWindow != null)
                    {
                        loginWindow.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při automatickém přihlášení: " + ex.Message);
            }
        }
    }
}