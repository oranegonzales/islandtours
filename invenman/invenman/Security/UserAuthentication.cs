using System;
using System.Data;
using System.Data.SqlClient;
using invenman.Configuration;

namespace invenman.Security
{
    internal static class UserAuthentication
    {
        public static string AuthenticateStaff(string username, string password)
        {
            return Authenticate(username, password, false);
        }

        public static string AuthenticateClient(string username, string password)
        {
            return Authenticate(username, password, true);
        }

        private static string Authenticate(string username, string password, bool requireClient)
        {
            int userId = 0;
            string storedPassword = null;
            string role = null;
            int failedLoginCount = 0;
            DateTime? lockoutEndUtc = null;

            using (SqlConnection connection =
                new SqlConnection(AppConfiguration.GetConnectionString()))
            using (SqlCommand command = new SqlCommand(
                "SELECT UserID, UserPassword, RoleName, FailedLoginCount, LockoutEndUtc FROM Users WITH (UPDLOCK, ROWLOCK) " +
                "WHERE Username=@Username AND IsActive=1",
                connection))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    command.Transaction = transaction;
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;

                    using (SqlDataReader reader =
                        command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            userId = reader.GetInt32(0);
                            storedPassword = reader.GetString(1);
                            role = reader.GetString(2);
                            failedLoginCount = reader.GetInt32(3);
                            lockoutEndUtc = reader.IsDBNull(4)
                                ? (DateTime?)null
                                : reader.GetDateTime(4);
                        }
                    }

                    if (userId <= 0)
                    {
                        PasswordSecurity.RunDummyVerification(password);
                        transaction.Commit();
                        return string.Empty;
                    }

                    if (lockoutEndUtc.HasValue && lockoutEndUtc.Value > DateTime.UtcNow)
                    {
                        PasswordSecurity.RunDummyVerification(password);
                        transaction.Commit();
                        return string.Empty;
                    }

                    bool isClient = string.Equals(
                        role,
                        "Client",
                        StringComparison.OrdinalIgnoreCase);
                    bool passwordValid = PasswordSecurity.VerifyPassword(password, storedPassword);

                    if (isClient != requireClient || !passwordValid)
                    {
                        if (isClient == requireClient)
                        {
                            RegisterFailedAttempt(connection, transaction, userId, failedLoginCount + 1);
                        }
                        transaction.Commit();
                        return string.Empty;
                    }

                    using (SqlCommand success = new SqlCommand(
                        "UPDATE Users SET UserPassword=@Password, FailedLoginCount=0, LockoutEndUtc=NULL, LastLoginAt=SYSUTCDATETIME() " +
                        "WHERE UserID=@UserID",
                        connection,
                        transaction))
                    {
                        success.Parameters.Add("@Password", SqlDbType.VarChar, 512).Value =
                            PasswordSecurity.NeedsRehash(storedPassword)
                                ? PasswordSecurity.HashPassword(password)
                                : storedPassword;
                        success.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                        success.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return role;
                }
            }
        }

        private static void RegisterFailedAttempt(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int failedLoginCount)
        {
            const int maximumAttempts = 5;
            using (SqlCommand failure = new SqlCommand(
                "UPDATE Users SET FailedLoginCount=@FailedLoginCount, " +
                "LockoutEndUtc=CASE WHEN @FailedLoginCount >= @MaximumAttempts " +
                "THEN DATEADD(MINUTE, 15, SYSUTCDATETIME()) ELSE NULL END " +
                "WHERE UserID=@UserID",
                connection,
                transaction))
            {
                failure.Parameters.Add("@FailedLoginCount", SqlDbType.Int).Value = failedLoginCount;
                failure.Parameters.Add("@MaximumAttempts", SqlDbType.Int).Value = maximumAttempts;
                failure.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                failure.ExecuteNonQuery();
            }
        }
    }
}
