using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using invenman.Configuration;

namespace invenman.Security
{
    internal static class ClientIdentity
    {
        public static int? GetClientId(Page page)
        {
            if (page == null) throw new ArgumentNullException("page");

            string username = page.Session["Username"] as string;
            string role = page.Session["Role"] as string;
            if (string.IsNullOrWhiteSpace(username) ||
                !string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            const string sql = @"
SELECT TOP (1) c.ClientID
FROM dbo.Users AS u
INNER JOIN dbo.Clients AS c
    ON c.Email = NULLIF(u.Email, N'')
    OR c.Email = u.Username
WHERE u.Username = @Username
  AND u.IsActive = 1
  AND u.RoleName = N'Client'
ORDER BY CASE
    WHEN c.Email = NULLIF(u.Email, N'') THEN 0
    ELSE 1
END, c.ClientID;";

            using (SqlConnection connection = new SqlConnection(AppConfiguration.GetConnectionString()))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value
                    ? (int?)null
                    : Convert.ToInt32(value);
            }
        }
    }
}
