using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class PaymentRefunds : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblStatus.Visible = false;
                LoadRefundablePayments();
            }
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;
        }

        private void LoadRefundablePayments()
        {
            ddlPayments.Items.Clear();
            ddlPayments.Items.Add(new ListItem("Select a payment", ""));

            string sql = @"
SELECT
    p.PaymentID,
    p.BookingID,
    p.Amount,
    p.PaymentMethod,
    c.FirstName,
    c.LastName,
    a.Name AS AttractionName
FROM Payments p
INNER JOIN Bookings b ON p.BookingID = b.BookingID
INNER JOIN Clients c ON b.ClientID = c.ClientID
INNER JOIN Attractions a ON b.AttractionID = a.AttractionID
WHERE NOT EXISTS (SELECT 1 FROM Refunds r WHERE r.PaymentID = p.PaymentID)
ORDER BY p.PaymentID DESC";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int paymentId = reader.GetInt32(reader.GetOrdinal("PaymentID"));
                        int bookingId = reader.GetInt32(reader.GetOrdinal("BookingID"));
                        decimal amount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                        string method = reader.GetString(reader.GetOrdinal("PaymentMethod"));
                        string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
                        string lastName = reader.GetString(reader.GetOrdinal("LastName"));
                        string attractionName = reader.GetString(reader.GetOrdinal("AttractionName"));

                        string clientName = firstName + " " + lastName;

                        string text = "Payment " + paymentId
                                      + " | Booking " + bookingId
                                      + " | " + clientName
                                      + " | " + attractionName
                                      + " | " + amount.ToString("0.00") + " JMD"
                                      + " | " + method;

                        ddlPayments.Items.Add(new ListItem(text, paymentId.ToString()));
                    }
                }
            }
        }

        protected void btnProcessRefund_Click(object sender, EventArgs e)
        {
            lblStatus.Visible = false;
            lblStatus.CssClass = "";

            if (string.IsNullOrWhiteSpace(ddlPayments.SelectedValue))
            {
                ShowStatus("Select a payment to refund.", false);
                return;
            }

            decimal refundAmount;
            if (!decimal.TryParse(txtRefundAmount.Text.Trim(), out refundAmount) || refundAmount <= 0)
            {
                ShowStatus("Enter a valid refund amount.", false);
                return;
            }

            int paymentId = int.Parse(ddlPayments.SelectedValue);
            string reason = txtReason.Text.Trim();
            string currentUser = Context.User != null && Context.User.Identity != null
                ? Context.User.Identity.Name
                : "System";

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        decimal originalAmount = 0m;

                        using (SqlCommand cmdAmount = new SqlCommand("SELECT Amount FROM Payments WHERE PaymentID = @PaymentID", conn, tx))
                        {
                            cmdAmount.Parameters.Add("@PaymentID", SqlDbType.Int).Value = paymentId;
                            object result = cmdAmount.ExecuteScalar();
                            if (result == null || result == DBNull.Value)
                            {
                                tx.Rollback();
                                ShowStatus("Cannot find the selected payment.", false);
                                return;
                            }

                            originalAmount = Convert.ToDecimal(result);
                        }

                        if (refundAmount > originalAmount)
                        {
                            tx.Rollback();
                            ShowStatus("Refund amount cannot be greater than the original payment amount.", false);
                            return;
                        }

                        using (SqlCommand cmdInsert = new SqlCommand(
                            "INSERT INTO Refunds (PaymentID, RefundAmount, Reason, RefundDate, ProcessedByUser) VALUES (@PaymentID, @RefundAmount, @Reason, GETDATE(), @ProcessedByUser)", conn, tx))
                        {
                            cmdInsert.Parameters.Add("@PaymentID", SqlDbType.Int).Value = paymentId;
                            cmdInsert.Parameters.Add("@RefundAmount", SqlDbType.Decimal).Value = refundAmount;
                            cmdInsert.Parameters.Add("@Reason", SqlDbType.VarChar, 500).Value = (object)reason ?? DBNull.Value;
                            cmdInsert.Parameters.Add("@ProcessedByUser", SqlDbType.VarChar, 100).Value = currentUser;
                            cmdInsert.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }

                txtRefundAmount.Text = "";
                txtReason.Text = "";
                LoadRefundablePayments();
                ShowStatus("Refund processed successfully.", true);
            }
            catch (Exception ex)
            {
                ShowStatus("An error occurred while processing the refund. Detail: " + ex.Message, false);
            }
        }

        private void ShowStatus(string message, bool success)
        {
            lblStatus.Text = message;
            lblStatus.CssClass = success
                ? "alert alert-success mt-3 d-block"
                : "alert alert-danger mt-3 d-block";
            lblStatus.Visible = true;
        }
    }
}