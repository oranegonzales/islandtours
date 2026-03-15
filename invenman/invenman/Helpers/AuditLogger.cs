using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace invenman
{
    public static class AuditLogger
    {
        public static void Log(Page page, string actionType, string details)
        {
            if (page == null)
            {
                return;
            }

            string username = page.Session["Username"] as string;
            string role = page.Session["Role"] as string;

            if (string.IsNullOrWhiteSpace(username))
            {
                username = "Guest";
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                role = "Guest";
            }

            string pageUrl = "";
            try
            {
                pageUrl = page.Request.RawUrl ?? "";
            }
            catch
            {
                pageUrl = "";
            }

            WriteLog(username, role, actionType, pageUrl, details);
        }

        public static void LogFromHttpContext(HttpContext context, string actionType, string details)
        {
            if (context == null)
            {
                return;
            }

            string username = context.Session == null ? "" : (context.Session["Username"] as string);
            string role = context.Session == null ? "" : (context.Session["Role"] as string);

            if (string.IsNullOrWhiteSpace(username))
            {
                username = "Guest";
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                role = "Guest";
            }

            string pageUrl = context.Request == null ? "" : (context.Request.RawUrl ?? "");
            WriteLog(username, role, actionType, pageUrl, details);
        }

        private static void WriteLog(string username, string role, string actionType, string pageUrl, string details)
        {
            string connStr = GetConnStr();
            if (string.IsNullOrWhiteSpace(connStr))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(actionType))
            {
                actionType = "System";
            }

            if (details == null)
            {
                details = "";
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO AuditLogs (LogDate, Username, RoleName, ActionType, PageUrl, Details)
VALUES (GETDATE(), @Username, @RoleName, @ActionType, @PageUrl, @Details)", conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@RoleName", role);
                cmd.Parameters.AddWithValue("@ActionType", actionType);
                cmd.Parameters.AddWithValue("@PageUrl", pageUrl ?? "");
                cmd.Parameters.AddWithValue("@Details", details);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static string GetConnStr()
        {
            ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["TravelTime"];
            if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                return cs.ConnectionString;
            }

            cs = ConfigurationManager.ConnectionStrings["TravelTimeDb"];
            if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                return cs.ConnectionString;
            }

            return "";
        }
    }
}
