using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using invenman.Security;

namespace invenman
{
    public partial class PaymentRecord : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool canRecord = AuthorizationRules.IsStaffOrAdmin(Session["Role"] as string);
            lblMode.Text = canRecord
                ? "Record a payment already completed through the office, bank, or card terminal."
                : "Review the payment options for your booking. Online card collection is not enabled.";
            btnPay.Visible = canRecord;
            txtTransactionReference.Enabled = canRecord;

            if (!IsPostBack)
            {
                BindMethods(canRecord);
                LoadBookings();
                ApplyPaymentMethodRules(canRecord);
            }
        }

        private void BindMethods(bool canRecord)
        {
            ddlPaymentMethod.Items.Clear();
            if (canRecord)
            {
                ddlPaymentMethod.Items.Add(new ListItem("Card terminal", "Card terminal"));
                ddlPaymentMethod.Items.Add(new ListItem("Cash at office", "Cash"));
                ddlPaymentMethod.Items.Add(new ListItem("Bank transfer", "Bank transfer"));
            }
            else
            {
                ddlPaymentMethod.Items.Add(new ListItem("Bank transfer", "Bank transfer"));
                ddlPaymentMethod.Items.Add(new ListItem("Cash at office", "Cash"));
            }
        }

        private void LoadBookings()
        {
            ddlBooking.Items.Clear();
            ddlBooking.Items.Add(new ListItem("Select a booking", ""));

            bool isClient = string.Equals(
                Session["Role"] as string,
                "Client",
                StringComparison.OrdinalIgnoreCase);
            int? clientId = isClient ? ClientIdentity.GetClientId(this) : null;
            if (isClient && !clientId.HasValue)
            {
                ShowError("Your account is not linked to a client profile.");
                return;
            }

            const string sql = @"
SELECT TOP (500)
       b.BookingID,
       a.Name,
       b.TourDate,
       b.TotalAmount -
         COALESCE((SELECT SUM(p.Amount) FROM dbo.Payments AS p WHERE p.BookingID=b.BookingID), 0) +
         COALESCE((SELECT SUM(r.RefundAmount)
                   FROM dbo.Refunds AS r
                   INNER JOIN dbo.Payments AS rp ON rp.PaymentID=r.PaymentID
                   WHERE rp.BookingID=b.BookingID), 0) AS Outstanding
FROM dbo.Bookings AS b
INNER JOIN dbo.Attractions AS a ON a.AttractionID=b.AttractionID
WHERE b.BookingStatus<>N'Cancelled'
  AND (@ClientID IS NULL OR b.ClientID=@ClientID)
ORDER BY b.TourDate DESC, b.BookingID DESC;";

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@ClientID", SqlDbType.Int).Value =
                    clientId.HasValue ? (object)clientId.Value : DBNull.Value;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal outstanding = Math.Max(0m, reader.GetDecimal(3));
                        string text =
                            "Booking " + reader.GetInt32(0).ToString(CultureInfo.InvariantCulture) +
                            " · " + reader.GetString(1) +
                            " · " + reader.GetDateTime(2).ToString("dd MMM yyyy", CultureInfo.InvariantCulture) +
                            " · JMD " + outstanding.ToString("N2", CultureInfo.InvariantCulture);
                        ddlBooking.Items.Add(new ListItem(text, reader.GetInt32(0).ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
        }

        protected void ddlBooking_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrefillOutstanding();
        }

        protected void ddlPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyPaymentMethodRules(AuthorizationRules.IsStaffOrAdmin(Session["Role"] as string));
        }

        private void PrefillOutstanding()
        {
            txtAmount.Text = string.Empty;
            int bookingId;
            if (!int.TryParse(ddlBooking.SelectedValue, out bookingId)) return;

            int? clientId = IsClient() ? ClientIdentity.GetClientId(this) : null;
            decimal? outstanding = GetOutstanding(bookingId, clientId, null);
            if (outstanding.HasValue)
            {
                txtAmount.Text = Math.Max(0m, outstanding.Value).ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        private void ApplyPaymentMethodRules(bool canRecord)
        {
            string method = ddlPaymentMethod.SelectedValue;
            pnlInstructions.Visible = true;
            if (method == "Cash")
            {
                lblInstructions.Text = "Pay at the office and quote the booking number shown above.";
            }
            else if (method == "Bank transfer")
            {
                lblInstructions.Text = "Use the bank details supplied by the operations team and include the booking number as the transfer reference.";
            }
            else
            {
                lblInstructions.Text = "Use this only after the external card terminal has approved the transaction. Do not enter card numbers or security codes here.";
            }

            txtAmount.Enabled = canRecord;
            rfvAmount.Enabled = canRecord;
            revAmount.Enabled = canRecord;
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            if (!AuthorizationRules.IsStaffOrAdmin(Session["Role"] as string))
            {
                ShowError("Staff or administrator access is required to record payments.");
                return;
            }
            if (!Page.IsValid) return;

            int bookingId;
            decimal amount;
            string method = ddlPaymentMethod.SelectedValue;
            string reference = txtTransactionReference.Text.Trim();
            if (!int.TryParse(ddlBooking.SelectedValue, out bookingId) ||
                !decimal.TryParse(txtAmount.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) ||
                amount <= 0 ||
                (method != "Cash" && method != "Bank transfer" && method != "Card terminal") ||
                reference.Length > 200 ||
                (method != "Cash" && string.IsNullOrWhiteSpace(reference)))
            {
                ShowError("Choose a booking and enter a valid amount, method, and reference.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        decimal? outstanding = GetOutstanding(bookingId, null, transaction);
                        if (!outstanding.HasValue || amount > outstanding.Value || outstanding.Value <= 0)
                        {
                            transaction.Rollback();
                            ShowError("The amount exceeds the current balance or the booking is unavailable.");
                            return;
                        }

                        using (SqlCommand insert = new SqlCommand(@"
INSERT INTO dbo.Payments
    (BookingID, PaymentDate, Amount, PaymentMethod, TransactionReference, CurrencyCode)
VALUES
    (@BookingID, SYSUTCDATETIME(), @Amount, @PaymentMethod, NULLIF(@Reference, N''), 'JMD');",
                            connection,
                            transaction))
                        {
                            insert.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                            SqlParameter paymentAmount = insert.Parameters.Add("@Amount", SqlDbType.Decimal);
                            paymentAmount.Precision = 18;
                            paymentAmount.Scale = 2;
                            paymentAmount.Value = amount;
                            insert.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = method;
                            insert.Parameters.Add("@Reference", SqlDbType.NVarChar, 200).Value = reference;
                            insert.ExecuteNonQuery();
                        }

                        if (outstanding.Value - amount <= 0)
                        {
                            using (SqlCommand update = new SqlCommand(
                                "UPDATE dbo.Bookings SET PaymentStatus=N'Paid' WHERE BookingID=@BookingID",
                                connection,
                                transaction))
                            {
                                update.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                                update.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                AuditLogger.Log(this, "PaymentRecorded", "Booking " + bookingId.ToString(CultureInfo.InvariantCulture));
                lblMessage.CssClass = "notice notice-success";
                lblMessage.Text = "Payment recorded.";
                txtTransactionReference.Text = string.Empty;
                LoadBookings();
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "PaymentRecordFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                ShowError("The payment could not be recorded.");
            }
        }

        private decimal? GetOutstanding(int bookingId, int? clientId, SqlTransaction transaction)
        {
            const string sql = @"
SELECT b.TotalAmount -
       COALESCE((SELECT SUM(p.Amount) FROM dbo.Payments AS p WHERE p.BookingID=b.BookingID), 0) +
       COALESCE((SELECT SUM(r.RefundAmount)
                 FROM dbo.Refunds AS r
                 INNER JOIN dbo.Payments AS rp ON rp.PaymentID=r.PaymentID
                 WHERE rp.BookingID=b.BookingID), 0)
FROM dbo.Bookings AS b WITH (UPDLOCK, ROWLOCK)
WHERE b.BookingID=@BookingID
  AND b.BookingStatus<>N'Cancelled'
  AND (@ClientID IS NULL OR b.ClientID=@ClientID);";

            SqlConnection connection = transaction == null
                ? new SqlConnection(GetConnectionString())
                : transaction.Connection;
            bool ownsConnection = transaction == null;
            try
            {
                using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                {
                    command.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value =
                        clientId.HasValue ? (object)clientId.Value : DBNull.Value;
                    if (ownsConnection) connection.Open();
                    object value = command.ExecuteScalar();
                    return value == null || value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
                }
            }
            finally
            {
                if (ownsConnection) connection.Dispose();
            }
        }

        private bool IsClient()
        {
            return string.Equals(Session["Role"] as string, "Client", StringComparison.OrdinalIgnoreCase);
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
