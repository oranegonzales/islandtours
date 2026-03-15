using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;

namespace invenman
{
    public partial class ReportsAttractionPopularity : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFromDate.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadParishList();
                LoadReport();
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void LoadParishList()
        {
            ddlParish.Items.Clear();
            ddlParish.Items.Add(new System.Web.UI.WebControls.ListItem("-- All Parishes --", ""));

            try
            {
                DataTable dt = new DataTable();

                string sql =
                    "SELECT DISTINCT Parish " +
                    "FROM Attractions " +
                    "WHERE Parish IS NOT NULL AND LTRIM(RTRIM(Parish)) <> '' " +
                    "ORDER BY Parish;";

                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    string parish = row["Parish"] == DBNull.Value ? "" : row["Parish"].ToString();
                    if (!string.IsNullOrWhiteSpace(parish))
                    {
                        ddlParish.Items.Add(new System.Web.UI.WebControls.ListItem(parish, parish));
                    }
                }
            }
            catch
            {
            }
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

            string parishFilter = ddlParish.SelectedValue == null ? "" : ddlParish.SelectedValue.Trim();

            string sql =
                "SELECT " +
                "a.AttractionID, a.Name, a.Parish, a.Category, " +
                "COUNT(*) AS BookingsCount, " +
                "SUM(CAST(b.TotalAmount AS decimal(18,2))) AS TotalRevenue " +
                "FROM Bookings b " +
                "INNER JOIN Attractions a ON a.AttractionID = b.AttractionID " +
                "WHERE a.IsActive = 1 " +
                "AND COALESCE(b.TourDate, b.BookingDate) >= @FromDate " +
                "AND COALESCE(b.TourDate, b.BookingDate) < DATEADD(day, 1, @ToDate) " +
                "AND ISNULL(b.BookingStatus,'') <> 'Cancelled' ";

            if (!string.IsNullOrWhiteSpace(parishFilter))
            {
                sql += "AND a.Parish = @Parish ";
            }

            sql +=
                "GROUP BY a.AttractionID, a.Name, a.Parish, a.Category " +
                "ORDER BY COUNT(*) DESC, SUM(CAST(b.TotalAmount AS decimal(18,2))) DESC;";

            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;

                    if (!string.IsNullOrWhiteSpace(parishFilter))
                    {
                        cmd.Parameters.Add("@Parish", SqlDbType.VarChar, 100).Value = parishFilter;
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                dt.Columns.Add("RankNo", typeof(int));
                dt.Columns.Add("ShareWidth", typeof(int));
                dt.Columns.Add("ShareText", typeof(string));

                int totalBookings = 0;
                decimal totalRevenue = 0m;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    row["RankNo"] = i + 1;

                    int bookings = 0;
                    if (row["BookingsCount"] != DBNull.Value)
                    {
                        bookings = Convert.ToInt32(row["BookingsCount"]);
                    }

                    decimal revenue = 0m;
                    if (row["TotalRevenue"] != DBNull.Value)
                    {
                        revenue = Convert.ToDecimal(row["TotalRevenue"]);
                    }

                    totalBookings += bookings;
                    totalRevenue += revenue;
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    int bookings = 0;
                    if (row["BookingsCount"] != DBNull.Value)
                    {
                        bookings = Convert.ToInt32(row["BookingsCount"]);
                    }

                    decimal pct = 0m;
                    if (totalBookings > 0)
                    {
                        pct = (decimal)bookings * 100m / (decimal)totalBookings;
                    }

                    int width = (int)Math.Round(pct, 0);
                    if (width < 0) width = 0;
                    if (width > 100) width = 100;

                    row["ShareWidth"] = width;
                    row["ShareText"] = pct.ToString("N1") + "% of bookings";
                }

                gvPopularity.DataSource = dt;
                gvPopularity.DataBind();

                string summary = fromDate.ToString("yyyy-MM-dd") + " to " + toDate.ToString("yyyy-MM-dd");
                if (!string.IsNullOrWhiteSpace(parishFilter))
                {
                    summary += " | Parish: " + parishFilter;
                }
                lblFilterSummary.Text = summary;

                if (dt.Rows.Count == 0)
                {
                    lblEmpty.Visible = true;
                    lblEmpty.Text = "No results found for the selected filters.";
                    ResetSummary();
                    return;
                }

                lblTotalBookings.Text = totalBookings.ToString();
                lblTotalRevenue.Text = totalRevenue.ToString("N2");

                DataRow top = dt.Rows[0];
                string topName = top["Name"] == DBNull.Value ? "N/A" : top["Name"].ToString();
                string topParish = top["Parish"] == DBNull.Value ? "" : top["Parish"].ToString();
                string topCat = top["Category"] == DBNull.Value ? "" : top["Category"].ToString();
                string topBookings = top["BookingsCount"] == DBNull.Value ? "0" : top["BookingsCount"].ToString();

                lblTopAttraction.Text = topName;
                lblTopAttractionMeta.Text = topBookings + " bookings" + (string.IsNullOrWhiteSpace(topParish) ? "" : " | " + topParish) + (string.IsNullOrWhiteSpace(topCat) ? "" : " | " + topCat);
            }
            catch (Exception ex)
            {
                ShowError("Unable to load attraction popularity report. Detail: " + ex.Message);
                ResetSummary();
                BindEmpty();
            }
        }

        private void ResetSummary()
        {
            lblTotalBookings.Text = "0";
            lblTotalRevenue.Text = "0.00";
            lblTopAttraction.Text = "N/A";
            lblTopAttractionMeta.Text = "";
        }

        private void BindEmpty()
        {
            gvPopularity.DataSource = new DataTable();
            gvPopularity.DataBind();
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