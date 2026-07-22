using System;
using System.Web.UI;
using invenman.Security;

namespace invenman
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (Context != null && Context.Session != null)
            {
                Page.ViewStateUserKey = Context.Session.SessionID;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RestoreSessionFromCookieIfNeeded();

            bool loggedIn = IsLoggedIn();
            string role = GetRoleFromSession();

            pnlLoggedOut.Visible = !loggedIn;
            pnlLoggedIn.Visible = loggedIn;

            lblAuthStatus.Text = loggedIn
                ? (Session["Username"] as string) + " · " + role
                : "Not signed in";

            pnlStaffMenuItems.Visible = loggedIn && !role.Equals("Client", StringComparison.OrdinalIgnoreCase);
            pnlClientMenuItems.Visible = loggedIn && role.Equals("Client", StringComparison.OrdinalIgnoreCase);
            pnlAdminMenu.Visible = loggedIn && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLoggedIn()
        {
            string username = Session["Username"] as string;
            return !string.IsNullOrWhiteSpace(username);
        }

        private string GetRoleFromSession()
        {
            string role = Session["Role"] as string;
            if (string.IsNullOrWhiteSpace(role))
            {
                return "Guest";
            }
            return role;
        }

        private void RestoreSessionFromCookieIfNeeded()
        {
            AuthSecurity.TryRestoreSession(Context);
        }

    }
}
