using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace invenman
{
    public partial class BookingUpcomingTours : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);

            if (!IsPostBack)
            {
                lblMode.Text = isClient ? "Client view of your upcoming tours" : "Staff or admin view of all upcoming tours";
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

        private int? GetClientIdForCurrentUser()
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ClientID FROM Clients WHERE Email = @Email",
                conn))
            {
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = username;

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    return null;
                }

                return Convert.ToInt32(result);
            }
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
                    gvUpcomingTours.DataSource = null;
                    gvUpcomingTours.DataBind();
                    return;
                }

                LoadUpcomingForClient(clientId.Value);
            }
            else
            {
                LoadUpcomingForStaff();
            }
        }

        private void LoadUpcomingForStaff()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT b.TourDate, c.FirstName + ' ' + c.LastName AS ClientName, " +
                "a.Name AS AttractionName, a.Parish, b.TotalAmount, b.PaymentStatus, b.BookingStatus " +
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
                    lblMessage.Text = "There are no upcoming tours at this time.";
                }
                else
                {
                    lblMessage.CssClass = "mb-3 d-block text-success";
                    lblMessage.Text = "Showing all upcoming tours.";
                }

                gvUpcomingTours.DataSource = dt;
                gvUpcomingTours.DataBind();
            }
        }

        private void LoadUpcomingForClient(int clientId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT b.TourDate, c.FirstName + ' ' + c.LastName AS ClientName, " +
                "a.Name AS AttractionName, a.Parish, b.TotalAmount, b.PaymentStatus, b.BookingStatus " +
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
                    lblMessage.Text = "You have no upcoming tours yet.";
                }
                else
                {
                    lblMessage.CssClass = "mb-3 d-block text-success";
                    lblMessage.Text = "These are your upcoming tours.";
                }

                gvUpcomingTours.DataSource = dt;
                gvUpcomingTours.DataBind();
            }
        }
    }
}
