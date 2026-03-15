using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace invenman
{
    public partial class ClientList : System.Web.UI.Page
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
                "SELECT ClientID, FirstName, LastName, Email, Phone, Country, DateCreated FROM Clients ORDER BY DateCreated DESC",
                conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                try
                {
                    conn.Open();
                    da.Fill(dt);

                    gvClients.DataSource = dt;
                    gvClients.DataBind();

                    lblMessage.CssClass = "mt-2 d-block text-muted";
                    lblMessage.Text = dt.Rows.Count == 0 ? "No clients found." : "";
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error loading clients: " + ex.Message;
                }
            }
        }

        protected void gvClients_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvClients.PageIndex = e.NewPageIndex;
            LoadClients();
        }
    }
}
