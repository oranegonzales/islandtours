using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace invenman
{
    public partial class AttractionList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAttractions();
            }
        }

        private void LoadAttractions()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT AttractionID, Name, Parish, Category, BasePrice, ChildPrice, OpenDays, IsActive " +
                "FROM Attractions ORDER BY Parish, Name",
                conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();

                try
                {
                    conn.Open();
                    da.Fill(dt);

                    gvAttractions.DataSource = dt;
                    gvAttractions.DataBind();

                    if (dt.Rows.Count == 0)
                    {
                        lblMessage.CssClass = "mt-2 d-block text-muted";
                        lblMessage.Text = "No attractions found.";
                    }
                    else
                    {
                        lblMessage.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error loading attractions: " + ex.Message;
                }
            }
        }

        protected void gvAttractions_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvAttractions.PageIndex = e.NewPageIndex;
            LoadAttractions();
        }
    }
}
