using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class AttractionPricing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPricing();
            }
        }

        private void LoadPricing()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT AttractionID, Name, Parish, Category, BasePrice, ChildPrice " +
                "FROM Attractions ORDER BY Parish, Name",
                conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();

                try
                {
                    conn.Open();
                    da.Fill(dt);

                    gvPricing.DataSource = dt;
                    gvPricing.DataBind();

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
                    lblMessage.Text = "Error loading pricing: " + ex.Message;
                }
            }
        }

        protected void gvPricing_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPricing.EditIndex = e.NewEditIndex;
            LoadPricing();
        }

        protected void gvPricing_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPricing.EditIndex = -1;
            LoadPricing();
        }

        protected void gvPricing_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int attractionId = Convert.ToInt32(gvPricing.DataKeys[e.RowIndex].Value);

            GridViewRow row = gvPricing.Rows[e.RowIndex];

            string basePriceText = ((TextBox)row.Cells[4].Controls[0]).Text.Trim();
            string childPriceText = ((TextBox)row.Cells[5].Controls[0]).Text.Trim();

            decimal basePrice;
            if (!decimal.TryParse(basePriceText, out basePrice))
            {
                lblMessage.CssClass = "mt-2 d-block text-danger";
                lblMessage.Text = "Enter a valid base price.";
                return;
            }

            decimal childPrice;
            bool hasChildPrice = decimal.TryParse(childPriceText, out childPrice);

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Attractions SET BasePrice = @BasePrice, ChildPrice = @ChildPrice WHERE AttractionID = @AttractionID",
                conn))
            {
                cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;
                cmd.Parameters.Add("@BasePrice", SqlDbType.Decimal).Value = basePrice;
                if (hasChildPrice)
                {
                    cmd.Parameters.Add("@ChildPrice", SqlDbType.Decimal).Value = childPrice;
                }
                else
                {
                    cmd.Parameters.Add("@ChildPrice", SqlDbType.Decimal).Value = DBNull.Value;
                }

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblMessage.CssClass = "mt-2 d-block text-success";
                    lblMessage.Text = "Pricing updated.";

                    gvPricing.EditIndex = -1;
                    LoadPricing();
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-2 d-block text-danger";
                    lblMessage.Text = "Error updating pricing: " + ex.Message;
                }
            }
        }
    }
}
