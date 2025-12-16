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

            using (var ctx = new AquaParkContext())
            {
                using (var trans = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Řešení adresy (Helper logika v C#, protože procedura bere jen ID)
                        var zip = txtZip.Text.Trim(); var city = txtCity.Text.Trim();
                        var postal = ctx.PostalCodes.FirstOrDefault(p => p.Code == zip && p.City == city);
                        if (postal == null)
                        {
                            postal = new PostalCode { Code = zip, City = city, Region = "Nezadáno", Country = "Nezadáno" };
                            ctx.PostalCodes.Add(postal); ctx.SaveChanges();
                        }

                        var street = txtStreet.Text.Trim(); var num = txtHouseNumber.Text.Trim();
                        var addr = ctx.Address.FirstOrDefault(a => a.PostalCodeId == postal.PostalCodeId && a.HouseNumber == num && a.Street == street);
                        if (addr == null)
                        {
                            addr = new Address { PostalCodeId = postal.PostalCodeId, HouseNumber = num, Street = street };
                            ctx.Address.Add(addr); ctx.SaveChanges();
                        }

                        // 2. Volání PROCEDURY SP_REGISTER_VISITOR
                        ctx.Database.ExecuteSqlRaw(
                            "BEGIN SP_REGISTER_VISITOR(:p_fn, :p_ln, :p_em, :p_aid, :p_dob); END;",
                            new OracleParameter("p_fn", txtFirstName.Text),
                            new OracleParameter("p_ln", txtLastName.Text),
                            new OracleParameter("p_em", txtEmail.Text),
                            new OracleParameter("p_aid", addr.AddressId),
                            new OracleParameter("p_dob", dpDob.SelectedDate ?? DateTime.Now)
                        );

                        // 3. Vytvoření USER záznamu (login)
                        // Musíme najít ID právě vytvořeného návštěvníka (podle emailu)
                        var visitor = ctx.Visitors.OrderByDescending(v => v.VisitorId).FirstOrDefault(v => v.Email == txtEmail.Text);
                        if (visitor == null) throw new Exception("Návštěvník nebyl vytvořen.");

                        var user = new User
                        {
                            Username = txtEmail.Text,
                            Email = txtEmail.Text,
                            PasswordHash = txtPassword.Password, // V praxi hashovat
                            VisitorId = visitor.VisitorId,
                            IsActive = "Y"
                        };
                        ctx.Users.Add(user);

                        // 4. Role - Každý začíná jako VISITOR
                        var role = ctx.Roles.FirstOrDefault(r => r.RoleName == "VISITOR");
                        if (role == null) { role = new Role { RoleName = "VISITOR" }; ctx.Roles.Add(role); ctx.SaveChanges(); }

                        ctx.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId });
                        ctx.SaveChanges();

                        trans.Commit();
                        MessageBox.Show("Registrace úspěšná!");
                        Close();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Chyba: " + ex.Message);
                    }
                }
            }
        }
    }
}