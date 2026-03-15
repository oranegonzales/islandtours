using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace invenman
{
    public partial class BookingInvoices : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string role = GetCurrentRole();
                bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);
                lblPageMode.Text = isClient ? "Client invoice view" : "Staff invoice view";
                BindBookings();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindBookings();
        }

        private string GetCurrentRole()
        {
            object r = Session["Role"];
            if (r == null)
            {
                return "Guest";
            }
            string role = r.ToString();
            if (string.IsNullOrWhiteSpace(role))
            {
                return "Guest";
            }
            return role;
        }

        private int? GetClientIdForCurrentUser()
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 ClientID FROM Clients " +
                "WHERE Email = @Identifier " +
                "OR LEFT(Email, CHARINDEX('@', Email + '@') - 1) = @Identifier",
                conn))
            {
                cmd.Parameters.Add("@Identifier", SqlDbType.VarChar, 255).Value = username;

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt32(result);
            }
        }

        private void BindBookings()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;
            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);

            DateTime fromDateValue;
            DateTime toDateValue;
            bool hasFrom = DateTime.TryParse(txtFromDate.Text, out fromDateValue);
            bool hasTo = DateTime.TryParse(txtToDate.Text, out toDateValue);

            string paymentStatus = ddlPaymentStatus.SelectedValue;

            int? clientIdFilter = null;
            if (isClient)
            {
                clientIdFilter = GetClientIdForCurrentUser();
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql =
                    "SELECT b.BookingID, b.TourDate, b.TotalAmount, b.PaymentStatus, b.BookingStatus, " +
                    "c.FirstName + ' ' + c.LastName AS ClientName, " +
                    "a.Name AS AttractionName, " +
                    "ISNULL(SUM(p.Amount), 0) AS PaidToDate, " +
                    "b.TotalAmount - ISNULL(SUM(p.Amount), 0) AS Balance " +
                    "FROM Bookings b " +
                    "INNER JOIN Clients c ON b.ClientID = c.ClientID " +
                    "INNER JOIN Attractions a ON b.AttractionID = a.AttractionID " +
                    "LEFT JOIN Payments p ON b.BookingID = p.BookingID " +
                    "WHERE 1 = 1 ";

                if (isClient && clientIdFilter.HasValue)
                {
                    sql += "AND b.ClientID = @ClientID ";
                }

                if (hasFrom)
                {
                    sql += "AND b.TourDate >= @FromDate ";
                }

                if (hasTo)
                {
                    sql += "AND b.TourDate <= @ToDate ";
                }

                if (!string.IsNullOrWhiteSpace(paymentStatus))
                {
                    sql += "AND b.PaymentStatus = @PaymentStatus ";
                }

                sql +=
                    "GROUP BY b.BookingID, b.TourDate, b.TotalAmount, b.PaymentStatus, b.BookingStatus, " +
                    "c.FirstName, c.LastName, a.Name " +
                    "ORDER BY b.TourDate DESC, b.BookingID DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (isClient && clientIdFilter.HasValue)
                    {
                        cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientIdFilter.Value;
                    }

                    if (hasFrom)
                    {
                        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDateValue.Date;
                    }

                    if (hasTo)
                    {
                        cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDateValue.Date;
                    }

                    if (!string.IsNullOrWhiteSpace(paymentStatus))
                    {
                        cmd.Parameters.Add("@PaymentStatus", SqlDbType.VarChar, 50).Value = paymentStatus;
                    }

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        lblMessage.CssClass = "d-block mb-3 text-warning";
                        lblMessage.Text = "No bookings found for the selected filters.";
                    }
                    else
                    {
                        lblMessage.CssClass = "d-block mb-3 text-success";
                        lblMessage.Text = "Select a booking and click View invoice to open the invoice details.";
                    }

                    gvBookings.DataSource = dt;
                    gvBookings.DataBind();
                }
            }
        }

        protected void gvBookings_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewInvoice")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                if (rowIndex < 0 || rowIndex >= gvBookings.Rows.Count)
                {
                    return;
                }

                int bookingId = Convert.ToInt32(gvBookings.DataKeys[rowIndex].Value);
                string url = "~/InvoiceDetails.aspx?BookingID=" + bookingId.ToString();
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}