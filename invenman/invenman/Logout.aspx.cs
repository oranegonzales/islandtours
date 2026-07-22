using System;
using invenman.Security;

namespace invenman
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblStatus.Text = "Click Confirm Logout to end your session.";
            }
        }

        protected void btnConfirmLogout_Click(object sender, EventArgs e)
        {
            AuthSecurity.SignOut(Context);
            Response.Redirect("~/Home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
