using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using invenman.Configuration;

namespace invenman
{
    public partial class AdminSystemConfig : Page
    {
        private const string ConnName = "TravelTime";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAdmin())
            {
                Response.Redirect("~/Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadAll();
            }
        }

        protected void btnReload_Click(object sender, EventArgs e)
        {
            LoadAll();
            lblMsg.Text = "Reloaded current settings.";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveToWebConfig();
                LoadAll();
                lblMsg.Text = "Settings saved successfully.";
            }
            catch (Exception)
            {
                lblMsg.Text = "Unable to save settings.";
            }
        }

        private bool IsAdmin()
        {
            string role = (Session["Role"] as string) ?? "";
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadAll()
        {
            lblConnName.Text = ConnName;
            lblDbInfo.Text = GetDbInfoSafe();

            chkMaintenanceMode.Checked = GetBool("MaintenanceMode", false);
            chkAdRotatorEnabled.Checked = GetBool("AdRotatorEnabled", true);
            chkShowUsd.Checked = GetBool("ShowUsdConversion", true);

            string rounding = GetString("UsdRounding", "2");
            if (rounding != "0" && rounding != "2")
            {
                rounding = "2";
            }
            ddlRounding.SelectedValue = rounding;

            txtCompanyName.Text = GetString("CompanyName", "IslandExplore Tours");
            txtSupportEmail.Text = GetString("SupportEmail", "");
            txtSupportPhone.Text = GetString("SupportPhone", "");

            txtCashInstructions.Text = GetString("CashPaymentInstructions",
                "Contact the operations team for the current office location and hours. Bring your booking number.");

            txtBankInstructions.Text = GetString("BankTransferInstructions",
                "Request current transfer instructions from the operations team. Include your booking number as the reference.");

            txtTransportProviders.Text = NormalizeLines(GetString("TransportProviders",
                "JUTA Taxi\r\nKnutsford Express\r\nIsland Routes\r\nPrivate Driver"));

            txtFixerKey.Text = string.IsNullOrWhiteSpace(AppConfiguration.GetFixerApiKey())
                ? "Not configured"
                : "Configured outside Web.config";
            txtFixerKey.ReadOnly = true;
        }

        private void SaveToWebConfig()
        {
            System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            var settings = config.AppSettings.Settings;

            SetSetting(settings, "MaintenanceMode", chkMaintenanceMode.Checked ? "true" : "false");
            SetSetting(settings, "AdRotatorEnabled", chkAdRotatorEnabled.Checked ? "true" : "false");
            SetSetting(settings, "ShowUsdConversion", chkShowUsd.Checked ? "true" : "false");
            SetSetting(settings, "UsdRounding", ddlRounding.SelectedValue);

            SetSetting(settings, "CompanyName", (txtCompanyName.Text ?? "").Trim());
            SetSetting(settings, "SupportEmail", (txtSupportEmail.Text ?? "").Trim());
            SetSetting(settings, "SupportPhone", (txtSupportPhone.Text ?? "").Trim());

            SetSetting(settings, "CashPaymentInstructions", NormalizeNewlines((txtCashInstructions.Text ?? "").Trim()));
            SetSetting(settings, "BankTransferInstructions", NormalizeNewlines((txtBankInstructions.Text ?? "").Trim()));

            string providers = NormalizeProviders(txtTransportProviders.Text ?? "");
            SetSetting(settings, "TransportProviders", providers);

            config.Save(ConfigurationSaveMode.Modified);
        }

        private static void SetSetting(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
        }

        private static string GetString(string key, string fallback)
        {
            string v = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(v))
            {
                return fallback;
            }
            return v;
        }

        private static bool GetBool(string key, bool fallback)
        {
            string v = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(v))
            {
                return fallback;
            }
            if (bool.TryParse(v.Trim(), out bool b))
            {
                return b;
            }
            return fallback;
        }

        private static string NormalizeProviders(string raw)
        {
            string normalized = NormalizeNewlines(raw);
            var lines = normalized.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (lines.Length == 0)
            {
                return "JUTA Taxi\r\nKnutsford Express\r\nIsland Routes\r\nPrivate Driver";
            }

            return string.Join("\r\n", lines);
        }

        private static string NormalizeNewlines(string s)
        {
            return (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        }

        private static string NormalizeLines(string s)
        {
            return NormalizeNewlines(s ?? "");
        }

        private string GetDbInfoSafe()
        {
            try
            {
                var cs = ConfigurationManager.ConnectionStrings[ConnName];
                if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                {
                    return "Connection string not found: " + ConnName;
                }

                SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(cs.ConnectionString);
                string server = string.IsNullOrWhiteSpace(b.DataSource) ? "Unknown server" : b.DataSource;
                string db = string.IsNullOrWhiteSpace(b.InitialCatalog) ? "Unknown database" : b.InitialCatalog;

                return server + " | " + db;
            }
            catch (Exception)
            {
                return "Unable to read connection information.";
            }
        }
    }
}
