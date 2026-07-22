using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using invenman.Security;

namespace invenman
{
    public partial class PaymentHistory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindPayments();
        }

        private void BindPayments()
        {
            bool isClient = string.Equals(
                Session["Role"] as string,
                "Client",
                StringComparison.OrdinalIgnoreCase);
            int? clientId = isClient ? ClientIdentity.GetClientId(this) : null;
            if (isClient && !clientId.HasValue)
            {
                gvPayments.DataSource = null;
                gvPayments.DataBind();
                lblError.Text = "Your account is not linked to a client profile.";
                return;
            }

            const string sql = @"
SELECT TOP (500)
       p.PaymentID,
       p.PaymentDate,
       c.FirstName + N' ' + c.LastName AS ClientName,
       p.Amount AS AmountJMD,
       p.PaymentMethod,
       p.CurrencyCode
FROM dbo.Payments AS p
INNER JOIN dbo.Bookings AS b ON b.BookingID=p.BookingID
INNER JOIN dbo.Clients AS c ON c.ClientID=b.ClientID
WHERE (@ClientID IS NULL OR b.ClientID=@ClientID)
ORDER BY p.PaymentDate DESC, p.PaymentID DESC;";

            try
            {
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value =
                        clientId.HasValue ? (object)clientId.Value : DBNull.Value;
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                    gvPayments.DataSource = table;
                    gvPayments.DataBind();
                    lblError.Text = table.Rows.Count == 0 ? "No payments found." : string.Empty;
                }
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "PaymentHistoryFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                lblError.Text = "Payment history could not be loaded.";
            }
        }
    }
}
