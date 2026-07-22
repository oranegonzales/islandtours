<%@ Page Title="Staff sign in" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="invenman.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-layout">
        <div class="auth-copy">
            <span class="eyebrow">Staff access</span>
            <h1>Welcome back.</h1>
            <p>Sign in to manage client records, bookings, transportation, payments, and reporting.</p>
        </div>

        <div class="auth-form">
            <h2>Staff sign in</h2>
            <asp:Label ID="lblMessage" runat="server" CssClass="notice notice-error"></asp:Label>

            <div class="field">
                <label for="<%= txtUsername.ClientID %>">Username</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="100" autocomplete="username" />
                <asp:RequiredFieldValidator ID="rfvUsername" runat="server"
                    ControlToValidate="txtUsername"
                    ErrorMessage="Enter your username."
                    CssClass="field-error"
                    Display="Dynamic" />
            </div>

            <div class="field">
                <label for="<%= txtPassword.ClientID %>">Password</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="256" autocomplete="current-password" />
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                    ControlToValidate="txtPassword"
                    ErrorMessage="Enter your password."
                    CssClass="field-error"
                    Display="Dynamic" />
            </div>

            <div class="check-field">
                <input id="chkStaySignedIn" runat="server" type="checkbox" />
                <asp:Label ID="lblStaySignedIn" runat="server" AssociatedControlID="chkStaySignedIn" Text="Keep me signed in on this device"></asp:Label>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="Sign in"
                CssClass="button button-primary auth-submit"
                OnClick="btnLogin_Click" />
            <p class="auth-switch">Travelling with us? <a runat="server" href="~/ClientLogin.aspx">Use client sign in</a>.</p>
        </div>
    </section>
</asp:Content>
