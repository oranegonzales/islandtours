<%@ Page Title="View Upcoming Tours" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingUpcomingTours.aspx.cs"
    Inherits="invenman.BookingUpcomingTours" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Upcoming tours</h2>

            <asp:Label ID="lblMode" runat="server" CssClass="mb-2 d-block text-info"></asp:Label>

            <asp:Label ID="lblMessage" runat="server" CssClass="mb-3 d-block"></asp:Label>

            <asp:GridView ID="gvUpcomingTours" runat="server"
                CssClass="table table-dark table-striped table-bordered table-sm"
                AutoGenerateColumns="False"
                EmptyDataText="No upcoming tours found."
                GridLines="None">
                <Columns>
                    <asp:BoundField DataField="TourDate" HeaderText="Tour date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" />
                    <asp:BoundField DataField="AttractionName" HeaderText="Attraction" />
                    <asp:BoundField DataField="Parish" HeaderText="Parish" />
                    <asp:BoundField DataField="TotalAmount" HeaderText="Total (JMD)" DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="PaymentStatus" HeaderText="Payment status" />
                    <asp:BoundField DataField="BookingStatus" HeaderText="Booking status" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
