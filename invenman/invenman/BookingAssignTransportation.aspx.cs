using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using invenman.Services;
using invenman.Security;

namespace invenman
{
    public partial class BookingAssignTransportation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);
            bool canManage = CanManageTransportation(role);

            if (!isClient && !canManage)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                lblMode.Text = isClient
                    ? "Review pickup details for your upcoming bookings."
                    : "Assign individual vehicles or plan the next 30 days as a fleet.";
                ConfigureUiForRole(isClient);
                BindGrid(isClient);
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

        private static bool CanManageTransportation(string role)
        {
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase);
        }

        private void ConfigureUiForRole(bool isClient)
        {
            if (isClient)
            {
                if (gvBookings.Columns.Count > 0)
                {
                    gvBookings.Columns[0].Visible = false;
                }
                pnlStaffEditor.Visible = false;
                pnlPlanningTools.Visible = false;
            }
            else
            {
                if (gvBookings.Columns.Count > 0)
                {
                    gvBookings.Columns[0].Visible = true;
                }
                pnlStaffEditor.Visible = true;
                pnlPlanningTools.Visible = true;
            }
        }

        private int? GetClientIdForCurrentUser()
        {
            return ClientIdentity.GetClientId(this);
        }

        private void BindGrid(bool isClient)
        {
            if (isClient)
            {
                int? clientId = GetClientIdForCurrentUser();
                if (clientId == null)
                {
                    lblMessage.CssClass = "mb-3 d-block text-warning";
                    lblMessage.Text = "No client profile is linked to your login yet.";
                    gvBookings.DataSource = null;
                    gvBookings.DataBind();
                    return;
                }

                LoadClientBookings(clientId.Value);
            }
            else
            {
                LoadStaffBookings();
            }
        }

        private void LoadStaffBookings()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP (500) b.BookingID, b.TourDate, c.FirstName + ' ' + c.LastName AS ClientName, " +
                "a.Name AS AttractionName, a.Parish, b.PartySize, b.TransportProvider, b.PickupLocation, " +
                "b.PickupDateTime, b.BookingStatus " +
                "FROM Bookings b " +
                "INNER JOIN Clients c ON b.ClientID = c.ClientID " +
                "INNER JOIN Attractions a ON b.AttractionID = a.AttractionID " +
                "WHERE b.TourDate >= @Today " +
                "AND b.BookingStatus IN ('Active','Transport assigned') " +
                "ORDER BY b.TourDate, ClientName",
                conn))
            {
                cmd.Parameters.Add("@Today", SqlDbType.Date).Value = DateTime.Today;

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                {
                    lblMessage.CssClass = "mb-3 d-block text-warning";
                    lblMessage.Text = "There are no upcoming bookings to assign transportation.";
                }
                else
                {
                    lblMessage.CssClass = "mb-3 d-block text-success";
                    lblMessage.Text = "Select a booking to assign or update transportation.";
                }

                gvBookings.DataSource = dt;
                gvBookings.DataBind();
            }
        }

        private void LoadClientBookings(int clientId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP (500) b.BookingID, b.TourDate, c.FirstName + ' ' + c.LastName AS ClientName, " +
                "a.Name AS AttractionName, a.Parish, b.PartySize, b.TransportProvider, b.PickupLocation, " +
                "b.PickupDateTime, b.BookingStatus " +
                "FROM Bookings b " +
                "INNER JOIN Clients c ON b.ClientID = c.ClientID " +
                "INNER JOIN Attractions a ON b.AttractionID = a.AttractionID " +
                "WHERE b.TourDate >= @Today " +
                "AND b.BookingStatus IN ('Active','Transport assigned') " +
                "AND b.ClientID = @ClientID " +
                "ORDER BY b.TourDate",
                conn))
            {
                cmd.Parameters.Add("@Today", SqlDbType.Date).Value = DateTime.Today;
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                {
                    lblMessage.CssClass = "mb-3 d-block text-warning";
                    lblMessage.Text = "You have no upcoming bookings with transportation.";
                }
                else
                {
                    lblMessage.CssClass = "mb-3 d-block text-success";
                    lblMessage.Text = "These are your upcoming bookings and assigned transportation.";
                }

                gvBookings.DataSource = dt;
                gvBookings.DataBind();
            }
        }

        protected void gvBookings_SelectedIndexChanged(object sender, EventArgs e)
        {
            string role = GetCurrentRole();
            if (!CanManageTransportation(role))
            {
                return;
            }

            if (gvBookings.SelectedDataKey == null)
            {
                return;
            }

            int bookingId = Convert.ToInt32(gvBookings.SelectedDataKey.Value);
            hfSelectedBookingId.Value = bookingId.ToString();

            LoadBookingForEdit(bookingId);
        }

        private void LoadBookingForEdit(int bookingId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TransportProvider, PickupLocation, PickupDateTime, TransportNotes " +
                "FROM Bookings WHERE BookingID = @BookingID",
                conn))
            {
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        string provider = rdr["TransportProvider"] as string ?? string.Empty;
                        ddlTransportProvider.ClearSelection();
                        if (!string.IsNullOrEmpty(provider))
                        {
                            var item = ddlTransportProvider.Items.FindByText(provider);
                            if (item != null)
                            {
                                item.Selected = true;
                            }
                            else
                            {
                                ddlTransportProvider.Items.Insert(0, provider);
                                ddlTransportProvider.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            if (ddlTransportProvider.Items.Count > 0)
                            {
                                ddlTransportProvider.SelectedIndex = 0;
                            }
                        }

                        txtPickupLocation.Text = rdr["PickupLocation"] as string ?? string.Empty;

                        if (rdr["PickupDateTime"] != DBNull.Value)
                        {
                            DateTime dt = Convert.ToDateTime(rdr["PickupDateTime"]);
                            txtPickupDate.Text = dt.ToString("yyyy-MM-dd");
                            txtPickupTime.Text = dt.ToString("HH:mm");
                        }
                        else
                        {
                            txtPickupDate.Text = string.Empty;
                            txtPickupTime.Text = string.Empty;
                        }

                        txtNotes.Text = rdr["TransportNotes"] as string ?? string.Empty;
                    }
                }
            }

            lblMessage.CssClass = "mb-3 d-block text-info";
            lblMessage.Text = "Editing transportation for booking " + bookingId.ToString();
        }

        protected void btnSaveAssignment_Click(object sender, EventArgs e)
        {
            string role = GetCurrentRole();
            if (!CanManageTransportation(role))
            {
                return;
            }

            int bookingId;
            if (!int.TryParse(hfSelectedBookingId.Value, out bookingId) || bookingId <= 0)
            {
                lblMessage.CssClass = "mb-3 d-block text-danger";
                lblMessage.Text = "Please select a booking from the list first.";
                return;
            }

            string provider = ddlTransportProvider.SelectedValue;
            string pickupLocation = txtPickupLocation.Text.Trim();
            string datePart = txtPickupDate.Text.Trim();
            string timePart = txtPickupTime.Text.Trim();
            string notes = txtNotes.Text.Trim();

            DateTime pickupDateTime;
            bool hasDateTime = DateTime.TryParseExact(
                datePart + " " + timePart,
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out pickupDateTime);

            if (string.IsNullOrWhiteSpace(provider) ||
                string.IsNullOrWhiteSpace(pickupLocation) ||
                !hasDateTime)
            {
                lblMessage.CssClass = "notice notice-error";
                lblMessage.Text = "Provider, pickup location, date, and time are required.";
                return;
            }

            if (pickupLocation.Length > 200 || notes.Length > 1000)
            {
                lblMessage.CssClass = "notice notice-error";
                lblMessage.Text = "Pickup location or notes exceed the allowed length.";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Bookings SET " +
                "TransportProvider = @TransportProvider, " +
                "PickupLocation = @PickupLocation, " +
                "PickupDateTime = @PickupDateTime, " +
                "TransportNotes = @TransportNotes, " +
                "BookingStatus = @BookingStatus " +
                "WHERE BookingID = @BookingID " +
                "AND BookingStatus IN ('Active','Transport assigned')",
                conn))
            {
                cmd.Parameters.Add("@TransportProvider", SqlDbType.NVarChar, 150).Value = provider;
                cmd.Parameters.Add("@PickupLocation", SqlDbType.NVarChar, 200).Value = pickupLocation;
                cmd.Parameters.Add("@PickupDateTime", SqlDbType.DateTime2).Value = pickupDateTime;

                if (string.IsNullOrEmpty(notes))
                {
                    cmd.Parameters.Add("@TransportNotes", SqlDbType.NVarChar, 1000).Value = DBNull.Value;
                }
                else
                {
                    cmd.Parameters.Add("@TransportNotes", SqlDbType.NVarChar, 1000).Value = notes;
                }

                cmd.Parameters.Add("@BookingStatus", SqlDbType.VarChar, 50).Value = "Transport assigned";
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows != 1)
                {
                    lblMessage.CssClass = "notice notice-error";
                    lblMessage.Text = "This booking changed before the assignment was saved. Refresh and try again.";
                    return;
                }
            }

            AuditLogger.Log(this, "TransportationAssigned", "Booking " + bookingId.ToString(CultureInfo.InvariantCulture));
            lblMessage.CssClass = "notice notice-success";
            lblMessage.Text = "Transportation details saved for booking " + bookingId.ToString();

            string roleNow = GetCurrentRole();
            bool isClientNow = string.Equals(roleNow, "Client", StringComparison.OrdinalIgnoreCase);
            BindGrid(isClientNow);
        }

        protected void btnAutoPlan_Click(object sender, EventArgs e)
        {
            if (!CanManageTransportation(GetCurrentRole()))
            {
                lblMessage.CssClass = "notice notice-error";
                lblMessage.Text = "Staff or administrator access is required.";
                return;
            }

            try
            {
                var service = new TransportationPlanningService();
                TransportationPlanningSummary summary = service.PlanUpcoming(DateTime.Today, 30);

                if (!string.IsNullOrWhiteSpace(summary.Message))
                {
                    lblPlanSummary.CssClass = "notice notice-neutral";
                    lblPlanSummary.Text = summary.Message;
                }
                else
                {
                    lblPlanSummary.CssClass = summary.Unscheduled.Count == 0
                        ? "notice notice-success"
                        : "notice notice-warning";
                    lblPlanSummary.Text =
                        summary.Assigned.ToString(CultureInfo.InvariantCulture) +
                        " of " +
                        summary.Considered.ToString(CultureInfo.InvariantCulture) +
                        " bookings assigned. " +
                        summary.Unscheduled.Count.ToString(CultureInfo.InvariantCulture) +
                        " need manual review. " +
                        summary.WriteConflicts.ToString(CultureInfo.InvariantCulture) +
                        " changed during planning.";
                }

                AuditLogger.Log(
                    this,
                    "TransportationAutoPlanned",
                    "Assigned " + summary.Assigned.ToString(CultureInfo.InvariantCulture) +
                    " of " + summary.Considered.ToString(CultureInfo.InvariantCulture) + " bookings.");
                BindGrid(false);
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "TransportationPlanningFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                lblPlanSummary.CssClass = "notice notice-error";
                lblPlanSummary.Text = "The planning run could not be completed. Check the database migration and try again.";
            }
            catch (InvalidOperationException ex)
            {
                AuditLogger.Log(this, "TransportationPlanningFailed", ex.Message);
                lblPlanSummary.CssClass = "notice notice-error";
                lblPlanSummary.Text = "The planning inputs are not valid. Review vehicle and booking data.";
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            hfSelectedBookingId.Value = string.Empty;
            if (ddlTransportProvider.Items.Count > 0)
            {
                ddlTransportProvider.SelectedIndex = 0;
            }
            txtPickupLocation.Text = string.Empty;
            txtPickupDate.Text = string.Empty;
            txtPickupTime.Text = string.Empty;
            txtNotes.Text = string.Empty;
            lblMessage.CssClass = "mb-3 d-block text-info";
            lblMessage.Text = "Selection cleared. Choose a booking from the list to edit transportation.";
        }
    }
}
