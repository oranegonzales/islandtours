<%@ Page Title="User accounts / roles" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminUserRoles.aspx.cs" Inherits="invenman.AdminUserRoles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="mx-auto" style="max-width: 980px;">
        <div class="p-4 rounded-4 border" style="background: rgba(2, 6, 23, 0.35); border-color: rgba(255,255,255,0.10) !important;">
            <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                <div>
                    <div class="h2 m-0 fw-bold">User accounts / roles</div>
                    <div class="text-secondary" style="color:#cbd5e1 !important;">Manage staff and client logins, roles, and status.</div>
                </div>
                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-sm btn-outline-light" OnClick="btnRefresh_Click" />
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="d-block mb-3" ForeColor="LightGreen"></asp:Label>
            <asp:Label ID="lblError" runat="server" CssClass="d-block mb-3" ForeColor="Salmon"></asp:Label>

            <div class="table-responsive">
                <asp:GridView ID="gvUsers" runat="server"
                    AutoGenerateColumns="False"
                    DataKeyNames="UserID"
                    CssClass="table table-dark table-striped align-middle"
                    GridLines="None"
                    OnRowEditing="gvUsers_RowEditing"
                    OnRowCancelingEdit="gvUsers_RowCancelingEdit"
                    OnRowUpdating="gvUsers_RowUpdating"
                    OnRowDeleting="gvUsers_RowDeleting">

                    <Columns>
                        <asp:BoundField DataField="UserID" HeaderText="User ID" ReadOnly="True" />

                        <asp:TemplateField HeaderText="Username">
                            <ItemTemplate>
                                <asp:Label ID="lblUsername" runat="server" Text='<%# Eval("Username") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtUsernameEdit" runat="server" Text='<%# Bind("Username") %>' CssClass="form-control form-control-sm"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Role">
                            <ItemTemplate>
                                <asp:Label ID="lblRole" runat="server" Text='<%# Eval("RoleName") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlRoleEdit" runat="server" CssClass="form-select form-select-sm"></asp:DropDownList>
                                <asp:HiddenField ID="hfRoleCurrent" runat="server" Value='<%# Eval("RoleName") %>' />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Active">
                            <ItemTemplate>
                                <asp:Label ID="lblActive" runat="server" Text='<%# (Convert.ToBoolean(Eval("IsActive")) ? "Yes" : "No") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:CheckBox ID="chkActiveEdit" runat="server" Checked='<%# Bind("IsActive") %>' />
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Email">
                            <ItemTemplate>
                                <asp:Label ID="lblEmail" runat="server" Text='<%# Eval("Email") %>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtEmailEdit" runat="server" Text='<%# Bind("Email") %>' CssClass="form-control form-control-sm"></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Set new password">
                            <ItemTemplate>
                                <span style="color:#94a3b8;">(leave blank)</span>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtPasswordEdit" runat="server" TextMode="Password" CssClass="form-control form-control-sm" placeholder=""></asp:TextBox>
                            </EditItemTemplate>
                        </asp:TemplateField>

                        <asp:CommandField ShowEditButton="True" />

                        <asp:TemplateField HeaderText="Delete">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnDelete" runat="server" CommandName="Delete" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                    OnClientClick="return confirm('Delete this user?');"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                </asp:GridView>
            </div>

            <hr style="border-color: rgba(255,255,255,0.12);" class="my-4" />

            <div class="h4 fw-bold mb-2">Create new user</div>

            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Username</label>
                    <asp:TextBox ID="txtNewUsername" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvNewUsername" runat="server"
                        ControlToValidate="txtNewUsername" ErrorMessage="Username is required."
                        ForeColor="Salmon" Display="Dynamic" />
                </div>

                <div class="col-md-4">
                    <label class="form-label">Password</label>
                    <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server"
                        ControlToValidate="txtNewPassword" ErrorMessage="Password is required."
                        ForeColor="Salmon" Display="Dynamic" />
                </div>

                <div class="col-md-4">
                    <label class="form-label">Role</label>
                    <asp:DropDownList ID="ddlNewRole" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Staff" Value="Staff" />
                        <asp:ListItem Text="Admin" Value="Admin" />
                        <asp:ListItem Text="Client" Value="Client" />
                    </asp:DropDownList>
                </div>

                <div class="col-md-6">
                    <label class="form-label">Email (optional)</label>
                    <asp:TextBox ID="txtNewEmail" runat="server" CssClass="form-control"></asp:TextBox>
                </div>

                <div class="col-md-6 d-flex align-items-end">
                    <div class="form-check">
                        <asp:CheckBox ID="chkNewActive" runat="server" Checked="true" CssClass="form-check-input" />
                        <label class="form-check-label" for="<%= chkNewActive.ClientID %>">Active</label>
                    </div>
                </div>

                <div class="col-12">
                    <asp:Button ID="btnCreateUser" runat="server" Text="Create user" CssClass="btn btn-warning fw-bold" OnClick="btnCreateUser_Click" />
                </div>
            </div>

        </div>
    </div>

</asp:Content>
