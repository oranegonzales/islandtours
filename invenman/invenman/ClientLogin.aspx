<%@ Page Title="Client sign in" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="ClientLogin.aspx.cs" Inherits="invenman.ClientLogin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-layout">
        <div class="auth-copy">
            <span class="eyebrow">Your itinerary</span>
            <h1>Pick up where you left off.</h1>
            <p>Review bookings, pickup details, invoices, and payment history using the account connected to your client record.</p>
        </div>

        <div class="auth-form">
            <h2>Client sign in</h2>
            <asp:Label ID="lblMessage" runat="server" CssClass="notice notice-error"></asp:Label>

            <div class="field">
                <label for="<%= txtUsername.ClientID %>">Username or email</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="100" autocomplete="username" />
                <asp:RequiredFieldValidator ID="rfvUsername" runat="server"
                    ControlToValidate="txtUsername"
                    ErrorMessage="Enter your username or email."
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
            <p class="auth-switch">Part of the operations team? <a runat="server" href="~/Login.aspx">Use staff sign in</a>.</p>
        </div>
    </section>
</asp:Content>
