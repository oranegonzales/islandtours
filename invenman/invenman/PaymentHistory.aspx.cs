using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace invenman
{
    public partial class PaymentHistory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPayments();
            }
        }

        private void BindPayments()
        {
            gvPayments.DataSource = null;
            gvPayments.DataBind();
            lblError.Text = "";

            string username = Session["Username"] as string;
            string role = Session["Role"] as string;

            if (string.IsNullOrWhiteSpace(username))
            {
                lblError.Text = "You must be logged in to view payments.";
                return;
            }

            bool isStaff = !string.IsNullOrWhiteSpace(role) &&
                           !role.Equals("Client", StringComparison.OrdinalIgnoreCase);

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (isStaff)
                    {
                        cmd.CommandText = @"
SELECT
    p.PaymentID,
    p.PaymentDate,
    c.FirstName + ' ' + c.LastName AS ClientName,
    p.Amount AS AmountJMD,
    p.PaymentMethod,
    p.CurrencyCode
FROM Payments p
INNER JOIN Bookings b ON p.BookingID = b.BookingID
INNER JOIN Clients c ON b.ClientID = c.ClientID
ORDER BY p.PaymentDate DESC";
                    }
                    else
                    {
                        cmd.CommandText = @"
SELECT
    p.PaymentID,
    p.PaymentDate,
    c.FirstName + ' ' + c.LastName AS ClientName,
    p.Amount AS AmountJMD,
    p.PaymentMethod,
    p.CurrencyCode
FROM Payments p
INNER JOIN Bookings b ON p.BookingID = b.BookingID
INNER JOIN Clients c ON b.ClientID = c.ClientID
INNER JOIN Users u ON u.Email = c.Email
WHERE u.Username = @Username
ORDER BY p.PaymentDate DESC";

                        cmd.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = username;
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            lblError.Text = "No payments found.";
                        }

                        gvPayments.DataSource = dt;
                        gvPayments.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to load payments at this time. Detail: " + ex.Message;
            }
        }
    }
}