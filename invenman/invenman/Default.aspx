<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="invenman._Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Dashboard</h1>
    <p>Today is <%: DateTime.Now.ToString("dddd, MMMM d, yyyy") %>.</p>

    <h2>Inventory Snapshot</h2>
    <ul>
        <li>Total products: 128</li>
        <li>Low stock items: 7</li>
        <li>Last backup: Yesterday 6:00 PM</li>
    </ul>

    <h2>System Notices</h2>
    <ul>
        <li>Use consistent product names.</li>
        <li>Verify unit numbers before saving.</li>
        <li>Each user should use their own account.</li>
    </ul>
</asp:Content>
