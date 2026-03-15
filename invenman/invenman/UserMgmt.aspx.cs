using System;
using System.Data;
using System.Data.SqlClient;

namespace invenman
{
    public partial class UserMgmt : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblStatus.Text = "";
            }
        }

        protected void btnSaveClient_Click(object sender, EventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string country = txtCountry.Text.Trim();

            if (firstName.Length == 0 || lastName.Length == 0 || email.Length == 0)
            {
                lblStatus.Text = "First Name, Last Name, and Email are required.";
                return;
            }

            string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=TravelTime;Integrated Security=True;TrustServerCertificate=True";

            string sql = "INSERT INTO Clients (FirstName, LastName, Email, Phone, Country) VALUES (@FirstName, @LastName, @Email, @Phone, @Country)";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@FirstName", SqlDbType.VarChar, 100).Value = firstName;
                        command.Parameters.Add("@LastName", SqlDbType.VarChar, 100).Value = lastName;
                        command.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = email;

                        if (phone.Length == 0)
                        {
                            command.Parameters.Add("@Phone", SqlDbType.VarChar, 20).Value = DBNull.Value;
                        }
                        else
                        {
                            command.Parameters.Add("@Phone", SqlDbType.VarChar, 20).Value = phone;
                        }

                        if (country.Length == 0)
                        {
                            command.Parameters.Add("@Country", SqlDbType.VarChar, 100).Value = DBNull.Value;
                        }
                        else
                        {
                            command.Parameters.Add("@Country", SqlDbType.VarChar, 100).Value = country;
                        }

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            lblStatus.Text = "Client saved successfully.";
                            txtFirstName.Text = "";
                            txtLastName.Text = "";
                            txtEmail.Text = "";
                            txtPhone.Text = "";
                            txtCountry.Text = "";
                        }
                        else
                        {
                            lblStatus.Text = "No changes were made.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtCountry.Text = "";
            lblStatus.Text = "";
        }
    }
}
