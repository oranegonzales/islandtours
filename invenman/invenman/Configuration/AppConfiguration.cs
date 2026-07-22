using System;
using System.Configuration;

namespace invenman.Configuration
{
    internal static class AppConfiguration
    {
        public static string GetConnectionString()
        {
            ConnectionStringSettings value =
                ConfigurationManager.ConnectionStrings["TravelTime"];

            if (value == null || string.IsNullOrWhiteSpace(value.ConnectionString))
            {
                value = ConfigurationManager.ConnectionStrings["TravelTimeDb"];
            }

            if (value == null || string.IsNullOrWhiteSpace(value.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "Missing the TravelTime connection string in Web.config.");
            }

            return value.ConnectionString;
        }

        public static string GetFixerApiKey()
        {
            string fromEnvironment =
                Environment.GetEnvironmentVariable("TRAVELTIME_FIXER_API_KEY");

            if (!string.IsNullOrWhiteSpace(fromEnvironment))
            {
                return fromEnvironment.Trim();
            }

            return (ConfigurationManager.AppSettings["FixerApiKey"] ?? string.Empty).Trim();
        }
    }
}
