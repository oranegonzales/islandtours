<%@ Page Title="Audit logs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminAuditLogs.aspx.cs" Inherits="invenman.AdminAuditLogs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="mx-auto" style="max-width: 1100px;">
        <div class="p-4 rounded-4 border" style="background: rgba(2, 6, 23, 0.35); border-color: rgba(255,255,255,0.10) !important;">

            <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                <div>
                    <div class="h2 m-0 fw-bold">Audit logs</div>
                    <div class="text-secondary" style="color:#cbd5e1 !important;">Admin-only activity trail. Filter, export, or clear logs.</div>
                </div>

                <div class="d-flex flex-wrap gap-2">
                    <asp:Button ID="btnApply" runat="server" Text="Apply filters" CssClass="btn btn-warning fw-bold" OnClick="btnApply_Click" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-outline-light" OnClick="btnReset_Click" />
                    <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn btn-outline-info" OnClick="btnExport_Click" />
                    <asp:Button ID="btnClearLogs" runat="server" Text="Clear logs" CssClass="btn btn-outline-danger"
                        OnClick="btnClearLogs_Click"
                        OnClientClick="return confirm('Clear all audit logs? This cannot be undone.');" />
                </div>
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="d-block mb-2" ForeColor="LightGreen"></asp:Label>
            <asp:Label ID="lblError" runat="server" CssClass="d-block mb-3" ForeColor="Salmon"></asp:Label>

            <div class="row g-3 mb-3">
                <div class="col-md-3">
                    <label class="form-label">Username contains</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" />
                </div>

                <div class="col-md-3">
                    <label class="form-label">Action</label>
                    <asp:DropDownList ID="ddlAction" runat="server" CssClass="form-select">
                        <asp:ListItem Text="All actions" Value="" />
                        <asp:ListItem Text="Login" Value="Login" />
                        <asp:ListItem Text="Logout" Value="Logout" />
                        <asp:ListItem Text="Create" Value="Create" />
                        <asp:ListItem Text="Update" Value="Update" />
                        <asp:ListItem Text="Delete" Value="Delete" />
                        <asp:ListItem Text="Payment" Value="Payment" />
                        <asp:ListItem Text="Refund" Value="Refund" />
                        <asp:ListItem Text="Booking" Value="Booking" />
                        <asp:ListItem Text="Attraction" Value="Attraction" />
                        <asp:ListItem Text="Client" Value="Client" />
                        <asp:ListItem Text="Report" Value="Report" />
                        <asp:ListItem Text="System" Value="System" />
                    </asp:DropDownList>
                </div>

                <div class="col-md-3">
                    <label class="form-label">From date</label>
                    <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>

                <div class="col-md-3">
                    <label class="form-label">To date</label>
                    <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="gvLogs" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-dark table-striped align-middle"
                    GridLines="None"
                    AllowPaging="True"
                    PageSize="20"
                    OnPageIndexChanging="gvLogs_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="AuditLogID" HeaderText="ID" />
                        <asp:BoundField DataField="LogDate" HeaderText="Date and time" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                        <asp:BoundField DataField="Username" HeaderText="Username" />
                        <asp:BoundField DataField="RoleName" HeaderText="Role" />
                        <asp:BoundField DataField="ActionType" HeaderText="Action" />
                        <asp:BoundField DataField="PageUrl" HeaderText="Page" />
                        <asp:BoundField DataField="Details" HeaderText="Details" />
                    </Columns>
                </asp:GridView>
            </div>

        </div>
    </div>

</asp:Content>
