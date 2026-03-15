using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class BookingModifyCancel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBookings();
            }

            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);

            if (isClient)
            {
                lblMode.Text = "Client bookings mode (cancel only)";
                HideEditColumn();
            }
            else
            {
                lblMode.Text = "Staff or admin bookings mode (edit and cancel)";
            }
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

        private void LoadBookings()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT b.BookingID, c.FirstName + ' ' + c.LastName AS ClientName, a.Name AS AttractionName, " +
                "b.TourDate, b.TotalAmount, b.PaymentStatus, b.BookingStatus " +
                "FROM Bookings b " +
                "INNER JOIN Clients c ON b.ClientID = c.ClientID " +
                "INNER JOIN Attractions a ON b.AttractionID = a.AttractionID " +
                "WHERE b.BookingStatus <> 'Cancelled' " +
                "ORDER BY b.TourDate DESC, b.BookingID DESC",
                conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();

                try
                {
                    conn.Open();
                    da.Fill(dt);

                    gvBookings.DataSource = dt;
                    gvBookings.DataBind();

                    if (dt.Rows.Count == 0)
                    {
                        lblMessage.CssClass = "mt-2 d-block text-muted";
                        lblMessage.Text = "No bookings found.";
                    }
                    else
                    {
                        lblMessage.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error loading bookings: " + ex.Message;
                }
            }

            string roleAfter = GetCurrentRole();
            bool isClientAfter = string.Equals(roleAfter, "Client", StringComparison.OrdinalIgnoreCase);
            if (isClientAfter)
            {
                HideEditColumn();
            }
        }

        private void HideEditColumn()
        {
            if (gvBookings.Columns.Count >= 8)
            {
                gvBookings.Columns[7].Visible = false;
            }
        }

        protected void gvBookings_RowEditing(object sender, GridViewEditEventArgs e)
        {
            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);

            if (isClient)
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
            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);

            if (isClient)
            {
                e.Cancel = true;
                gvBookings.EditIndex = -1;
                LoadBookings();
                return;
            }

            int bookingId = Convert.ToInt32(gvBookings.DataKeys[e.RowIndex].Value);

            GridViewRow row = gvBookings.Rows[e.RowIndex];

            DropDownList ddlPayment = row.FindControl("ddlPaymentStatusEdit") as DropDownList;
            DropDownList ddlBooking = row.FindControl("ddlBookingStatusEdit") as DropDownList;

            if (ddlPayment == null || ddlBooking == null)
            {
                lblMessage.CssClass = "mt-2 d-block text-danger";
                lblMessage.Text = "Unable to update booking.";
                return;
            }

            string paymentStatus = ddlPayment.SelectedValue;
            string bookingStatus = ddlBooking.SelectedValue;

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Bookings SET PaymentStatus = @PaymentStatus, BookingStatus = @BookingStatus WHERE BookingID = @BookingID",
                conn))
            {
                cmd.Parameters.Add("@PaymentStatus", SqlDbType.VarChar, 50).Value = paymentStatus;
                cmd.Parameters.Add("@BookingStatus", SqlDbType.VarChar, 50).Value = bookingStatus;
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblMessage.CssClass = "mt-2 d-block text-success";
                    lblMessage.Text = "Booking updated.";

                    gvBookings.EditIndex = -1;
                    LoadBookings();
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error updating booking: " + ex.Message;
                }
            }
        }

        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "CancelBooking")
            {
                return;
            }

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            int bookingId = Convert.ToInt32(gvBookings.DataKeys[rowIndex].Value);

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Bookings SET BookingStatus = 'Cancelled' WHERE BookingID = @BookingID",
                conn))
            {
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblMessage.CssClass = "mt-2 d-block text-success";
                    lblMessage.Text = "Booking cancelled.";

                    gvBookings.EditIndex = -1;
                    LoadBookings();
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error cancelling booking: " + ex.Message;
                }
            }
        }
    }
}
