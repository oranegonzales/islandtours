<%@ Page Title="Overview" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="invenman._Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-heading">
        <div>
            <span class="eyebrow">Operations overview</span>
            <h1>Plan the day’s tours.</h1>
            <p class="page-intro"><%: DateTime.Now.ToString("dddd, d MMMM yyyy") %></p>
        </div>
    </section>

    <section class="overview-grid">
        <a runat="server" href="~/BookingUpcomingTours.aspx">
            <span>Bookings</span>
            <strong>Review upcoming tours</strong>
        </a>
        <a runat="server" href="~/BookingAssignTransportation.aspx">
            <span>Transportation</span>
            <strong>Plan vehicle assignments</strong>
        </a>
        <a runat="server" href="~/PaymentHistory.aspx">
            <span>Payments</span>
            <strong>Reconcile payment history</strong>
        </a>
        <a runat="server" href="~/ReportsSales.aspx">
            <span>Reporting</span>
            <strong>Examine sales performance</strong>
        </a>
    </section>
</asp:Content>
