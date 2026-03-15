using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.UI;

namespace invenman
{
    public partial class PaymentReceipt : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReceipt();
            }
        }

        private void ShowError(string message)
        {
            pnlReceipt.Visible = false;
            pnlError.Visible = true;
            lblError.Text = message;
        }

        private void LoadReceipt()
        {
            pnlError.Visible = false;
            pnlReceipt.Visible = false;

            int paymentId = 0;
            if (!int.TryParse(Request.QueryString["paymentId"], out paymentId) || paymentId <= 0)
            {
                ShowError("No valid payment was specified for this receipt.");
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            try
            {
                ReceiptRow row = GetReceiptRow(connStr, paymentId);
                if (row == null)
                {
                    ShowError("No receipt data was found for that payment.");
                    return;
                }

                string attractionName = "";
                string attractionLocation = "";

                if (row.AttractionID > 0)
                {
                    AttractionDisplay ad = GetAttractionDisplay(connStr, row.AttractionID);
                    if (ad != null)
                    {
                        attractionName = ad.Name;
                        attractionLocation = ad.Location;
                    }
                }

                lblReceiptNumber.Text = row.PaymentID.ToString(CultureInfo.InvariantCulture);
                lblPaymentDate.Text = row.PaymentDate.HasValue ? row.PaymentDate.Value.ToString("yyyy-MM-dd HH:mm") : "N/A";
                lblPaymentMethod.Text = SafeText(row.PaymentMethod, "N/A");
                lblTxnRef.Text = SafeText(row.TransactionReference, "N/A");
                lblAmountJmd.Text = "JMD " + row.Amount.ToString("N2", CultureInfo.InvariantCulture);

                lblBookingId.Text = row.BookingID > 0 ? row.BookingID.ToString(CultureInfo.InvariantCulture) : "N/A";
                lblTourDate.Text = row.TourDate.HasValue ? row.TourDate.Value.ToString("yyyy-MM-dd") : "N/A";
                lblBookingStatus.Text = SafeText(row.BookingStatus, "N/A");
                lblPaymentStatus.Text = SafeText(row.PaymentStatus, "N/A");

                lblClientName.Text = SafeText(row.ClientName, "N/A");
                lblClientEmail.Text = SafeText(row.ClientEmail, "N/A");

                lblAttractionName.Text = SafeText(attractionName, row.AttractionID > 0 ? ("Attraction #" + row.AttractionID.ToString(CultureInfo.InvariantCulture)) : "N/A");
                lblAttractionLocation.Text = SafeText(attractionLocation, "N/A");

                lblAmountUsd.Text = TryGetUsdTextFromFixer(row.Amount);

                pnlReceipt.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("There was a problem loading the receipt. Detail: " + ex.Message);
            }
        }

        private string SafeText(string value, string fallback)
        {
            string v = (value ?? "").Trim();
            return string.IsNullOrWhiteSpace(v) ? fallback : v;
        }

        private string TryGetUsdTextFromFixer(decimal amountJmd)
        {
            try
            {
                string apiKey = ConfigurationManager.AppSettings["FixerApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return "USD conversion not configured";
                }

                decimal? usd = GetUsdFromFixer(apiKey, amountJmd);
                if (!usd.HasValue)
                {
                    return "USD conversion unavailable";
                }

                return "USD " + usd.Value.ToString("N2", CultureInfo.InvariantCulture);
            }
            catch
            {
                return "USD conversion unavailable";
            }
        }

        private decimal? GetUsdFromFixer(string apiKey, decimal amountJmd)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string url = "https://data.fixer.io/api/latest?access_key=" + Uri.EscapeDataString(apiKey) + "&symbols=JMD,USD";

            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                string json = wc.DownloadString(url) ?? "";

                decimal? jmdRate = TryParseRate(json, "JMD");
                decimal? usdRate = TryParseRate(json, "USD");

                if (!jmdRate.HasValue || !usdRate.HasValue)
                {
                    return null;
                }

                if (jmdRate.Value <= 0m)
                {
                    return null;
                }

                decimal usdPerJmd = usdRate.Value / jmdRate.Value;
                decimal usdAmount = amountJmd * usdPerJmd;
                return usdAmount;
            }
        }

        private decimal? TryParseRate(string json, string code)
        {
            int ratesIndex = json.IndexOf("\"rates\"", StringComparison.OrdinalIgnoreCase);
            if (ratesIndex < 0)
            {
                return null;
            }

            int codeIndex = json.IndexOf("\"" + code + "\"", ratesIndex, StringComparison.OrdinalIgnoreCase);
            if (codeIndex < 0)
            {
                return null;
            }

            int colon = json.IndexOf(":", codeIndex, StringComparison.OrdinalIgnoreCase);
            if (colon < 0)
            {
                return null;
            }

            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t'))
            {
                start++;
            }

            int end = start;
            while (end < json.Length && ("0123456789.-".IndexOf(json[end]) >= 0))
            {
                end++;
            }

            if (end <= start)
            {
                return null;
            }

            string num = json.Substring(start, end - start);
            decimal parsed;
            if (!decimal.TryParse(num, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
            {
                return null;
            }

            return parsed;
        }

        private ReceiptRow GetReceiptRow(string connStr, int paymentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
SELECT
    p.PaymentID,
    p.BookingID,
    p.PaymentDate,
    p.Amount,
    p.PaymentMethod,
    ISNULL(p.TransactionReference, '') AS TransactionReference,
    ISNULL(p.CurrencyCode, 'JMD') AS CurrencyCode,
    b.TourDate,
    ISNULL(b.BookingStatus, '') AS BookingStatus,
    ISNULL(b.PaymentStatus, '') AS PaymentStatus,
    b.AttractionID,
    c.FirstName,
    c.LastName,
    ISNULL(c.Email, '') AS Email
FROM Payments p
INNER JOIN Bookings b ON p.BookingID = b.BookingID
INNER JOIN Clients c ON b.ClientID = c.ClientID
WHERE p.PaymentID = @PaymentID", conn))
            {
                cmd.Parameters.Add("@PaymentID", SqlDbType.Int).Value = paymentId;

                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        return null;
                    }

                    ReceiptRow row = new ReceiptRow();
                    row.PaymentID = SafeGetInt(r, "PaymentID");
                    row.BookingID = SafeGetInt(r, "BookingID");
                    row.PaymentDate = SafeGetDateTime(r, "PaymentDate");
                    row.Amount = SafeGetDecimal(r, "Amount");
                    row.PaymentMethod = SafeGetString(r, "PaymentMethod");
                    row.TransactionReference = SafeGetString(r, "TransactionReference");
                    row.CurrencyCode = SafeGetString(r, "CurrencyCode");
                    row.TourDate = SafeGetDate(r, "TourDate");
                    row.BookingStatus = SafeGetString(r, "BookingStatus");
                    row.PaymentStatus = SafeGetString(r, "PaymentStatus");
                    row.AttractionID = SafeGetInt(r, "AttractionID");

                    string first = SafeGetString(r, "FirstName");
                    string last = SafeGetString(r, "LastName");
                    row.ClientName = (first + " " + last).Trim();
                    row.ClientEmail = SafeGetString(r, "Email");

                    return row;
                }
            }
        }

        private AttractionDisplay GetAttractionDisplay(string connStr, int attractionId)
        {
            AttractionDisplay ad = TryAttractionQuery(connStr, attractionId, "SELECT AttractionName AS AName, Location AS ALoc FROM Attractions WHERE AttractionID = @AttractionID");
            if (ad != null)
            {
                return ad;
            }

            ad = TryAttractionQuery(connStr, attractionId, "SELECT Name AS AName, Location AS ALoc FROM Attractions WHERE AttractionID = @AttractionID");
            if (ad != null)
            {
                return ad;
            }

            ad = TryAttractionQuery(connStr, attractionId, "SELECT AttractionName AS AName, Parish AS ALoc FROM Attractions WHERE AttractionID = @AttractionID");
            if (ad != null)
            {
                return ad;
            }

            ad = TryAttractionQuery(connStr, attractionId, "SELECT Name AS AName, Parish AS ALoc FROM Attractions WHERE AttractionID = @AttractionID");
            return ad;
        }

        private AttractionDisplay TryAttractionQuery(string connStr, int attractionId, string sql)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@AttractionID", SqlDbType.Int).Value = attractionId;

                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            return null;
                        }

                        AttractionDisplay ad = new AttractionDisplay();
                        ad.Name = SafeGetString(r, "AName");
                        ad.Location = SafeGetString(r, "ALoc");
                        return ad;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private int SafeGetInt(SqlDataReader r, string col)
        {
            int ordinal = r.GetOrdinal(col);
            if (r.IsDBNull(ordinal))
            {
                return 0;
            }
            return Convert.ToInt32(r.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private decimal SafeGetDecimal(SqlDataReader r, string col)
        {
            int ordinal = r.GetOrdinal(col);
            if (r.IsDBNull(ordinal))
            {
                return 0m;
            }
            return Convert.ToDecimal(r.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private string SafeGetString(SqlDataReader r, string col)
        {
            int ordinal = r.GetOrdinal(col);
            if (r.IsDBNull(ordinal))
            {
                return "";
            }
            return Convert.ToString(r.GetValue(ordinal), CultureInfo.InvariantCulture) ?? "";
        }

        private DateTime? SafeGetDateTime(SqlDataReader r, string col)
        {
            int ordinal = r.GetOrdinal(col);
            if (r.IsDBNull(ordinal))
            {
                return null;
            }
            return Convert.ToDateTime(r.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private DateTime? SafeGetDate(SqlDataReader r, string col)
        {
            int ordinal = r.GetOrdinal(col);
            if (r.IsDBNull(ordinal))
            {
                return null;
            }
            DateTime dt = Convert.ToDateTime(r.GetValue(ordinal), CultureInfo.InvariantCulture);
            return dt.Date;
        }

        private class ReceiptRow
        {
            public int PaymentID;
            public int BookingID;
            public DateTime? PaymentDate;
            public decimal Amount;
            public string PaymentMethod;
            public string TransactionReference;
            public string CurrencyCode;
            public DateTime? TourDate;
            public string BookingStatus;
            public string PaymentStatus;
            public int AttractionID;
            public string ClientName;
            public string ClientEmail;
        }

        private class AttractionDisplay
        {
            public string Name;
            public string Location;
        }
    }
}