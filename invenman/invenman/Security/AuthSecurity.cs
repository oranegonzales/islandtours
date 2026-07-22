using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using invenman.Configuration;

namespace invenman.Security
{
    internal static class AuthSecurity
    {
        private const string CookieName = "TravelTimeAuth";

        public static void SignIn(HttpContext context, string username, bool persistent)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            DateTime issued = DateTime.UtcNow;
            DateTime expires = persistent ? issued.AddDays(7) : issued.AddHours(8);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                username,
                issued,
                expires,
                persistent,
                string.Empty,
                FormsAuthentication.FormsCookiePath);

            HttpCookie cookie = new HttpCookie(
                CookieName,
                FormsAuthentication.Encrypt(ticket));
            cookie.HttpOnly = true;
            cookie.Secure = context.Request.IsSecureConnection;
            cookie.SameSite = SameSiteMode.Strict;
            if (persistent)
            {
                cookie.Expires = expires.ToLocalTime();
            }
            context.Response.Cookies.Add(cookie);

            if (context.Session != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
            }
        }

        public static bool TryRestoreSession(HttpContext context)
        {
            if (context == null || context.Session == null)
            {
                return false;
            }

            string existingUsername = context.Session["Username"] as string;
            if (!string.IsNullOrWhiteSpace(existingUsername))
            {
                return true;
            }

            HttpCookie cookie = context.Request.Cookies[CookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
            {
                return false;
            }

            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(cookie.Value);
            }
            catch (Exception)
            {
                ExpireCookie(context);
                return false;
            }

            if (ticket == null || ticket.Expired || string.IsNullOrWhiteSpace(ticket.Name))
            {
                ExpireCookie(context);
                return false;
            }

            string role = GetActiveRole(ticket.Name);
            if (string.IsNullOrWhiteSpace(role))
            {
                ExpireCookie(context);
                return false;
            }

            context.Session["Username"] = ticket.Name;
            context.Session["Role"] = role;
            return true;
        }

        public static void SignOut(HttpContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.Session != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
            }

            ExpireCookie(context);
        }

        public static void ExpireCookie(HttpContext context)
        {
            HttpCookie cookie = new HttpCookie(CookieName, string.Empty);
            cookie.HttpOnly = true;
            cookie.Secure = context.Request.IsSecureConnection;
            cookie.SameSite = SameSiteMode.Strict;
            cookie.Expires = DateTime.UtcNow.AddDays(-1);
            context.Response.Cookies.Add(cookie);
        }

        private static string GetActiveRole(string username)
        {
            using (SqlConnection connection =
                new SqlConnection(AppConfiguration.GetConnectionString()))
            using (SqlCommand command = new SqlCommand(
                "SELECT RoleName FROM Users WHERE Username=@Username AND IsActive=1",
                connection))
            {
                command.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = username;
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(value);
            }
        }
    }
}
