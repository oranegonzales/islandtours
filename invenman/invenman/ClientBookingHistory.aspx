<%@ Page Title="Client Booking History" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="ClientBookingHistory.aspx.cs"
    Inherits="invenman.ClientBookingHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Client booking history</h2>

            <div class="mb-3">
                <label for="ddlClients" class="form-label">Select client</label>
                <asp:DropDownList ID="ddlClients" runat="server" CssClass="form-select" />
            </div>

            <asp:Button ID="btnLoadHistory" runat="server" Text="Load booking history"
                CssClass="btn btn-warning fw-bold mb-3"
                OnClick="btnLoadHistory_Click" />

            <asp:GridView ID="gvHistory" runat="server"
                CssClass="table table-dark table-striped table-sm"
                AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="Booking ID" />
                    <asp:BoundField DataField="AttractionName" HeaderText="Attraction" />
                    <asp:BoundField DataField="BookingDate" HeaderText="Booked"
                        DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="TourDate" HeaderText="Tour date"
                        DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="TotalAmount" HeaderText="Total (JMD)"
                        DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="PaymentStatus" HeaderText="Payment status" />
                    <asp:BoundField DataField="BookingStatus" HeaderText="Booking status" />
                </Columns>
            </asp:GridView>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
