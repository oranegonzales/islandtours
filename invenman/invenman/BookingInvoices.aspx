<%@ Page Title="Booking invoices" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingInvoices.aspx.cs"
    Inherits="invenman.BookingInvoices" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3 text-light">Generate invoices</h2>

            <asp:Label ID="lblPageMode" runat="server" CssClass="d-block mb-2 text-info"></asp:Label>
            <asp:Label ID="lblMessage" runat="server" CssClass="d-block mb-3"></asp:Label>

            <div class="row g-3 mb-3">
                <div class="col-md-3">
                    <label for="txtFromDate" class="form-label text-light">Tour date from</label>
                    <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                <div class="col-md-3">
                    <label for="txtToDate" class="form-label text-light">Tour date to</label>
                    <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                <div class="col-md-3">
                    <label for="ddlPaymentStatus" class="form-label text-light">Payment status</label>
                    <asp:DropDownList ID="ddlPaymentStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="All" Value=""></asp:ListItem>
                        <asp:ListItem Text="Pending" Value="Pending"></asp:ListItem>
                        <asp:ListItem Text="Paid" Value="Paid"></asp:ListItem>
                        <asp:ListItem Text="Cancelled" Value="Cancelled"></asp:ListItem>
                        <asp:ListItem Text="Partially paid" Value="Partially paid"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-3 d-flex align-items-end">
                    <asp:Button ID="btnFilter" runat="server" Text="Apply filter"
                        CssClass="btn btn-warning w-100"
                        OnClick="btnFilter_Click" />
                </div>
            </div>

            <asp:GridView ID="gvBookings" runat="server"
                CssClass="table table-dark table-striped table-bordered table-sm"
                AutoGenerateColumns="False"
                DataKeyNames="BookingID"
                OnRowCommand="gvBookings_RowCommand"
                EmptyDataText="No bookings found."
                GridLines="None">
                <Columns>
                    <asp:ButtonField Text="View invoice" CommandName="ViewInvoice" />
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                    <asp:BoundField DataField="TourDate" HeaderText="Tour date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" />
                    <asp:BoundField DataField="AttractionName" HeaderText="Attraction" />
                    <asp:BoundField DataField="TotalAmount" HeaderText="Total (JMD)" DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="PaidToDate" HeaderText="Paid to date (JMD)" DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="Balance" HeaderText="Balance (JMD)" DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="PaymentStatus" HeaderText="Payment status" />
                    <asp:BoundField DataField="BookingStatus" HeaderText="Booking status" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>