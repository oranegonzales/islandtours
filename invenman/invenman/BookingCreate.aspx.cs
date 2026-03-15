using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class BookingCreate : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);

            if (!IsPostBack)
            {
                BindClients(isClient);
                BindAttractions();
            }

            ApplyRoleUi(isClient);
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

        private void ApplyRoleUi(bool isClient)
        {
            if (isClient)
            {
                lblMode.Text = "Client booking mode";

                ddlClient.Enabled = false;

                rfvClient.Enabled = true;

                txtTotalAmount.Enabled = false;
                rfvTotalAmount.Enabled = false;
                revTotalAmount.Enabled = false;

                ddlPaymentStatus.Enabled = false;
                ddlBookingStatus.Enabled = false;

                lblClientAmountNote.Text = "Amount and status will be set automatically.";
            }
            else
            {
                lblMode.Text = "Staff or admin booking mode";

                ddlClient.Enabled = true;

                rfvClient.Enabled = true;

                txtTotalAmount.Enabled = true;
                rfvTotalAmount.Enabled = true;
                revTotalAmount.Enabled = true;

                ddlPaymentStatus.Enabled = true;
                ddlBookingStatus.Enabled = true;

                lblClientAmountNote.Text = "";
            }
        }

        private void BindClients(bool isClient)
        {
            if (isClient)
            {
                bool boundToLoggedIn = TryBindLoggedInClient();
                if (!boundToLoggedIn)
                {
                    BindClientsAll();
                    if (ddlClient.Items.Count > 1)
                    {
                        ddlClient.SelectedIndex = 1;
                    }
                }
            }
            else
            {
                BindClientsAll();
            }
        }

        private bool TryBindLoggedInClient()
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ClientID, FirstName, LastName FROM Clients WHERE Email = @Email",
                conn))
            {
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = username;

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return false;
                    }

                    string name = reader["FirstName"].ToString() + " " + reader["LastName"].ToString();
                    string id = reader["ClientID"].ToString();

                    ddlClient.Items.Clear();
                    ddlClient.Items.Add(new ListItem(name, id));
                    ddlClient.SelectedIndex = 0;

                    return true;
                }
            }
        }

        private void BindClientsAll()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            ddlClient.Items.Clear();
            ddlClient.Items.Add(new ListItem("-- Select client --", ""));

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ClientID, FirstName, LastName FROM Clients ORDER BY LastName, FirstName",
                conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["FirstName"].ToString() + " " + reader["LastName"].ToString();
                        string id = reader["ClientID"].ToString();
                        ddlClient.Items.Add(new ListItem(name, id));
                    }
                }
            }
        }

        private void BindAttractions()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            ddlAttraction.Items.Clear();
            ddlAttraction.Items.Add(new ListItem("-- Select attraction --", ""));

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT AttractionID, Name, Parish FROM Attractions WHERE IsActive = 1 ORDER BY Parish, Name",
                conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["Name"].ToString() + " (" + reader["Parish"].ToString() + ")";
                        string id = reader["AttractionID"].ToString();
                        ddlAttraction.Items.Add(new ListItem(name, id));
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);

            if (!Page.IsValid)
            {
                return;
            }

            int clientId;
            int attractionId;
            DateTime tourDate;
            decimal totalAmount;
            string paymentStatus;
            string bookingStatus;

            if (!int.TryParse(ddlClient.SelectedValue, out clientId))
            {
                lblMessage.CssClass = "mt-3 d-block text-danger";
                lblMessage.Text = "Select a valid client.";
                return;
            }

            if (!int.TryParse(ddlAttraction.SelectedValue, out attractionId))
            {
                lblMessage.CssClass = "mt-3 d-block text-danger";
                lblMessage.Text = "Select a valid attraction.";
                return;
            }

            if (!DateTime.TryParse(txtTourDate.Text.Trim(), out tourDate))
            {
                lblMessage.CssClass = "mt-3 d-block text-danger";
                lblMessage.Text = "Enter a valid tour date.";
                return;
            }

            if (isClient)
            {
                string connStrLocal = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStrLocal))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT BasePrice FROM Attractions WHERE AttractionID = @AttractionID",
                    conn))
                {
                    cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        lblMessage.CssClass = "mt-3 d-block text-danger";
                        lblMessage.Text = "Unable to find attraction pricing.";
                        return;
                    }

                    totalAmount = Convert.ToDecimal(result);
                }

                paymentStatus = "Pending";
                bookingStatus = "Active";
            }
            else
            {
                if (!decimal.TryParse(txtTotalAmount.Text.Trim(), out totalAmount))
                {
                    lblMessage.CssClass = "mt-3 d-block text-danger";
                    lblMessage.Text = "Enter a valid total amount.";
                    return;
                }

                paymentStatus = ddlPaymentStatus.SelectedValue;
                bookingStatus = ddlBookingStatus.SelectedValue;
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Bookings (ClientID, AttractionID, BookingDate, TourDate, TotalAmount, PaymentStatus, BookingStatus) " +
                "VALUES (@ClientID, @AttractionID, @BookingDate, @TourDate, @TotalAmount, @PaymentStatus, @BookingStatus)",
                conn))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;
                cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;
                cmd.Parameters.Add("@BookingDate", SqlDbType.Date).Value = DateTime.Today;
                cmd.Parameters.Add("@TourDate", SqlDbType.Date).Value = tourDate.Date;
                cmd.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = totalAmount;
                cmd.Parameters.Add("@PaymentStatus", SqlDbType.VarChar, 50).Value = paymentStatus;
                cmd.Parameters.Add("@BookingStatus", SqlDbType.VarChar, 50).Value = bookingStatus;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblMessage.CssClass = "mt-3 d-block text-success";
                    lblMessage.Text = "Booking saved successfully.";

                    ddlClient.SelectedIndex = 0;
                    ddlAttraction.SelectedIndex = 0;
                    txtTourDate.Text = "";
                    txtTotalAmount.Text = "";
                    ddlPaymentStatus.SelectedValue = "Pending";
                    ddlBookingStatus.SelectedValue = "Active";
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-3 d-block text-danger";
                    lblMessage.Text = "Error saving booking: " + ex.Message;
                }
            }
        }
    }
}
