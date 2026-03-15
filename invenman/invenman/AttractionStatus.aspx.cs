using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class AttractionStatus : System.Web.UI.Page
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
                "SELECT AttractionID, Name, Parish, Category, IsActive FROM Attractions ORDER BY Parish, Name",
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

        protected void gvAttractions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Toggle")
            {
                return;
            }

            int attractionId = Convert.ToInt32(e.CommandArgument);
            ToggleAttraction(attractionId);
            LoadAttractions();
        }

        private void ToggleAttraction(int attractionId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Attractions SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE AttractionID = @AttractionID",
                conn))
            {
                cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    lblMessage.CssClass = "mt-2 d-block text-success";
                    lblMessage.Text = "Attraction status updated.";
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error updating status: " + ex.Message;
                }
            }
        }
    }
}
