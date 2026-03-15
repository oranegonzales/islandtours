using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace invenman
{
    public partial class ReportsPaymentPerformance : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsUserLoggedIn())
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string role = (Session["Role"] as string) ?? "";
            if (role.Equals("Client", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            lblMessage.CssClass = "tt-msg";
            lblMessage.Text = "";

            try
            {
                string connStr = GetConnectionString();

                EnsurePaymentsSchema(connStr);

                DateTime fromDate;
                DateTime toDate;
                GetDateRange(out fromDate, out toDate);

                string view = (ddlView.SelectedValue ?? "PeriodSummary").Trim();
                string period = (ddlPeriod.SelectedValue ?? "Monthly").Trim();

                DataTable dt = BuildReport(connStr, view, period, fromDate, toDate);

                gvReport.DataSource = dt;
                gvReport.DataBind();

                lblMessage.Visible = true;
                lblMessage.Text = "Report generated successfully.";
            }
            catch (Exception ex)
            {
                gvReport.DataSource = null;
                gvReport.DataBind();

                lblMessage.Visible = true;
                lblMessage.CssClass = "tt-msg tt-error";
                lblMessage.Text = "Unable to load payment performance report. Detail: " + ex.Message;
            }
        }

        private bool IsUserLoggedIn()
        {
            string username = Session["Username"] as string;
            if (!string.IsNullOrWhiteSpace(username))
            {
                return true;
            }

            if (Request.Cookies["TravelTimeAuth"] != null)
            {
                return true;
            }

            return false;
        }

        private string GetConnectionString()
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

            ConnectionStringSettings c = ConfigurationManager.ConnectionStrings["TravelTimeDbConnectionString"];
            if (c != null && !string.IsNullOrWhiteSpace(c.ConnectionString))
            {
                return c.ConnectionString;
            }

            ConnectionStringSettings d = ConfigurationManager.ConnectionStrings["TravelTimeDb"];
            if (d != null && !string.IsNullOrWhiteSpace(d.ConnectionString))
            {
                return d.ConnectionString;
            }

            throw new Exception("No valid connection string found. Add TravelTime or TravelTimeDb to Web.config.");
        }

        private void EnsurePaymentsSchema(string connStr)
        {
            if (!TableExists(connStr, "Payments"))
            {
                throw new Exception("Payments table not found.");
            }

            if (!ColumnExists(connStr, "Payments", "PaymentDate"))
            {
                throw new Exception("Payments.PaymentDate column not found.");
            }

            if (!ColumnExists(connStr, "Payments", "Amount"))
            {
                throw new Exception("Payments.Amount column not found.");
            }

            if (!ColumnExists(connStr, "Payments", "PaymentMethod"))
            {
                throw new Exception("Payments.PaymentMethod column not found.");
            }
        }

        private void GetDateRange(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = new DateTime(1900, 1, 1);
            toDate = new DateTime(2099, 12, 31);

            string mode = (ddlDateMode.SelectedValue ?? "AllTime").Trim();

            if (!mode.Equals("Range", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            DateTime parsedFrom;
            if (DateTime.TryParse(txtFromDate.Text, out parsedFrom))
            {
                fromDate = parsedFrom.Date;
            }

            DateTime parsedTo;
            if (DateTime.TryParse(txtToDate.Text, out parsedTo))
            {
                toDate = parsedTo.Date.AddDays(1).AddTicks(-1);
            }
        }

        private DataTable BuildReport(string connStr, string view, string period, DateTime fromDate, DateTime toDate)
        {
            if (view.Equals("MethodBreakdown", StringComparison.OrdinalIgnoreCase))
            {
                return GetMethodBreakdown(connStr, fromDate, toDate);
            }

            if (view.Equals("StatusTrend", StringComparison.OrdinalIgnoreCase))
            {
                if (!TableExists(connStr, "Bookings"))
                {
                    throw new Exception("Bookings table not found, cannot run status trend.");
                }

                if (!ColumnExists(connStr, "Bookings", "PaymentStatus"))
                {
                    throw new Exception("Bookings.PaymentStatus column not found, cannot run status trend.");
                }

                if (!ColumnExists(connStr, "Payments", "BookingID"))
                {
                    throw new Exception("Payments.BookingID column not found, cannot link to bookings.");
                }

                return GetStatusTrend(connStr, period, fromDate, toDate);
            }

            return GetPeriodSummary(connStr, period, fromDate, toDate);
        }

        private DataTable GetPeriodSummary(string connStr, string period, DateTime fromDate, DateTime toDate)
        {
            string periodExpr;
            string orderExpr;

            if (period.Equals("Daily", StringComparison.OrdinalIgnoreCase))
            {
                periodExpr = "CONVERT(date, PaymentDate)";
                orderExpr = periodExpr;
            }
            else if (period.Equals("Yearly", StringComparison.OrdinalIgnoreCase))
            {
                periodExpr = "CONVERT(varchar(4), YEAR(PaymentDate))";
                orderExpr = "CONVERT(int, " + periodExpr + ")";
            }
            else
            {
                periodExpr = "CONVERT(varchar(7), PaymentDate, 120)";
                orderExpr = periodExpr;
            }

            string sql =
                "SELECT " +
                periodExpr + " AS Period, " +
                "COUNT(*) AS PaymentCount, " +
                "SUM(Amount) AS TotalAmountJMD, " +
                "AVG(Amount) AS AvgPaymentJMD " +
                "FROM Payments " +
                "WHERE PaymentDate >= @FromDate AND PaymentDate <= @ToDate " +
                "GROUP BY " + periodExpr + " " +
                "ORDER BY " + orderExpr + " ASC";

            return FillDataTable(connStr, sql, fromDate, toDate);
        }

        private DataTable GetMethodBreakdown(string connStr, DateTime fromDate, DateTime toDate)
        {
            string sql =
                "SELECT " +
                "ISNULL(NULLIF(LTRIM(RTRIM(PaymentMethod)), ''), 'Unknown') AS PaymentMethod, " +
                "COUNT(*) AS PaymentCount, " +
                "SUM(Amount) AS TotalAmountJMD, " +
                "AVG(Amount) AS AvgPaymentJMD " +
                "FROM Payments " +
                "WHERE PaymentDate >= @FromDate AND PaymentDate <= @ToDate " +
                "GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(PaymentMethod)), ''), 'Unknown') " +
                "ORDER BY TotalAmountJMD DESC, PaymentCount DESC";

            return FillDataTable(connStr, sql, fromDate, toDate);
        }

        private DataTable GetStatusTrend(string connStr, string period, DateTime fromDate, DateTime toDate)
        {
            string periodExpr;
            string orderExpr;

            if (period.Equals("Daily", StringComparison.OrdinalIgnoreCase))
            {
                periodExpr = "CONVERT(date, p.PaymentDate)";
                orderExpr = periodExpr;
            }
            else if (period.Equals("Yearly", StringComparison.OrdinalIgnoreCase))
            {
                periodExpr = "CONVERT(varchar(4), YEAR(p.PaymentDate))";
                orderExpr = "CONVERT(int, " + periodExpr + ")";
            }
            else
            {
                periodExpr = "CONVERT(varchar(7), p.PaymentDate, 120)";
                orderExpr = periodExpr;
            }

            string sql =
                "SELECT " +
                periodExpr + " AS Period, " +
                "ISNULL(NULLIF(LTRIM(RTRIM(b.PaymentStatus)), ''), 'Unknown') AS PaymentStatus, " +
                "COUNT(*) AS PaymentCount, " +
                "SUM(p.Amount) AS TotalAmountJMD " +
                "FROM Payments p " +
                "INNER JOIN Bookings b ON p.BookingID = b.BookingID " +
                "WHERE p.PaymentDate >= @FromDate AND p.PaymentDate <= @ToDate " +
                "GROUP BY " + periodExpr + ", ISNULL(NULLIF(LTRIM(RTRIM(b.PaymentStatus)), ''), 'Unknown') " +
                "ORDER BY " + orderExpr + " ASC, PaymentStatus ASC";

            return FillDataTable(connStr, sql, fromDate, toDate);
        }

        private DataTable FillDataTable(string connStr, string sql, DateTime fromDate, DateTime toDate)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = fromDate;
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = toDate;

                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private bool TableExists(string connStr, string tableName)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @T",
                conn))
            {
                cmd.Parameters.Add("@T", SqlDbType.VarChar, 128).Value = tableName;

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private bool ColumnExists(string connStr, string tableName, string columnName)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @T AND COLUMN_NAME = @C",
                conn))
            {
                cmd.Parameters.Add("@T", SqlDbType.VarChar, 128).Value = tableName;
                cmd.Parameters.Add("@C", SqlDbType.VarChar, 128).Value = columnName;

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}