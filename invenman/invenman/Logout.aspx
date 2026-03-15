<%@ Page Title="Logout" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="invenman.Logout" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="p-3">
        <h2 class="mb-3" style="font-weight:800;">Logout</h2>

        <asp:Label ID="lblStatus" runat="server"></asp:Label>

        <div class="mt-3" style="max-width:520px;">
            <asp:Button ID="btnConfirmLogout" runat="server" Text="Confirm Logout" CssClass="tt-btn" OnClick="btnConfirmLogout_Click" />
            <div class="mt-3">
                <a class="tt-btn" href="Home.aspx">Cancel and go to Home</a>
            </div>
        </div>
    </div>
</asp:Content>
