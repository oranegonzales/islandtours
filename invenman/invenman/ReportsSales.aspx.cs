using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;

namespace invenman
{
    public partial class ReportsSales : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlGroupBy.SelectedValue = "Daily";
                txtFromDate.Text = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                ddlPaymentStatus.SelectedValue = "";
                LoadReport();
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void LoadReport()
        {
            pnlError.Visible = false;
            lblError.Text = "";
            lblEmpty.Visible = false;
            lblEmpty.Text = "";

            DateTime fromDate;
            DateTime toDate;

            if (!DateTime.TryParse(txtFromDate.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate) ||
                !DateTime.TryParse(txtToDate.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
            {
                ShowError("Please select valid From and To dates.");
                ResetSummary();
                BindEmpty();
                return;
            }

            if (toDate < fromDate)
            {
                ShowError("To date cannot be earlier than From date.");
                ResetSummary();
                BindEmpty();
                return;
            }

            string groupBy = (ddlGroupBy.SelectedValue ?? "Daily").Trim();
            string status = (ddlPaymentStatus.SelectedValue ?? "").Trim();

            string groupExpr;
            string labelExpr;
            string orderExpr;

            if (groupBy.Equals("Monthly", StringComparison.OrdinalIgnoreCase))
            {
                groupExpr = "DATEFROMPARTS(YEAR(p.PaymentDate), MONTH(p.PaymentDate), 1)";
                labelExpr = "CONVERT(varchar(7), DATEFROMPARTS(YEAR(p.PaymentDate), MONTH(p.PaymentDate), 1), 120)";
                orderExpr = groupExpr;
            }
            else if (groupBy.Equals("Yearly", StringComparison.OrdinalIgnoreCase))
            {
                groupExpr = "YEAR(p.PaymentDate)";
                labelExpr = "CONVERT(varchar(4), YEAR(p.PaymentDate))";
                orderExpr = "YEAR(p.PaymentDate)";
            }
            else
            {
                groupExpr = "CONVERT(date, p.PaymentDate)";
                labelExpr = "CONVERT(varchar(10), CONVERT(date, p.PaymentDate), 120)";
                orderExpr = groupExpr;
                groupBy = "Daily";
            }

            string sql =
                "SELECT " +
                labelExpr + " AS PeriodLabel, " +
                "COUNT(*) AS PaymentsCount, " +
                "SUM(CAST(p.Amount AS decimal(18,2))) AS TotalAmount, " +
                "AVG(CAST(p.Amount AS decimal(18,2))) AS AverageAmount " +
                "FROM Payments p " +
                "LEFT JOIN Bookings b ON b.BookingID = p.BookingID " +
                "WHERE p.PaymentDate >= @FromDate AND p.PaymentDate < DATEADD(day, 1, @ToDate) ";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += "AND ISNULL(b.PaymentStatus,'') = @Status ";
            }

            sql +=
                "GROUP BY " + groupExpr + " " +
                "ORDER BY " + orderExpr + " ASC";

            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;

                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50).Value = status;
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                gvSales.DataSource = dt;
                gvSales.DataBind();

                if (dt.Rows.Count == 0)
                {
                    lblEmpty.Visible = true;
                    lblEmpty.Text = "No results found for the selected filters.";
                    ResetSummary();
                    lblFilterSummary.Text = BuildFilterSummary(fromDate, toDate, groupBy, status);
                    return;
                }

                decimal total = 0m;
                int count = 0;

                foreach (DataRow row in dt.Rows)
                {
                    if (row["TotalAmount"] != DBNull.Value)
                    {
                        total += Convert.ToDecimal(row["TotalAmount"]);
                    }
                    if (row["PaymentsCount"] != DBNull.Value)
                    {
                        count += Convert.ToInt32(row["PaymentsCount"]);
                    }
                }

                decimal avg = 0m;
                if (count > 0)
                {
                    avg = total / count;
                }

                lblTotalSales.Text = total.ToString("N2");
                lblPaymentsCount.Text = count.ToString();
                lblAvgPayment.Text = avg.ToString("N2");
                lblFilterSummary.Text = BuildFilterSummary(fromDate, toDate, groupBy, status);
            }
            catch (Exception ex)
            {
                ShowError("Unable to load sales report. Detail: " + ex.Message);
                ResetSummary();
                BindEmpty();
            }
        }

        private string BuildFilterSummary(DateTime fromDate, DateTime toDate, string groupBy, string status)
        {
            string s = string.IsNullOrWhiteSpace(status) ? "All" : status;
            return fromDate.ToString("yyyy-MM-dd") + " to " + toDate.ToString("yyyy-MM-dd") + " | " + groupBy + " | " + s;
        }

        private void ResetSummary()
        {
            lblTotalSales.Text = "0.00";
            lblPaymentsCount.Text = "0";
            lblAvgPayment.Text = "0.00";
        }

        private void BindEmpty()
        {
            gvSales.DataSource = new DataTable();
            gvSales.DataBind();
        }

        private void ShowError(string msg)
        {
            pnlError.Visible = true;
            lblError.Text = msg;
        }

        private string GetConnStr()
        {
            ConnectionStringSettings a = ConfigurationManager.ConnectionStrings["TravelTime"];
            if (a != null && !string.IsNullOrWhiteSpace(a.ConnectionString))
            {
                return a.ConnectionString;
            }

            ConnectionStringSettings b = ConfigurationManager.ConnectionStrings["TravelTimeDb"];
            if (b != null && !string.IsNullOrWhiteSpace(b.ConnectionString))
            {
                return b.ConnectionString;
            }

            throw new Exception("Missing connection string. Add TravelTime (or TravelTimeDb) to web.config.");
        }
    }
}