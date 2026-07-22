using System;
using invenman.Security;

namespace invenman
{
    public partial class ClientLogin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = string.Empty;
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
            string role = UserAuthentication.AuthenticateClient(username, password);

            if (string.IsNullOrWhiteSpace(role))
            {
                lblMessage.Text = "Invalid client username or password.";
                return;
            }

            AuthSecurity.SignIn(Context, username, chkStaySignedIn.Checked);

            Response.Redirect("~/Home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
