using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class ClientBookingHistory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadClients();
            }
        }

        private void LoadClients()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ClientID, FirstName, LastName, Email FROM Clients ORDER BY LastName, FirstName",
                conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();

                try
                {
                    conn.Open();
                    da.Fill(dt);

                    ddlClients.Items.Clear();
                    ddlClients.Items.Add(new ListItem("Select a client", ""));

                    foreach (DataRow row in dt.Rows)
                    {
                        int id = Convert.ToInt32(row["ClientID"]);
                        string fullName = row["FirstName"] + " " + row["LastName"];
                        string email = row["Email"].ToString();
                        string text = fullName + " (" + email + ")";
                        ddlClients.Items.Add(new ListItem(text, id.ToString()));
                    }

                    if (dt.Rows.Count == 0)
                    {
                        lblMessage.CssClass = "mt-2 d-block text-muted";
                        lblMessage.Text = "No clients available.";
                    }
                    else
                    {
                        lblMessage.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error loading clients: " + ex.Message;
                }
            }
        }

        protected void btnLoadHistory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ddlClients.SelectedValue))
            {
                gvHistory.DataSource = null;
                gvHistory.DataBind();
                lblMessage.CssClass = "mt-2 d-block text-warning";
                lblMessage.Text = "Select a client first.";
                return;
            }

            int clientId = int.Parse(ddlClients.SelectedValue);
            LoadHistory(clientId);
        }

        private void LoadHistory(int clientId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT b.BookingID, a.Name AS AttractionName, b.BookingDate, b.TourDate, " +
                "b.TotalAmount, b.PaymentStatus, b.BookingStatus " +
                "FROM Bookings b " +
                "JOIN Attractions a ON b.AttractionID = a.AttractionID " +
                "WHERE b.ClientID = @ClientID " +
                "ORDER BY b.TourDate DESC",
                conn))
            {
                cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientId;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();

                    try
                    {
                        conn.Open();
                        da.Fill(dt);

                        gvHistory.DataSource = dt;
                        gvHistory.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            lblMessage.CssClass = "mt-2 d-block text-muted";
                            lblMessage.Text = "This client has no bookings.";
                        }
                        else
                        {
                            lblMessage.CssClass = "mt-2 d-block text-muted";
                            lblMessage.Text = "Showing " + dt.Rows.Count + " booking(s).";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.CssClass = "mt-2 d-block text-danger";
                        lblMessage.Text = "Error loading booking history: " + ex.Message;
                    }
                }
            }
        }
    }
}
