using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using invenman.Security;

namespace invenman
{
    public partial class BookingInvoices : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool isClient = IsClient();
                lblPageMode.Text = isClient
                    ? "Invoices linked to your client profile."
                    : "Search up to 500 bookings and open their invoice detail.";
                BindGrid();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        private void BindGrid()
        {
            bool isClient = IsClient();
            int? clientId = isClient ? ClientIdentity.GetClientId(this) : null;
            if (isClient && !clientId.HasValue)
            {
                ShowError("Your account is not linked to a client profile.");
                gvBookings.DataSource = null;
                gvBookings.DataBind();
                return;
            }

            DateTime from;
            DateTime to;
            bool hasFrom = DateTime.TryParseExact(
                txtFromDate.Text,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out from);
            bool hasTo = DateTime.TryParseExact(
                txtToDate.Text,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out to);
            if (hasFrom && hasTo && (from > to || (to - from).TotalDays > 366))
            {
                ShowError("Choose a date range of no more than one year.");
                return;
            }

            string paymentStatus = ddlPaymentStatus.SelectedValue;
            if (!string.IsNullOrEmpty(paymentStatus) &&
                paymentStatus != "Pending" &&
                paymentStatus != "Paid" &&
                paymentStatus != "Cancelled" &&
                paymentStatus != "Partially paid")
            {
                ShowError("Choose a valid payment status.");
                return;
            }

            const string sql = @"
SELECT TOP (500)
       b.BookingID,
       b.TourDate,
       c.FirstName + N' ' + c.LastName AS ClientName,
       a.Name AS AttractionName,
       b.TotalAmount,
       COALESCE((SELECT SUM(p.Amount) FROM dbo.Payments AS p WHERE p.BookingID=b.BookingID), 0) AS PaidToDate,
       b.TotalAmount -
         COALESCE((SELECT SUM(p.Amount) FROM dbo.Payments AS p WHERE p.BookingID=b.BookingID), 0) +
         COALESCE((SELECT SUM(r.RefundAmount)
                   FROM dbo.Refunds AS r
                   INNER JOIN dbo.Payments AS rp ON rp.PaymentID=r.PaymentID
                   WHERE rp.BookingID=b.BookingID), 0) AS Balance,
       b.PaymentStatus,
       b.BookingStatus
FROM dbo.Bookings AS b
INNER JOIN dbo.Clients AS c ON c.ClientID=b.ClientID
INNER JOIN dbo.Attractions AS a ON a.AttractionID=b.AttractionID
WHERE (@ClientID IS NULL OR b.ClientID=@ClientID)
  AND (@FromDate IS NULL OR b.TourDate>=@FromDate)
  AND (@ToDate IS NULL OR b.TourDate<=@ToDate)
  AND (@PaymentStatus=N'' OR b.PaymentStatus=@PaymentStatus)
ORDER BY b.TourDate DESC, b.BookingID DESC;";

            try
            {
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value =
                        clientId.HasValue ? (object)clientId.Value : DBNull.Value;
                    command.Parameters.Add("@FromDate", SqlDbType.Date).Value =
                        hasFrom ? (object)from.Date : DBNull.Value;
                    command.Parameters.Add("@ToDate", SqlDbType.Date).Value =
                        hasTo ? (object)to.Date : DBNull.Value;
                    command.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar, 50).Value = paymentStatus;
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                    gvBookings.DataSource = table;
                    gvBookings.DataBind();
                    lblMessage.CssClass = "notice notice-neutral";
                    lblMessage.Text = table.Rows.Count == 0 ? "No invoices match these filters." : string.Empty;
                }
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "InvoiceListFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                ShowError("Invoices could not be loaded.");
            }
        }

        protected void gvBookings_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ViewInvoice") return;

            int rowIndex;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out rowIndex) ||
                rowIndex < 0 ||
                rowIndex >= gvBookings.Rows.Count)
            {
                ShowError("Select a valid invoice.");
                return;
            }

            int bookingId = Convert.ToInt32(gvBookings.DataKeys[rowIndex].Value);
            Response.Redirect(
                "~/InvoiceDetails.aspx?BookingID=" + bookingId.ToString(CultureInfo.InvariantCulture),
                false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private bool IsClient()
        {
            return string.Equals(Session["Role"] as string, "Client", StringComparison.OrdinalIgnoreCase);
        }

        private void ShowError(string message)
        {
            lblMessage.CssClass = "notice notice-error";
            lblMessage.Text = message;
        }
    }
}
