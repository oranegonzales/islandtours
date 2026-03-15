using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace invenman
{
    public partial class AdminUserRoles : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            EnforceAdminOnly();

            if (!IsPostBack)
            {
                BindUsers();
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearMessages();
            BindUsers();
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

        private void BindUsers()
        {
            ClearMessages();

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand("SELECT UserID, Username, RoleName, IsActive, Email FROM Users ORDER BY UserID DESC", conn))
                {
                    conn.Open();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvUsers.DataSource = dt;
                        gvUsers.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to load users. Detail: " + ex.Message;
            }
        }

        protected void gvUsers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            ClearMessages();
            gvUsers.EditIndex = e.NewEditIndex;
            BindUsers();
        }

        protected void gvUsers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            ClearMessages();
            gvUsers.EditIndex = -1;
            BindUsers();
        }

        protected void gvUsers_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ClearMessages();

            int userId = Convert.ToInt32(gvUsers.DataKeys[e.RowIndex].Value);

            GridViewRow row = gvUsers.Rows[e.RowIndex];

            TextBox txtUsernameEdit = row.FindControl("txtUsernameEdit") as TextBox;
            DropDownList ddlRoleEdit = row.FindControl("ddlRoleEdit") as DropDownList;
            CheckBox chkActiveEdit = row.FindControl("chkActiveEdit") as CheckBox;
            TextBox txtEmailEdit = row.FindControl("txtEmailEdit") as TextBox;
            TextBox txtPasswordEdit = row.FindControl("txtPasswordEdit") as TextBox;

            string newUsername = (txtUsernameEdit == null) ? "" : txtUsernameEdit.Text.Trim();
            string newRole = (ddlRoleEdit == null) ? "" : ddlRoleEdit.SelectedValue;
            bool isActive = chkActiveEdit != null && chkActiveEdit.Checked;
            string newEmail = (txtEmailEdit == null) ? "" : (txtEmailEdit.Text ?? "").Trim();
            string newPassword = (txtPasswordEdit == null) ? "" : (txtPasswordEdit.Text ?? "");

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                lblError.Text = "Username cannot be empty.";
                return;
            }

            string currentUsername = Session["Username"] as string;
            if (!string.IsNullOrWhiteSpace(currentUsername) && string.Equals(currentUsername, newUsername, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(newRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    lblError.Text = "You cannot change your own role away from Admin while logged in.";
                    return;
                }

                if (!isActive)
                {
                    lblError.Text = "You cannot deactivate your own account while logged in.";
                    return;
                }
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE Users
SET Username = @Username,
    UserPassword = CASE WHEN @UserPassword = '' THEN UserPassword ELSE @UserPassword END,
    RoleName = @RoleName,
    IsActive = @IsActive,
    Email = NULLIF(@Email, '')
WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = newUsername;
                    cmd.Parameters.Add("@UserPassword", SqlDbType.VarChar, 200).Value = newPassword;
                    cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 50).Value = newRole;
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                    cmd.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = newEmail;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    gvUsers.EditIndex = -1;
                    BindUsers();

                    lblMessage.Text = rows > 0 ? "User updated successfully." : "No changes were saved.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to update user. Detail: " + ex.Message;
            }
        }

        protected void gvUsers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ClearMessages();

            int userId = Convert.ToInt32(gvUsers.DataKeys[e.RowIndex].Value);

            string currentUsername = Session["Username"] as string;

            try
            {
                if (!string.IsNullOrWhiteSpace(currentUsername))
                {
                    string deletingUsername = GetUsernameById(userId);
                    if (!string.IsNullOrWhiteSpace(deletingUsername) &&
                        string.Equals(deletingUsername, currentUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        lblError.Text = "You cannot delete your own account while logged in.";
                        return;
                    }
                }

                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    BindUsers();
                    lblMessage.Text = rows > 0 ? "User deleted successfully." : "User not found.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to delete user. Detail: " + ex.Message;
            }
        }

        protected void btnCreateUser_Click(object sender, EventArgs e)
        {
            ClearMessages();

            if (!Page.IsValid)
            {
                return;
            }

            string username = (txtNewUsername.Text ?? "").Trim();
            string password = txtNewPassword.Text ?? "";
            string role = ddlNewRole.SelectedValue ?? "Staff";
            string email = (txtNewEmail.Text ?? "").Trim();
            bool isActive = chkNewActive.Checked;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Username and password are required.";
                return;
            }

            try
            {
                if (UsernameExists(username))
                {
                    lblError.Text = "That username already exists.";
                    return;
                }

                using (SqlConnection conn = new SqlConnection(GetConnStr()))
                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO Users (Username, UserPassword, RoleName, IsActive, Email)
VALUES (@Username, @UserPassword, @RoleName, @IsActive, NULLIF(@Email, ''))", conn))
                {
                    cmd.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@UserPassword", SqlDbType.VarChar, 200).Value = password;
                    cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 50).Value = role;
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                    cmd.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = email;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    txtNewUsername.Text = "";
                    txtNewPassword.Text = "";
                    txtNewEmail.Text = "";
                    chkNewActive.Checked = true;
                    ddlNewRole.SelectedValue = "Staff";

                    BindUsers();
                    lblMessage.Text = "User created successfully.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Unable to create user. Detail: " + ex.Message;
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            if ((e.Row.RowState & DataControlRowState.Edit) != 0)
            {
                DropDownList ddlRoleEdit = e.Row.FindControl("ddlRoleEdit") as DropDownList;
                HiddenField hfRoleCurrent = e.Row.FindControl("hfRoleCurrent") as HiddenField;

                if (ddlRoleEdit != null)
                {
                    ddlRoleEdit.Items.Clear();
                    ddlRoleEdit.Items.Add(new ListItem("Staff", "Staff"));
                    ddlRoleEdit.Items.Add(new ListItem("Admin", "Admin"));
                    ddlRoleEdit.Items.Add(new ListItem("Client", "Client"));

                    string current = hfRoleCurrent == null ? "" : (hfRoleCurrent.Value ?? "");
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        ListItem li = ddlRoleEdit.Items.FindByValue(current);
                        if (li != null)
                        {
                            ddlRoleEdit.ClearSelection();
                            li.Selected = true;
                        }
                    }
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            gvUsers.RowDataBound += gvUsers_RowDataBound;
        }

        private bool UsernameExists(string username)
        {
            using (SqlConnection conn = new SqlConnection(GetConnStr()))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Username = @Username", conn))
            {
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = username;
                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private string GetUsernameById(int userId)
        {
            using (SqlConnection conn = new SqlConnection(GetConnStr()))
            using (SqlCommand cmd = new SqlCommand("SELECT Username FROM Users WHERE UserID = @UserID", conn))
            {
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return "";
                }
                return result.ToString();
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
