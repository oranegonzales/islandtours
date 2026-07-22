using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web;
using System.Web.UI;

namespace invenman
{
    public static class AuditLogger
    {
        public static void Log(Page page, string actionType, string details)
        {
            if (page == null) return;
            string username = page.Session["Username"] as string;
            string role = page.Session["Role"] as string;
            string pageUrl = string.Empty;
            try
            {
                pageUrl = page.Request.RawUrl ?? string.Empty;
            }
            catch (HttpException)
            {
                pageUrl = string.Empty;
            }
            WriteLog(username, role, actionType, pageUrl, details);
        }

        public static void LogFromHttpContext(HttpContext context, string actionType, string details)
        {
            if (context == null) return;
            string username = context.Session == null ? null : context.Session["Username"] as string;
            string role = context.Session == null ? null : context.Session["Role"] as string;
            string pageUrl = context.Request == null ? string.Empty : context.Request.RawUrl;
            WriteLog(username, role, actionType, pageUrl, details);
        }

        private static void WriteLog(
            string username,
            string role,
            string actionType,
            string pageUrl,
            string details)
        {
            string connectionString = GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString)) return;

            username = Clean(username, 100, "Guest");
            role = Clean(role, 50, "Guest");
            actionType = Clean(actionType, 100, "System");
            pageUrl = Clean(pageUrl, 500, string.Empty);
            details = Clean(details, 4000, string.Empty);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(@"
INSERT INTO dbo.AuditLogs
    (LogDate, Username, RoleName, ActionType, PageUrl, Details)
VALUES
    (SYSUTCDATETIME(), @Username, @RoleName, @ActionType, @PageUrl, @Details);", connection))
                {
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                    command.Parameters.Add("@RoleName", SqlDbType.NVarChar, 50).Value = role;
                    command.Parameters.Add("@ActionType", SqlDbType.NVarChar, 100).Value = actionType;
                    command.Parameters.Add("@PageUrl", SqlDbType.NVarChar, 500).Value = pageUrl;
                    command.Parameters.Add("@Details", SqlDbType.NVarChar, 4000).Value = details;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                Trace.TraceWarning("Audit write failed with SQL error {0}.", ex.Number);
            }
        }

        private static string Clean(string value, int maximumLength, string fallback)
        {
            string cleaned = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            cleaned = cleaned.Replace("\r", " ").Replace("\n", " ");
            return cleaned.Length <= maximumLength
                ? cleaned
                : cleaned.Substring(0, maximumLength);
        }

        private static string GetConnectionString()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["TravelTime"];
            if (settings != null && !string.IsNullOrWhiteSpace(settings.ConnectionString))
                return settings.ConnectionString;
            settings = ConfigurationManager.ConnectionStrings["TravelTimeDb"];
            return settings == null ? string.Empty : settings.ConnectionString;
        }
    }
}
