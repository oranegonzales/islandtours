using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;

namespace invenman
{
    public partial class ReportsBestSelling : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFromDate.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                ddlTop.SelectedValue = "10";
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

            int topN = 10;
            int.TryParse(ddlTop.SelectedValue, out topN);
            if (topN <= 0)
            {
                topN = 10;
            }

            string sql =
                "SELECT TOP (@TopN) " +
                "a.Name AS PackageName, " +
                "a.Parish, " +
                "a.Category, " +
                "COUNT(*) AS BookingsCount, " +
                "SUM(CAST(b.TotalAmount AS decimal(18,2))) AS TotalRevenue, " +
                "AVG(CAST(b.TotalAmount AS decimal(18,2))) AS AvgRevenue " +
                "FROM Bookings b " +
                "INNER JOIN Attractions a ON a.AttractionID = b.AttractionID " +
                "WHERE COALESCE(b.TourDate, b.BookingDate) >= @FromDate " +
                "AND COALESCE(b.TourDate, b.BookingDate) < DATEADD(day, 1, @ToDate) " +
                "AND ISNULL(b.BookingStatus,'') <> 'Cancelled' " +
                "GROUP BY a.Name, a.Parish, a.Category " +
                "ORDER BY COUNT(*) DESC, SUM(CAST(b.TotalAmount AS decimal(18,2))) DESC;";

            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@TopN", SqlDbType.Int).Value = topN;
                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                dt.Columns.Add("RankNo", typeof(int));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["RankNo"] = i + 1;
                }

                gvBestSelling.DataSource = dt;
                gvBestSelling.DataBind();

                lblFilterSummary.Text = fromDate.ToString("yyyy-MM-dd") + " to " + toDate.ToString("yyyy-MM-dd") + " | Top " + topN;

                if (dt.Rows.Count == 0)
                {
                    lblEmpty.Visible = true;
                    lblEmpty.Text = "No results found for the selected range.";
                    ResetSummary();
                    return;
                }

                int totalBookings = 0;
                decimal totalRevenue = 0m;

                foreach (DataRow row in dt.Rows)
                {
                    if (row["BookingsCount"] != DBNull.Value)
                    {
                        totalBookings += Convert.ToInt32(row["BookingsCount"]);
                    }
                    if (row["TotalRevenue"] != DBNull.Value)
                    {
                        totalRevenue += Convert.ToDecimal(row["TotalRevenue"]);
                    }
                }

                decimal avg = 0m;
                if (totalBookings > 0)
                {
                    avg = totalRevenue / totalBookings;
                }

                lblTotalBookings.Text = totalBookings.ToString();
                lblTotalRevenue.Text = totalRevenue.ToString("N2");
                lblAvgBooking.Text = avg.ToString("N2");
            }
            catch (Exception ex)
            {
                ShowError("Unable to load best selling tour packages. Detail: " + ex.Message);
                ResetSummary();
                BindEmpty();
            }
        }

        private void ResetSummary()
        {
            lblTotalBookings.Text = "0";
            lblTotalRevenue.Text = "0.00";
            lblAvgBooking.Text = "0.00";
        }

        private void BindEmpty()
        {
            gvBestSelling.DataSource = new DataTable();
            gvBestSelling.DataBind();
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