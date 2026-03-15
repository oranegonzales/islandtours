<%@ Page Title="Staff Login" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="invenman.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="p-3">
        <h2 class="mb-3" style="font-weight:800;">Staff Login</h2>

        <asp:Label ID="lblMessage" runat="server" ForeColor="#ffb4b4"></asp:Label>

        <div class="mt-3" style="max-width:520px;">
            <div class="mb-3">
                <label class="form-label" style="color:#cbd5e1;">Username</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" ErrorMessage="Username is required." ForeColor="#ffb4b4" Display="Dynamic" />
            </div>

            <div class="mb-3">
                <label class="form-label" style="color:#cbd5e1;">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required." ForeColor="#ffb4b4" Display="Dynamic" />
            </div>

            <div class="form-check mb-3">
                <input id="chkStaySignedIn" runat="server" type="checkbox" class="form-check-input" />
                <asp:Label ID="lblStaySignedIn" runat="server" AssociatedControlID="chkStaySignedIn" CssClass="form-check-label" Text="Stay signed in"></asp:Label>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="tt-btn" OnClick="btnLogin_Click" />

            <div class="mt-3">
                <a class="tt-btn" href="ClientLogin.aspx">Go to Client Login</a>
            </div>
        </div>
    </div>
</asp:Content>
