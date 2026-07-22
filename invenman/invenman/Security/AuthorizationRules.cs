using System;
using System.Collections.Generic;
using System.Web;

namespace invenman.Security
{
    internal static class AuthorizationRules
    {
        private static readonly HashSet<string> PublicPages =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                string.Empty,
                "default.aspx",
                "home.aspx",
                "login.aspx",
                "clientlogin.aspx",
                "publicattractions.aspx",
                "attractionservice.asmx"
            };

        private static readonly HashSet<string> AdminPages =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "adminauditlogs.aspx",
                "adminsystemconfig.aspx",
                "adminuserroles.aspx"
            };

        private static readonly HashSet<string> ClientPages =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bookingcreate.aspx",
                "bookingmodifycancel.aspx",
                "bookingupcomingtours.aspx",
                "bookingcalendar.aspx",
                "bookingassigntransportation.aspx",
                "bookinginvoices.aspx",
                "invoicedetails.aspx",
                "paymenthistory.aspx",
                "paymentrecord.aspx",
                "paymentreceipt.aspx",
                "logout.aspx"
            };

        public static bool CanAccess(HttpContext context)
        {
            string page = GetPageName(context);
            if (IsStaticResource(context.Request.Path) || PublicPages.Contains(page))
            {
                return true;
            }

            string username = context.Session == null
                ? null
                : context.Session["Username"] as string;
            string role = context.Session == null
                ? null
                : context.Session["Role"] as string;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            if (AdminPages.Contains(page))
            {
                return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase))
            {
                return ClientPages.Contains(page);
            }

            return IsStaffOrAdmin(role);
        }

        public static bool IsStaffOrAdmin(string role)
        {
            return string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetPageName(HttpContext context)
        {
            string appRelative =
                context.Request.AppRelativeCurrentExecutionFilePath ?? string.Empty;
            string file = VirtualPathUtility.GetFileName(appRelative) ?? string.Empty;
            return file.ToLowerInvariant();
        }

        private static bool IsStaticResource(string path)
        {
            string value = (path ?? string.Empty).ToLowerInvariant();
            return value.Contains("/content/")
                || value.Contains("/scripts/")
                || value.Contains("/images/")
                || value.EndsWith("/favicon.ico")
                || value.Contains("webresource.axd")
                || value.Contains("scriptresource.axd");
        }
    }
}
