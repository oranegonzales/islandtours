using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using invenman.Security;

namespace invenman
{
    public partial class BookingUpcomingTours : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            bool isClient = string.Equals(
                Session["Role"] as string,
                "Client",
                StringComparison.OrdinalIgnoreCase);
            lblMode.Text = isClient
                ? "Upcoming tours linked to your client profile."
                : "The next 500 active tours across the operation.";
            BindGrid(isClient);
        }

        private void BindGrid(bool isClient)
        {
            int? clientId = isClient ? ClientIdentity.GetClientId(this) : null;
            if (isClient && !clientId.HasValue)
            {
                gvUpcomingTours.DataSource = null;
                gvUpcomingTours.DataBind();
                lblMessage.CssClass = "notice notice-error";
                lblMessage.Text = "Your account is not linked to a client profile.";
                return;
            }

            const string sql = @"
SELECT TOP (500)
       b.TourDate,
       c.FirstName + N' ' + c.LastName AS ClientName,
       a.Name AS AttractionName,
       a.Parish,
       b.TotalAmount,
       b.PaymentStatus,
       b.BookingStatus
FROM dbo.Bookings AS b
INNER JOIN dbo.Clients AS c ON c.ClientID=b.ClientID
INNER JOIN dbo.Attractions AS a ON a.AttractionID=b.AttractionID
WHERE b.TourDate>=@Today
  AND b.BookingStatus<>N'Cancelled'
  AND (@ClientID IS NULL OR b.ClientID=@ClientID)
ORDER BY b.TourDate, b.BookingID;";

            try
            {
                using (SqlConnection connection = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Today", SqlDbType.Date).Value = DateTime.Today;
                    command.Parameters.Add("@ClientID", SqlDbType.Int).Value =
                        clientId.HasValue ? (object)clientId.Value : DBNull.Value;
                    var table = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                    gvUpcomingTours.DataSource = table;
                    gvUpcomingTours.DataBind();
                    lblMessage.CssClass = "notice notice-neutral";
                    lblMessage.Text = table.Rows.Count == 0 ? "No upcoming tours found." : string.Empty;
                }
            }
            catch (SqlException ex)
            {
                AuditLogger.Log(this, "UpcomingToursFailed", ex.Number.ToString(CultureInfo.InvariantCulture));
                lblMessage.CssClass = "notice notice-error";
                lblMessage.Text = "Upcoming tours could not be loaded.";
            }
        }
    }
}
