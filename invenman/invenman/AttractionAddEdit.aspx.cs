using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace invenman
{
    public partial class AttractionAddEdit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string idText = Request.QueryString["id"];
                int id;
                if (!string.IsNullOrWhiteSpace(idText) && int.TryParse(idText, out id))
                {
                    hfAttractionID.Value = id.ToString();
                    lblTitle.Text = "Edit attraction";
                    btnSave.Text = "Update attraction";
                    LoadAttraction(id);
                }
                else
                {
                    lblTitle.Text = "Add attraction";
                    btnSave.Text = "Save attraction";
                    chkIsActive.Checked = true;
                }
            }
        }

        private void LoadAttraction(int attractionId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Name, Description, Parish, Category, BasePrice, ChildPrice, OpenDays, IsActive " +
                "FROM Attractions WHERE AttractionID = @AttractionID",
                conn))
            {
                cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtName.Text = reader["Name"].ToString();
                            txtDescription.Text = reader["Description"].ToString();
                            txtParish.Text = reader["Parish"].ToString();
                            txtCategory.Text = reader["Category"].ToString();
                            txtBasePrice.Text = Convert.ToDecimal(reader["BasePrice"]).ToString("0.##");
                            object child = reader["ChildPrice"];
                            txtChildPrice.Text = child == DBNull.Value ? "" : Convert.ToDecimal(child).ToString("0.##");
                            txtOpenDays.Text = reader["OpenDays"].ToString();
                            chkIsActive.Checked = Convert.ToBoolean(reader["IsActive"]);
                        }
                        else
                        {
                            lblMessage.CssClass = "mt-3 d-block text-danger";
                            lblMessage.Text = "Attraction not found.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-3 d-block text-danger";
                    lblMessage.Text = "Error loading attraction: " + ex.Message;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string name = txtName.Text.Trim();
            string description = txtDescription.Text.Trim();
            string parish = txtParish.Text.Trim();
            string category = txtCategory.Text.Trim();
            string openDays = txtOpenDays.Text.Trim();
            bool isActive = chkIsActive.Checked;

            decimal basePrice;
            if (!decimal.TryParse(txtBasePrice.Text.Trim(), out basePrice))
            {
                lblMessage.CssClass = "mt-3 d-block text-danger";
                lblMessage.Text = "Enter a valid base price.";
                return;
            }

            decimal childPrice;
            bool hasChildPrice = decimal.TryParse(txtChildPrice.Text.Trim(), out childPrice);

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            int attractionId;
            bool isEdit = int.TryParse(hfAttractionID.Value, out attractionId) && attractionId > 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                if (isEdit)
                {
                    cmd.CommandText =
                        "UPDATE Attractions SET Name = @Name, Description = @Description, Parish = @Parish, " +
                        "Category = @Category, BasePrice = @BasePrice, ChildPrice = @ChildPrice, OpenDays = @OpenDays, IsActive = @IsActive " +
                        "WHERE AttractionID = @AttractionID";
                    cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;
                }
                else
                {
                    cmd.CommandText =
                        "INSERT INTO Attractions (Name, Description, Parish, Category, BasePrice, ChildPrice, OpenDays, IsActive) " +
                        "VALUES (@Name, @Description, @Parish, @Category, @BasePrice, @ChildPrice, @OpenDays, @IsActive)";
                }

                cmd.Parameters.Add("@Name", SqlDbType.VarChar, 200).Value = name;
                cmd.Parameters.Add("@Description", SqlDbType.Text).Value = string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description;
                cmd.Parameters.Add("@Parish", SqlDbType.VarChar, 200).Value = parish;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category;
                cmd.Parameters.Add("@BasePrice", SqlDbType.Decimal).Value = basePrice;
                if (hasChildPrice)
                {
                    cmd.Parameters.Add("@ChildPrice", SqlDbType.Decimal).Value = childPrice;
                }
                else
                {
                    cmd.Parameters.Add("@ChildPrice", SqlDbType.Decimal).Value = DBNull.Value;
                }
                cmd.Parameters.Add("@OpenDays", SqlDbType.VarChar, 200).Value = string.IsNullOrWhiteSpace(openDays) ? (object)DBNull.Value : openDays;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblMessage.CssClass = "mt-3 d-block text-success";
                    lblMessage.Text = isEdit ? "Attraction updated successfully." : "Attraction added successfully.";

                    if (!isEdit)
                    {
                        txtName.Text = "";
                        txtDescription.Text = "";
                        txtParish.Text = "";
                        txtCategory.Text = "";
                        txtBasePrice.Text = "";
                        txtChildPrice.Text = "";
                        txtOpenDays.Text = "";
                        chkIsActive.Checked = true;
                        hfAttractionID.Value = "";
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-3 d-block text-danger";
                    lblMessage.Text = "Error saving attraction: " + ex.Message;
                }
            }
        }
    }
}
