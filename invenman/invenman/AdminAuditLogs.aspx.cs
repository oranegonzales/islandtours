using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class AdminAuditLogs : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            EnforceAdminOnly();

            if (!IsPostBack)
            {
                BindLogs(0);
            }
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            ClearMessages();
            gvLogs.PageIndex = 0;
            BindLogs(0);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearMessages();

            txtUsername.Text = "";
            ddlAction.SelectedValue = "";
            txtFromDate.Text = "";
            txtToDate.Text = "";

            gvLogs.PageIndex = 0;
            BindLogs(0);
        }

        protected void gvLogs_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ClearMessages();
            gvLogs.PageIndex = e.NewPageIndex;
            BindLogs(e.NewPageIndex);
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            ClearMessages();

            try
            {
                DataTable dt = LoadLogsForExport();
                string csv = ToCsv(dt);

                Response.Clear();
                Response.ContentType = "text/csv";
                Response.AddHeader("Content-Disposition", "attachment;filename=audit_logs.csv");
                Response.ContentEncoding = Encoding.UTF8;
                Response.Write(csv);
                Response.End();
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to export logs. Detail: " + ex.Message;
            }
        }

        protected void btnClearLogs_Click(object sender, EventArgs e)
        {
            ClearMessages();

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM AuditLogs", conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                gvLogs.PageIndex = 0;
                BindLogs(0);
                lblMessage.Text = "Logs cleared successfully.";
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to clear logs. Detail: " + ex.Message;
            }
        }

        private void BindLogs(int pageIndex)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = BuildFilteredQuery(top: 500))
                {
                    cmd.Connection = conn;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvLogs.DataSource = dt;
                        gvLogs.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to load audit logs. Detail: " + ex.Message;
            }
        }

        private DataTable LoadLogsForExport()
        {
            using (SqlConnection conn = new SqlConnection(GetConnStr()))
            using (SqlCommand cmd = BuildFilteredQuery(top: 2000))
            {
                cmd.Connection = conn;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private SqlCommand BuildFilteredQuery(int top)
        {
            string usernameFilter = (txtUsername.Text ?? "").Trim();
            string actionFilter = (ddlAction.SelectedValue ?? "").Trim();
            DateTime? fromDate = ParseDate(txtFromDate.Text);
            DateTime? toDate = ParseDate(txtToDate.Text);

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT TOP (@Top) AuditLogID, LogDate, Username, RoleName, ActionType, PageUrl, Details ");
            sql.Append("FROM AuditLogs WHERE 1=1 ");

            SqlCommand cmd = new SqlCommand();

            cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;

            if (!string.IsNullOrWhiteSpace(usernameFilter))
            {
                sql.Append("AND Username LIKE @Username ");
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 120).Value = "%" + usernameFilter + "%";
            }

            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                sql.Append("AND ActionType = @ActionType ");
                cmd.Parameters.Add("@ActionType", SqlDbType.VarChar, 100).Value = actionFilter;
            }

            if (fromDate.HasValue)
            {
                sql.Append("AND LogDate >= @FromDate ");
                cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = fromDate.Value;
            }

            if (toDate.HasValue)
            {
                DateTime endExclusive = toDate.Value.Date.AddDays(1);
                sql.Append("AND LogDate < @ToDate ");
                cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = endExclusive;
            }

            sql.Append("ORDER BY LogDate DESC, AuditLogID DESC");

            cmd.CommandText = sql.ToString();
            return cmd;
        }

        private DateTime? ParseDate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            DateTime parsed;
            if (DateTime.TryParse(input, out parsed))
            {
                return parsed.Date;
            }

            return null;
        }

        private string ToCsv(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(EscapeCsv(dt.Columns[i].ColumnName));
            }
            sb.AppendLine();

            for (int r = 0; r < dt.Rows.Count; r++)
            {
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    if (c > 0) sb.Append(",");
                    string val = dt.Rows[r][c] == DBNull.Value ? "" : dt.Rows[r][c].ToString();
                    sb.Append(EscapeCsv(val));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string EscapeCsv(string value)
        {
            if (value == null) value = "";
            bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n");
            value = value.Replace("\"", "\"\"");

            if (mustQuote)
            {
                return "\"" + value + "\"";
            }

            return value;
        }

        private void EnforceAdminOnly()
        {
            string username = Session["Username"] as string;
            string role = Session["Role"] as string;

            if (string.IsNullOrWhiteSpace(username))
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Home.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private string GetConnStr()
        {
            ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["TravelTime"];
            if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                return cs.ConnectionString;
            }

            cs = ConfigurationManager.ConnectionStrings["TravelTime"];
            if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                return cs.ConnectionString;
            }

            return "";
        }

        private void ClearMessages()
        {
            lblMessage.Text = "";
            lblError.Text = "";
        }
    }
}
