using System;
using System.Configuration;
using System.Data.SqlClient;

namespace invenman
{
    public partial class ClientAdd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string country = txtCountry.Text.Trim();

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Clients (FirstName, LastName, Email, Phone, Country) VALUES (@FirstName, @LastName, @Email, @Phone, @Country)",
                conn))
            {
                cmd.Parameters.Add("@FirstName", System.Data.SqlDbType.VarChar, 100).Value = firstName;
                cmd.Parameters.Add("@LastName", System.Data.SqlDbType.VarChar, 100).Value = lastName;
                cmd.Parameters.Add("@Email", System.Data.SqlDbType.VarChar, 255).Value = email;
                cmd.Parameters.Add("@Phone", System.Data.SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone;
                cmd.Parameters.Add("@Country", System.Data.SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(country) ? (object)DBNull.Value : country;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    lblMessage.CssClass = "mt-3 d-block text-success";
                    lblMessage.Text = "Client saved successfully.";

                    txtFirstName.Text = "";
                    txtLastName.Text = "";
                    txtEmail.Text = "";
                    txtPhone.Text = "";
                    txtCountry.Text = "";
                }
                catch (Exception ex)
                {
                    lblMessage.CssClass = "mt-3 d-block text-danger";
                    lblMessage.Text = "Error saving client: " + ex.Message;
                }
            }
        }
    }
}
