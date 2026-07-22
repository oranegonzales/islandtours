using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using invenman.Security;

namespace invenman
{
    public partial class BookingModifyCancel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isClient = IsClient();
            lblMode.Text = isClient
                ? "Review or cancel bookings linked to your client profile."
                : "Review booking state or make an operational correction.";

            if (!IsPostBack) LoadBookings();
            if (isClient) HideEditColumn();
        }

        private bool IsClient()
        {
            return string.Equals(Session["Role"] as string, "Client", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadBookings()
        {
            bool isClient = IsClient();
            int? clientId = isClient ? ClientIdentity.GetClientId(this) : null;
            if (isClient && !clientId.HasValue)
            {
                gvBookings.DataSource = null;
                gvBookings.DataBind();
                ShowError("Your account is not linked to a client profile.");
                return;
            }

            const string sql = @"
SELECT TOP (500)
       b.BookingID,
       c.FirstName + N' ' + c.LastName AS ClientName,
       a.Name AS AttractionName,
       b.TourDate,
       b.TotalAmount,
       b.PaymentStatus,
       b.BookingStatus
FROM dbo.Bookings AS b
INNER JOIN dbo.Clients AS c ON c.ClientID=b.ClientID
INNER JOIN dbo.Attractions AS a ON a.AttractionID=b.AttractionID
WHERE b.BookingStatus<>N'Cancelled'
  AND (@ClientID IS NULL OR b.ClientID=@ClientID)
ORDER BY b.TourDate DESC, b.BookingID DESC;";

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value =
                        clientId.HasValue ? (object)clientId.Value : DBNull.Value;
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                    gvBookings.DataSource = table;
                    gvBookings.DataBind();
                    lblMessage.Text = table.Rows.Count == 0 ? "No current bookings found." : string.Empty;
                    lblMessage.CssClass = "notice notice-neutral";
                }
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "BookingListFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                ShowError("Bookings could not be loaded.");
            }

            if (isClient) HideEditColumn();
        }

        private void HideEditColumn()
        {
            if (gvBookings.Columns.Count > 7) gvBookings.Columns[7].Visible = false;
        }

        protected void gvBookings_RowEditing(object sender, GridViewEditEventArgs e)
        {
            if (!AuthorizationRules.IsStaffOrAdmin(Session["Role"] as string))
            {
                e.Cancel = true;
                return;
            }
            gvBookings.EditIndex = e.NewEditIndex;
            LoadBookings();
        }

        protected void gvBookings_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvBookings.EditIndex = -1;
            LoadBookings();
        }

        protected void gvBookings_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            if (!AuthorizationRules.IsStaffOrAdmin(Session["Role"] as string))
            {
                e.Cancel = true;
                return;
            }

            int bookingId = Convert.ToInt32(gvBookings.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvBookings.Rows[e.RowIndex];
            DropDownList payment = row.FindControl("ddlPaymentStatusEdit") as DropDownList;
            DropDownList booking = row.FindControl("ddlBookingStatusEdit") as DropDownList;
            if (payment == null || booking == null ||
                !IsAllowedPaymentStatus(payment.SelectedValue) ||
                !IsAllowedBookingStatus(booking.SelectedValue))
            {
                ShowError("Choose valid booking statuses.");
                return;
            }

            const string sql = @"
UPDATE dbo.Bookings
SET PaymentStatus=@PaymentStatus, BookingStatus=@BookingStatus
WHERE BookingID=@BookingID AND BookingStatus<>N'Cancelled';";

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar, 50).Value = payment.SelectedValue;
                    command.Parameters.Add("@BookingStatus", SqlDbType.NVarChar, 50).Value = booking.SelectedValue;
                    command.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                    connection.Open();
                    if (command.ExecuteNonQuery() != 1)
                    {
                        ShowError("The booking changed before it could be saved.");
                        return;
                    }
                }

                AuditLogger.Log(this, "BookingUpdated", "Booking " + bookingId.ToString(CultureInfo.InvariantCulture));
                gvBookings.EditIndex = -1;
                LoadBookings();
                lblMessage.CssClass = "notice notice-success";
                lblMessage.Text = "Booking updated.";
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "BookingUpdateFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                ShowError("The booking could not be updated.");
            }
        }

        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "CancelBooking") return;

            int rowIndex;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out rowIndex) ||
                rowIndex < 0 ||
                rowIndex >= gvBookings.Rows.Count)
            {
                ShowError("Select a valid booking.");
                return;
            }

            int bookingId = Convert.ToInt32(gvBookings.DataKeys[rowIndex].Value);
            bool isClient = IsClient();
            int? clientId = isClient ? ClientIdentity.GetClientId(this) : null;
            if (isClient && !clientId.HasValue)
            {
                ShowError("Your client profile could not be verified.");
                return;
            }

            const string sql = @"
UPDATE dbo.Bookings
SET BookingStatus=N'Cancelled'
WHERE BookingID=@BookingID
  AND BookingStatus NOT IN (N'Cancelled', N'Completed')
  AND TourDate>=CAST(GETDATE() AS date)
  AND (@ClientID IS NULL OR ClientID=@ClientID);";

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value =
                        clientId.HasValue ? (object)clientId.Value : DBNull.Value;
                    connection.Open();
                    if (command.ExecuteNonQuery() != 1)
                    {
                        ShowError("This booking cannot be cancelled or no longer belongs to this account.");
                        return;
                    }
                }

                AuditLogger.Log(this, "BookingCancelled", "Booking " + bookingId.ToString(CultureInfo.InvariantCulture));
                gvBookings.EditIndex = -1;
                LoadBookings();
                lblMessage.CssClass = "notice notice-success";
                lblMessage.Text = "Booking cancelled.";
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "BookingCancelFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                ShowError("The booking could not be cancelled.");
            }
        }

        private static bool IsAllowedPaymentStatus(string value)
        {
            return value == "Pending" || value == "Paid" || value == "Cancelled";
        }

        private static bool IsAllowedBookingStatus(string value)
        {
            return value == "Active" || value == "Completed" ||
                   value == "Cancelled" || value == "Transport assigned";
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;
        }

        private void ShowError(string message)
        {
            lblMessage.CssClass = "notice notice-error";
            lblMessage.Text = message;
        }
    }
}
