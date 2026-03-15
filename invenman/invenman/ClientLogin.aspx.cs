using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace invenman
{
    public partial class ClientLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = "";
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            string role = ValidateClientAndGetRole(username, password);

            if (string.IsNullOrWhiteSpace(role))
            {
                lblMessage.Text = "Invalid client username or password.";
                return;
            }

            Session["Username"] = username;
            Session["Role"] = role;

            if (chkStaySignedIn.Checked)
            {
                HttpCookie cookie = new HttpCookie("TravelTimeAuth", username);
                cookie.Expires = DateTime.Now.AddDays(7);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
            }

            Response.Redirect("~/Home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private string ValidateClientAndGetRole(string username, string password)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT RoleName FROM Users WHERE Username = @Username AND UserPassword = @Password AND IsActive = 1 AND RoleName = 'Client'",
                conn))
            {
                cmd.Parameters.Add("@Username", System.Data.SqlDbType.VarChar, 100).Value = username;
                cmd.Parameters.Add("@Password", System.Data.SqlDbType.VarChar, 200).Value = password;

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    return "";
                }

                return result.ToString();
            }
        }
    }
}
