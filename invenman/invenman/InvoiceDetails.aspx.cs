using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;
using invenman.Configuration;
using invenman.Security;

namespace invenman
{
    public partial class InvoiceDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadInvoice();
            }
        }

        private void LoadInvoice()
        {
            lblError.Text = string.Empty;
            pnlInvoice.Visible = true;

            string idValue = Request.QueryString["BookingID"];
            int bookingId;
            if (!int.TryParse(idValue, out bookingId) || bookingId <= 0)
            {
                pnlInvoice.Visible = false;
                lblError.Text = "Invalid booking id.";
                return;
            }

            string role = GetCurrentRole();
            bool isClient = string.Equals(role, "Client", StringComparison.OrdinalIgnoreCase);
            int? clientIdFilter = null;

            if (isClient)
            {
                clientIdFilter = GetClientIdForCurrentUser();
                if (clientIdFilter == null)
                {
                    pnlInvoice.Visible = false;
                    lblError.Text = "Your login is not linked to a client profile.";
                    return;
                }
            }

            string connStr = ConfigurationManager.ConnectionStrings["TravelTime"].ConnectionString;

            int clientIdFromBooking = 0;
            decimal totalAmount = 0m;
            string paymentStatus = string.Empty;
            string bookingStatus = string.Empty;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql =
                    "SELECT b.BookingID, b.BookingDate, b.TourDate, b.TotalAmount, b.PaymentStatus, b.BookingStatus, " +
                    "b.TransportProvider, b.PickupLocation, b.PickupDateTime, b.TransportNotes, " +
                    "c.ClientID, c.FirstName, c.LastName, c.Email, c.Phone, c.Country, " +
                    "a.Name AS AttractionName, a.Parish AS AttractionLocation, a.Parish " +
                    "FROM Bookings b " +
                    "INNER JOIN Clients c ON b.ClientID = c.ClientID " +
                    "INNER JOIN Attractions a ON b.AttractionID = a.AttractionID " +
                    "WHERE b.BookingID = @BookingID";

                if (isClient && clientIdFilter.HasValue)
                {
                    sql += " AND b.ClientID = @ClientID";
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;
                    if (isClient && clientIdFilter.HasValue)
                    {
                        cmd.Parameters.Add("@ClientID", SqlDbType.Int).Value = clientIdFilter.Value;
                    }

                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.Read())
                        {
                            pnlInvoice.Visible = false;
                            lblError.Text = "Invoice could not be found or you do not have access to it.";
                            return;
                        }

                        clientIdFromBooking = Convert.ToInt32(rdr["ClientID"]);
                        totalAmount = rdr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(rdr["TotalAmount"]) : 0m;
                        paymentStatus = rdr["PaymentStatus"] as string ?? string.Empty;
                        bookingStatus = rdr["BookingStatus"] as string ?? string.Empty;

                        lblInvoiceNumber.Text = "INV-" + bookingId.ToString("D6");
                        lblInvoiceDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

                        string firstName = rdr["FirstName"] as string ?? string.Empty;
                        string lastName = rdr["LastName"] as string ?? string.Empty;
                        lblClientName.Text = (firstName + " " + lastName).Trim();
                        lblClientEmail.Text = rdr["Email"] as string ?? string.Empty;
                        lblClientPhone.Text = rdr["Phone"] as string ?? string.Empty;
                        lblClientCountry.Text = rdr["Country"] as string ?? string.Empty;

                        lblBookingId.Text = bookingId.ToString();
                        if (rdr["BookingDate"] != DBNull.Value)
                        {
                            DateTime bd = Convert.ToDateTime(rdr["BookingDate"]);
                            lblBookingDate.Text = bd.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            lblBookingDate.Text = string.Empty;
                        }

                        if (rdr["TourDate"] != DBNull.Value)
                        {
                            DateTime td = Convert.ToDateTime(rdr["TourDate"]);
                            lblTourDate.Text = td.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            lblTourDate.Text = string.Empty;
                        }

                        lblAttractionName.Text = rdr["AttractionName"] as string ?? string.Empty;
                        string loc = rdr["AttractionLocation"] as string ?? string.Empty;
                        string parish = rdr["Parish"] as string ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(parish))
                        {
                            if (string.IsNullOrWhiteSpace(loc))
                            {
                                lblAttractionLocation.Text = parish;
                            }
                            else
                            {
                                lblAttractionLocation.Text = loc + ", " + parish;
                            }
                        }
                        else
                        {
                            lblAttractionLocation.Text = loc;
                        }

                        lblBookingStatus.Text = bookingStatus;
                        lblPaymentStatus.Text = paymentStatus;

                        lblTransportProvider.Text = rdr["TransportProvider"] as string ?? string.Empty;
                        lblPickupLocation.Text = rdr["PickupLocation"] as string ?? string.Empty;
                        if (rdr["PickupDateTime"] != DBNull.Value)
                        {
                            DateTime pdt = Convert.ToDateTime(rdr["PickupDateTime"]);
                            lblPickupDateTime.Text = pdt.ToString("yyyy-MM-dd HH:mm");
                        }
                        else
                        {
                            lblPickupDateTime.Text = string.Empty;
                        }
                    }
                }
            }

            decimal paidToDate = 0m;
            DataTable paymentsTable = new DataTable();

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT PaymentDate, Amount, PaymentMethod, TransactionReference " +
                "FROM Payments WHERE BookingID = @BookingID ORDER BY PaymentDate",
                conn))
            {
                cmd.Parameters.Add("@BookingID", SqlDbType.Int).Value = bookingId;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(paymentsTable);
                }

                foreach (DataRow row in paymentsTable.Rows)
                {
                    if (row["Amount"] != DBNull.Value)
                    {
                        paidToDate += Convert.ToDecimal(row["Amount"]);
                    }
                }
            }

            gvPayments.DataSource = paymentsTable;
            gvPayments.DataBind();

            decimal balance = totalAmount - paidToDate;

            lblTotalJmd.Text = totalAmount.ToString("N2", CultureInfo.InvariantCulture) + " JMD";
            lblPaidJmd.Text = paidToDate.ToString("N2", CultureInfo.InvariantCulture) + " JMD";
            lblBalanceJmd.Text = balance.ToString("N2", CultureInfo.InvariantCulture) + " JMD";

            decimal? jmdToUsdRate = GetJmdToUsdRate();
            if (jmdToUsdRate.HasValue)
            {
                decimal rate = jmdToUsdRate.Value;
                decimal totalUsd = Math.Round(totalAmount * rate, 2);
                decimal paidUsd = Math.Round(paidToDate * rate, 2);
                decimal balanceUsd = Math.Round(balance * rate, 2);

                lblTotalUsd.Text = totalUsd.ToString("N2", CultureInfo.InvariantCulture) + " USD";
                lblPaidUsd.Text = paidUsd.ToString("N2", CultureInfo.InvariantCulture) + " USD";
                lblBalanceUsd.Text = balanceUsd.ToString("N2", CultureInfo.InvariantCulture) + " USD";

                lblRateInfo.Text = "Converted using Fixer live rates. Approximate rate: 1 JMD = " +
                    rate.ToString("0.0000", CultureInfo.InvariantCulture) + " USD.";
            }
            else
            {
                lblTotalUsd.Text = "N/A";
                lblPaidUsd.Text = "N/A";
                lblBalanceUsd.Text = "N/A";
                lblRateInfo.Text = "Live USD conversion is temporarily unavailable.";
            }
        }

        private string GetCurrentRole()
        {
            object r = Session["Role"];
            if (r == null)
            {
                return "Guest";
            }
            string role = r.ToString();
            if (string.IsNullOrWhiteSpace(role))
            {
                return "Guest";
            }
            return role;
        }

        private int? GetClientIdForCurrentUser()
        {
            return ClientIdentity.GetClientId(this);
        }

        private decimal? GetJmdToUsdRate()
        {
            string apiKey = AppConfiguration.GetFixerApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            string url = "https://data.fixer.io/api/latest?access_key=" + Uri.EscapeDataString(apiKey) + "&symbols=USD,JMD";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 5000;
                request.ReadWriteTimeout = 5000;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(json);
                    if (root == null)
                    {
                        return null;
                    }

                    object successObj;
                    if (!root.TryGetValue("success", out successObj))
                    {
                        return null;
                    }

                    bool success = Convert.ToBoolean(successObj);
                    if (!success)
                    {
                        return null;
                    }

                    object ratesObj;
                    if (!root.TryGetValue("rates", out ratesObj))
                    {
                        return null;
                    }

                    Dictionary<string, object> ratesDict = ratesObj as Dictionary<string, object>;
                    if (ratesDict == null)
                    {
                        return null;
                    }

                    if (!ratesDict.ContainsKey("USD") || !ratesDict.ContainsKey("JMD"))
                    {
                        return null;
                    }

                    decimal eurToUsd = Convert.ToDecimal(ratesDict["USD"], CultureInfo.InvariantCulture);
                    decimal eurToJmd = Convert.ToDecimal(ratesDict["JMD"], CultureInfo.InvariantCulture);
                    if (eurToJmd == 0m)
                    {
                        return null;
                    }

                    decimal jmdToUsd = eurToUsd / eurToJmd;
                    return jmdToUsd;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
