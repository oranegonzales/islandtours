using System;
using System.Web;
using System.Web.UI;
using invenman.Security;

namespace invenman
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RestoreSessionFromCookieIfNeeded();

            bool loggedIn = IsLoggedIn();
            string role = GetRoleFromSession();

            pnlLoggedOut.Visible = !loggedIn;
            pnlLoggedIn.Visible = loggedIn;

            lblAuthStatus.Text = loggedIn
                ? "Signed in: " + (Session["Username"] as string) + " (" + role + ")"
                : "Guest mode (Home only)";

            pnlStaffMenuItems.Visible = loggedIn && !role.Equals("Client", StringComparison.OrdinalIgnoreCase);
            pnlClientMenuItems.Visible = loggedIn && role.Equals("Client", StringComparison.OrdinalIgnoreCase);
            pnlAdminMenu.Visible = loggedIn && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            EnforceAccessRules(loggedIn, role);
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

        private void EnforceAccessRules(bool loggedIn, string role)
        {
            string key = GetCurrentPageKey();

            bool isHome = key == string.Empty || key == "default" || key == "default.aspx" || key == "home" || key == "home.aspx";
            bool isStaffLogin = key == "login" || key == "login.aspx";
            bool isClientLogin = key == "clientlogin" || key == "clientlogin.aspx";
            bool isLogout = key == "logout" || key == "logout.aspx";

            if (!loggedIn)
            {
                if (!(isHome || isStaffLogin || isClientLogin))
                {
                    SafeRedirect("~/Login.aspx");
                    return;
                }
            }

            if (loggedIn && role.Equals("Client", StringComparison.OrdinalIgnoreCase))
            {
                bool allowBookingCreate = key == "bookingcreate" || key == "bookingcreate.aspx";
                bool allowBookingModifyCancel = key == "bookingmodifycancel" || key == "bookingmodifycancel.aspx";
                bool allowBookingUpcomingTours = key == "bookingupcomingtours" || key == "bookingupcomingtours.aspx";
                bool allowBookingCalendar = key == "bookingcalendar" || key == "bookingcalendar.aspx";
                bool allowBookingAssignTransportation = key == "bookingassigntransportation" || key == "bookingassigntransportation.aspx";
                bool allowBookingInvoices = key == "bookinginvoices" || key == "bookinginvoices.aspx";
                bool allowPaymentRecord = key == "paymentrecord" || key == "paymentrecord.aspx";
                bool allowPaymentHistory = key == "paymenthistory" || key == "paymenthistory.aspx";

                bool allowed = isHome || isLogout ||
                               allowBookingCreate ||
                               allowBookingModifyCancel ||
                               allowBookingUpcomingTours ||
                               allowBookingCalendar ||
                               allowBookingAssignTransportation ||
                               allowBookingInvoices ||
                               allowPaymentRecord ||
                               allowPaymentHistory;

                if (!allowed)
                {
                    SafeRedirect("~/Home.aspx");
                    return;
                }
            }

            if (loggedIn && (isStaffLogin || isClientLogin))
            {
                SafeRedirect("~/Home.aspx");
            }
        }

        private string GetCurrentPageKey()
        {
            string appRel = Request.AppRelativeCurrentExecutionFilePath ?? string.Empty;
            string file = VirtualPathUtility.GetFileName(appRel) ?? string.Empty;
            string path = (Request.Path ?? string.Empty).Trim();

            string key = file;

            if (string.IsNullOrWhiteSpace(key))
            {
                key = path.Trim('/');
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            int slash = key.LastIndexOf('/');
            if (slash >= 0)
            {
                key = key.Substring(slash + 1);
            }

            return key.ToLowerInvariant();
        }

        private void SafeRedirect(string url)
        {
            string current = (Request.Path ?? string.Empty).TrimEnd('/').ToLowerInvariant();
            string target = ResolveUrl(url);
            string targetPath = target.TrimEnd('/').ToLowerInvariant();

            if (current == targetPath)
            {
                return;
            }

            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
