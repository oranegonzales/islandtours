using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using invenman.Security;

namespace invenman
{
    public partial class BookingCreate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isClient = IsClient();
            if (!IsPostBack)
            {
                BindClients(isClient);
                BindAttractions();
                txtTourDate.Text = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                txtPartySize.Text = "1";
            }

            ApplyRoleUi(isClient);
        }

        private bool IsClient()
        {
            return string.Equals(
                Session["Role"] as string,
                "Client",
                StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyRoleUi(bool isClient)
        {
            lblMode.Text = isClient
                ? "Create a booking for your linked client profile."
                : "Create a booking on behalf of a client.";
            ddlClient.Enabled = !isClient;
            txtTotalAmount.Enabled = !isClient;
            rfvTotalAmount.Enabled = !isClient;
            revTotalAmount.Enabled = !isClient;
            ddlPaymentStatus.Enabled = !isClient;
            ddlBookingStatus.Enabled = !isClient;
            lblClientAmountNote.Text = isClient
                ? "The amount is calculated from the attraction price and party size."
                : string.Empty;
        }

        private void BindClients(bool isClient)
        {
            ddlClient.Items.Clear();
            if (isClient)
            {
                int? clientId = ClientIdentity.GetClientId(this);
                if (!clientId.HasValue)
                {
                    ddlClient.Items.Add(new ListItem("No linked client profile", ""));
                    btnSave.Enabled = false;
                    lblMessage.CssClass = "notice notice-error";
                    lblMessage.Text = "Your account is not linked to a client profile. Contact staff before booking.";
                    return;
                }

                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                using (SqlCommand command = new SqlCommand(
                    "SELECT FirstName, LastName FROM dbo.Clients WHERE ClientID=@ClientID",
                    connection))
                {
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId.Value;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            ddlClient.Items.Add(new ListItem(
                                reader.GetString(0) + " " + reader.GetString(1),
                                clientId.Value.ToString(CultureInfo.InvariantCulture)));
                            return;
                        }
                    }
                }

                btnSave.Enabled = false;
                lblMessage.CssClass = "notice notice-error";
                lblMessage.Text = "The linked client profile could not be loaded.";
                return;
            }

            ddlClient.Items.Add(new ListItem("Select a client", ""));
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = new SqlCommand(
                "SELECT ClientID, FirstName, LastName FROM dbo.Clients ORDER BY LastName, FirstName",
                connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ddlClient.Items.Add(new ListItem(
                            reader.GetString(1) + " " + reader.GetString(2),
                            reader.GetInt32(0).ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
        }

        private void BindAttractions()
        {
            ddlAttraction.Items.Clear();
            ddlAttraction.Items.Add(new ListItem("Select an attraction", ""));
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = new SqlCommand(
                "SELECT AttractionID, Name, Parish FROM dbo.Attractions WHERE IsActive=1 ORDER BY Parish, Name",
                connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ddlAttraction.Items.Add(new ListItem(
                            reader.GetString(1) + " · " + reader.GetString(2),
                            reader.GetInt32(0).ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            bool isClient = IsClient();
            int clientId;
            int attractionId;
            int partySize;
            DateTime tourDate;

            if (!int.TryParse(ddlClient.SelectedValue, out clientId) ||
                !int.TryParse(ddlAttraction.SelectedValue, out attractionId) ||
                !int.TryParse(txtPartySize.Text, out partySize) ||
                partySize < 1 ||
                partySize > 50 ||
                !DateTime.TryParseExact(
                    txtTourDate.Text,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out tourDate) ||
                tourDate.Date < DateTime.Today)
            {
                ShowError("Choose a client, attraction, future date, and party size from 1 to 50.");
                return;
            }

            if (isClient)
            {
                int? linkedClientId = ClientIdentity.GetClientId(this);
                if (!linkedClientId.HasValue || linkedClientId.Value != clientId)
                {
                    ShowError("Your client profile could not be verified.");
                    return;
                }
            }

            decimal totalAmount;
            string paymentStatus;
            string bookingStatus;
            if (isClient)
            {
                decimal? basePrice = GetActiveAttractionPrice(attractionId);
                if (!basePrice.HasValue)
                {
                    ShowError("That attraction is no longer available.");
                    return;
                }
                totalAmount = basePrice.Value * partySize;
                paymentStatus = "Pending";
                bookingStatus = "Active";
            }
            else
            {
                if (!decimal.TryParse(
                    txtTotalAmount.Text,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out totalAmount) ||
                    totalAmount < 0 ||
                    !IsAllowedPaymentStatus(ddlPaymentStatus.SelectedValue) ||
                    !IsAllowedBookingStatus(ddlBookingStatus.SelectedValue))
                {
                    ShowError("Review the amount and booking statuses.");
                    return;
                }
                paymentStatus = ddlPaymentStatus.SelectedValue;
                bookingStatus = ddlBookingStatus.SelectedValue;
            }

            const string sql = @"
INSERT INTO dbo.Bookings
    (ClientID, AttractionID, BookingDate, TourDate, PartySize, TotalAmount, PaymentStatus, BookingStatus)
SELECT @ClientID, a.AttractionID, @BookingDate, @TourDate, @PartySize, @TotalAmount, @PaymentStatus, @BookingStatus
FROM dbo.Attractions AS a
WHERE a.AttractionID=@AttractionID AND a.IsActive=1;";

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                    command.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;
                    command.Parameters.Add("@BookingDate", SqlDbType.Date).Value = DateTime.Today;
                    command.Parameters.Add("@TourDate", SqlDbType.Date).Value = tourDate.Date;
                    command.Parameters.Add("@PartySize", SqlDbType.Int).Value = partySize;
                    SqlParameter amount = command.Parameters.Add("@TotalAmount", SqlDbType.Decimal);
                    amount.Precision = 18;
                    amount.Scale = 2;
                    amount.Value = totalAmount;
                    command.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar, 50).Value = paymentStatus;
                    command.Parameters.Add("@BookingStatus", SqlDbType.NVarChar, 50).Value = bookingStatus;
                    connection.Open();

                    if (command.ExecuteNonQuery() != 1)
                    {
                        ShowError("That attraction is no longer available.");
                        return;
                    }
                }

                AuditLogger.Log(this, "BookingCreated", "Client " + clientId.ToString(CultureInfo.InvariantCulture));
                lblMessage.CssClass = "notice notice-success";
                lblMessage.Text = "Booking created.";
                BindClients(isClient);
                BindAttractions();
                txtPartySize.Text = "1";
                txtTotalAmount.Text = string.Empty;
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "BookingCreateFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                ShowError("The booking could not be saved. Try again.");
            }
        }

        private decimal? GetActiveAttractionPrice(int attractionId)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = new SqlCommand(
                "SELECT BasePrice FROM dbo.Attractions WHERE AttractionID=@AttractionID AND IsActive=1",
                connection))
            {
                command.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
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
