using System;
using System.Web;

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
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["TravelTimeAuth"] != null)
            {
                HttpCookie cookie = new HttpCookie("TravelTimeAuth");
                cookie.Value = "";
                cookie.Expires = DateTime.Now.AddDays(-1);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
            }

            Response.Redirect("~/Home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
