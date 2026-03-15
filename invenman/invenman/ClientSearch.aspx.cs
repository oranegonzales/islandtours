using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace invenman
{
    public partial class ClientSearch : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string term = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(term))
            {
                gvResults.DataSource = null;
                gvResults.DataBind();
                lblMessage.CssClass = "mt-2 d-block text-warning";
                lblMessage.Text = "Enter a search term.";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ClientID, FirstName, LastName, Email, Phone, Country " +
                "FROM Clients " +
                "WHERE FirstName LIKE @Term OR LastName LIKE @Term OR Email LIKE @Term OR Phone LIKE @Term " +
                "ORDER BY LastName, FirstName",
                conn))
            {
                string like = "%" + term + "%";
                cmd.Parameters.Add("@Term", System.Data.SqlDbType.VarChar, 255).Value = like;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        conn.Open();
                        da.Fill(dt);

                        gvResults.DataSource = dt;
                        gvResults.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            lblMessage.CssClass = "mt-2 d-block text-muted";
                            lblMessage.Text = "No clients matched that search.";
                        }
                        else
                        {
                            lblMessage.CssClass = "mt-2 d-block text-muted";
                            lblMessage.Text = "Found " + dt.Rows.Count + " client(s).";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.CssClass = "mt-2 d-block text-danger";
                        lblMessage.Text = "Error searching clients: " + ex.Message;
                    }
                }
            }
        }
    }
}
