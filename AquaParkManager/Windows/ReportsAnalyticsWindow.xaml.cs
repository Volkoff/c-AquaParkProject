using System;
using System.Linq;
using System.Windows;
using System.Data;
using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client; // Pro OracleParameter

namespace AquaParkManager.Windows
{
    public partial class ReportsAnalyticsWindow : Window
    {
        public ReportsAnalyticsWindow()
        {
            InitializeComponent();
            LoadHierarchy();
            dpRevenueDate.SelectedDate = DateTime.Today;
        }

        private void LoadHierarchy()
        {
            try
            {
                using (var context = new AquaParkContext())
                {
                    // Bod 17: Načtení dat z hierarchického pohledu
                    dgHierarchy.ItemsSource = context.AreaHierarchies.ToList();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Chyba hierarchie: " + ex.Message;
            }
        }

        private void BtnCalcRevenue_Click(object sender, RoutedEventArgs e)
        {
            if (dpRevenueDate.SelectedDate == null) return;

            using (var context = new AquaParkContext())
            {
                try
                {
                    // Bod 5: Volání netriviální procedury SP_DAILY_REVENUE_REPORT
                    // Procedura: SP_DAILY_REVENUE_REPORT(p_report_date IN DATE, o_total_revenue OUT NUMBER)

                    var pDate = new OracleParameter("p_report_date", OracleDbType.Date);
                    pDate.Value = dpRevenueDate.SelectedDate.Value;

                    var pOutRevenue = new OracleParameter("o_total_revenue", OracleDbType.Decimal);
                    pOutRevenue.Direction = ParameterDirection.Output;

                    context.Database.ExecuteSqlRaw(
                        "BEGIN SP_DAILY_REVENUE_REPORT(:p_report_date, :o_total_revenue); END;",
                        pDate, pOutRevenue
                    );

                    // Převedení OracleDecimal na string/decimal
                    string result = pOutRevenue.Value?.ToString() ?? "0";
                    txtRevenueResult.Text = $"{result} Kč";
                    lblStatus.Text = "Tržba vypočtena databází.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chyba procedury: " + ex.Message);
                }
            }
        }
    }
}