using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace invenman
{
    public partial class PaymentRecord : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadBookings();
                ddlPaymentMethod.SelectedValue = "Card";
                ApplyPaymentMethodRules();
            }
        }

        private string GetConnectionString()
        {
            var cs = ConfigurationManager.ConnectionStrings["TravelTime"];
            if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                return cs.ConnectionString;
            }

            cs = ConfigurationManager.ConnectionStrings["TravelTimeDb"];
            if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                return cs.ConnectionString;
            }

            throw new InvalidOperationException("No TravelTime or TravelTimeDb connection string is defined.");
        }

        private void LoadBookings()
        {
            ddlBooking.Items.Clear();
            ddlBooking.Items.Add(new System.Web.UI.WebControls.ListItem("Select a booking", ""));

            string connStr = GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                string sql =
                    "SELECT b.BookingID, a.Name AS AttractionName, b.TourDate, b.TotalAmount " +
                    "FROM Bookings b " +
                    "INNER JOIN Attractions a ON b.AttractionID = a.AttractionID " +
                    "ORDER BY b.TourDate DESC";

                cmd.CommandText = sql;

                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int bookingId = rdr.GetInt32(0);
                        string attractionName = rdr.GetString(1);
                        DateTime tourDate = rdr.GetDateTime(2);
                        decimal totalAmount = rdr.GetDecimal(3);

                        string text = "Booking " + bookingId + " " + attractionName + " on " + tourDate.ToString("yyyy-MM-dd") + " Amount " + totalAmount.ToString("0.00");
                        ddlBooking.Items.Add(new System.Web.UI.WebControls.ListItem(text, bookingId.ToString()));
                    }
                }
            }

            if (ddlBooking.Items.Count > 1)
            {
                ddlBooking.SelectedIndex = 1;
                PrefillAmountFromBooking();
            }
        }

        protected void ddlBooking_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrefillAmountFromBooking();
        }

        private void PrefillAmountFromBooking()
        {
            txtAmount.Text = "";

            if (string.IsNullOrWhiteSpace(ddlBooking.SelectedValue))
            {
                return;
            }

            int bookingId;
            if (!int.TryParse(ddlBooking.SelectedValue, out bookingId))
            {
                return;
            }

            string connStr = GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SELECT TotalAmount FROM Bookings WHERE BookingID = @BookingID", conn))
            {
                cmd.Parameters.Add("@BookingID", System.Data.SqlDbType.Int).Value = bookingId;

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    decimal amount = Convert.ToDecimal(result);
                    txtAmount.Text = amount.ToString("0.00");
                }
            }
        }

        protected void ddlPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyPaymentMethodRules();
        }

        private void ApplyPaymentMethodRules()
        {
            string method = ddlPaymentMethod.SelectedValue;

            if (method == "Card")
            {
                txtAmount.Enabled = true;
                txtAmount.ReadOnly = false;

                rfvAmount.Enabled = true;
                revAmount.Enabled = true;

                pnlCardDetails.Visible = true;
                pnlInstructions.Visible = false;

                btnPay.Enabled = true;
            }
            else if (method == "Cash")
            {
                txtAmount.Enabled = false;
                txtAmount.ReadOnly = true;

                rfvAmount.Enabled = false;
                revAmount.Enabled = false;

                pnlCardDetails.Visible = false;
                pnlInstructions.Visible = true;
                lblInstructions.Text = "Please visit IslandExplore Jamaica Tours office at 10 King Street, Kingston. Bring your booking number and email address. Payment will be taken at the cashier.";

                btnPay.Enabled = false;
            }
            else if (method == "Bank")
            {
                txtAmount.Enabled = false;
                txtAmount.ReadOnly = true;

                rfvAmount.Enabled = false;
                revAmount.Enabled = false;

                pnlCardDetails.Visible = false;
                pnlInstructions.Visible = true;
                lblInstructions.Text = "Please send a bank transfer to Bank of Nova Scotia, Account name IslandExplore Jamaica Tours, Account number 123456789, Branch Half Way Tree. Include your booking number in the transfer reference.";

                btnPay.Enabled = false;
            }
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            if (!Page.IsValid)
            {
                lblMessage.Text = "Please correct the highlighted errors.";
                return;
            }

            if (ddlPaymentMethod.SelectedValue != "Card")
            {
                lblMessage.Text = "Online payment is only available for card payments.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ddlBooking.SelectedValue))
            {
                lblMessage.Text = "Please select a booking.";
                return;
            }

            int bookingId;
            if (!int.TryParse(ddlBooking.SelectedValue, out bookingId))
            {
                lblMessage.Text = "Invalid booking selection.";
                return;
            }

            decimal amount;
            if (!decimal.TryParse(txtAmount.Text.Trim(), out amount) || amount <= 0)
            {
                lblMessage.Text = "Enter a valid payment amount.";
                return;
            }

            string connStr = GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                cmd.CommandText =
                    "INSERT INTO Payments (BookingID, Amount, PaymentMethod, TransactionReference) " +
                    "VALUES (@BookingID, @Amount, @PaymentMethod, @TransactionReference)";

                cmd.Parameters.Add("@BookingID", System.Data.SqlDbType.Int).Value = bookingId;
                cmd.Parameters.Add("@Amount", System.Data.SqlDbType.Decimal).Value = amount;
                cmd.Parameters.Add("@PaymentMethod", System.Data.SqlDbType.VarChar, 50).Value = "Card";
                cmd.Parameters.Add("@TransactionReference", System.Data.SqlDbType.VarChar, 200).Value = Guid.NewGuid().ToString("N");

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            lblSuccess.Text = "Payment approved.";
            lblMessage.Text = "";
        }
    }
}