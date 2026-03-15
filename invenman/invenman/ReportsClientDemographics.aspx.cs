using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace invenman
{
    public partial class ReportsClientDemographics : Page
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
                DateTime fromDate;
                DateTime toDate;
                GetDateRange(out fromDate, out toDate);

                string reportType = ddlReportType.SelectedValue ?? "Country";

                DataTable dt = BuildReport(reportType, fromDate, toDate);

                gvResults.DataSource = dt;
                gvResults.DataBind();

                lblMessage.Visible = true;
                lblMessage.Text = "Report generated successfully.";
            }
            catch (Exception ex)
            {
                gvResults.DataSource = null;
                gvResults.DataBind();

                lblMessage.Visible = true;
                lblMessage.CssClass = "tt-msg tt-error";
                lblMessage.Text = "Unable to load report at this time. Detail: " + ex.Message;
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

        private void GetDateRange(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = new DateTime(1900, 1, 1);
            toDate = new DateTime(2099, 12, 31);

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

        private DataTable BuildReport(string reportType, DateTime fromDate, DateTime toDate)
        {
            string connStr = GetConnectionString();
            string bookingDateColumn = GetExistingBookingDateColumn(connStr);

            if (reportType == "Country")
            {
                return GetCountryBreakdown(connStr, fromDate, toDate);
            }

            if (reportType == "NewClients")
            {
                return GetNewClientsByMonth(connStr, fromDate, toDate);
            }

            if (reportType == "ActiveInactive")
            {
                return GetActiveInactive(connStr, bookingDateColumn, fromDate, toDate);
            }

            if (reportType == "TopSpenders")
            {
                return GetTopSpenders(connStr, bookingDateColumn, fromDate, toDate);
            }

            if (reportType == "MostBookings")
            {
                return GetMostBookings(connStr, bookingDateColumn, fromDate, toDate);
            }

            return GetCountryBreakdown(connStr, fromDate, toDate);
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

            throw new Exception("No valid connection string found. Add TravelTime or TravelTimeDb to Web.config.");
        }

        private string GetExistingBookingDateColumn(string connStr)
        {
            if (ColumnExists(connStr, "Bookings", "TourDate"))
            {
                return "TourDate";
            }

            if (ColumnExists(connStr, "Bookings", "BookingDate"))
            {
                return "BookingDate";
            }

            return "";
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

        private DataTable GetCountryBreakdown(string connStr, DateTime fromDate, DateTime toDate)
        {
            if (!ColumnExists(connStr, "Clients", "Country"))
            {
                throw new Exception("Clients.Country column not found.");
            }

            string sql =
                "SELECT " +
                "ISNULL(NULLIF(LTRIM(RTRIM(Country)), ''), 'Unknown') AS Country, " +
                "COUNT(*) AS ClientCount " +
                "FROM Clients " +
                "WHERE DateCreated >= @FromDate AND DateCreated <= @ToDate " +
                "GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(Country)), ''), 'Unknown') " +
                "ORDER BY ClientCount DESC, Country ASC";

            return FillDataTable(connStr, sql, fromDate, toDate);
        }

        private DataTable GetNewClientsByMonth(string connStr, DateTime fromDate, DateTime toDate)
        {
            string sql =
                "SELECT " +
                "CONVERT(varchar(7), DateCreated, 120) AS YearMonth, " +
                "COUNT(*) AS NewClients " +
                "FROM Clients " +
                "WHERE DateCreated >= @FromDate AND DateCreated <= @ToDate " +
                "GROUP BY CONVERT(varchar(7), DateCreated, 120) " +
                "ORDER BY YearMonth ASC";

            return FillDataTable(connStr, sql, fromDate, toDate);
        }

        private DataTable GetActiveInactive(string connStr, string bookingDateColumn, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(bookingDateColumn))
            {
                throw new Exception("Bookings date column not found. Add TourDate or BookingDate.");
            }

            string sql =
                "SELECT " +
                "CASE WHEN b.ClientID IS NULL THEN 'Inactive' ELSE 'Active' END AS ClientStatus, " +
                "COUNT(*) AS ClientCount " +
                "FROM Clients c " +
                "LEFT JOIN ( " +
                "   SELECT DISTINCT ClientID " +
                "   FROM Bookings " +
                "   WHERE " + bookingDateColumn + " >= @FromDate AND " + bookingDateColumn + " <= @ToDate " +
                ") b ON c.ClientID = b.ClientID " +
                "GROUP BY CASE WHEN b.ClientID IS NULL THEN 'Inactive' ELSE 'Active' END " +
                "ORDER BY ClientStatus DESC";

            return FillDataTable(connStr, sql, fromDate, toDate);
        }

        private DataTable GetTopSpenders(string connStr, string bookingDateColumn, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(bookingDateColumn))
            {
                throw new Exception("Bookings date column not found. Add TourDate or BookingDate.");
            }

            string sql =
                "SELECT TOP 10 " +
                "c.ClientID, " +
                "(c.FirstName + ' ' + c.LastName) AS ClientName, " +
                "c.Email, " +
                "COUNT(b.BookingID) AS BookingCount, " +
                "SUM(b.TotalAmount) AS TotalSpendJMD " +
                "FROM Clients c " +
                "INNER JOIN Bookings b ON c.ClientID = b.ClientID " +
                "WHERE b." + bookingDateColumn + " >= @FromDate AND b." + bookingDateColumn + " <= @ToDate " +
                "GROUP BY c.ClientID, c.FirstName, c.LastName, c.Email " +
                "ORDER BY TotalSpendJMD DESC, BookingCount DESC";

            return FillDataTable(connStr, sql, fromDate, toDate);
        }

        private DataTable GetMostBookings(string connStr, string bookingDateColumn, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(bookingDateColumn))
            {
                throw new Exception("Bookings date column not found. Add TourDate or BookingDate.");
            }

            string sql =
                "SELECT TOP 10 " +
                "c.ClientID, " +
                "(c.FirstName + ' ' + c.LastName) AS ClientName, " +
                "c.Email, " +
                "COUNT(b.BookingID) AS BookingCount, " +
                "SUM(b.TotalAmount) AS TotalSpendJMD " +
                "FROM Clients c " +
                "INNER JOIN Bookings b ON c.ClientID = b.ClientID " +
                "WHERE b." + bookingDateColumn + " >= @FromDate AND b." + bookingDateColumn + " <= @ToDate " +
                "GROUP BY c.ClientID, c.FirstName, c.LastName, c.Email " +
                "ORDER BY BookingCount DESC, TotalSpendJMD DESC";

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
    }
}