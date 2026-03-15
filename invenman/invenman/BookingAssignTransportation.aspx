<%@ Page Title="Assign Transportation" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingAssignTransportation.aspx.cs"
    Inherits="invenman.BookingAssignTransportation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3 text-light">Assign transportation</h2>

            <asp:Label ID="lblMode" runat="server" CssClass="mb-2 d-block text-info"></asp:Label>
            <asp:Label ID="lblMessage" runat="server" CssClass="mb-3 d-block"></asp:Label>

            <asp:GridView ID="gvBookings" runat="server"
                CssClass="table table-dark table-striped table-bordered table-sm"
                AutoGenerateColumns="False"
                DataKeyNames="BookingID"
                OnSelectedIndexChanged="gvBookings_SelectedIndexChanged"
                EmptyDataText="No upcoming bookings found."
                GridLines="None">
                <Columns>
                    <asp:CommandField ShowSelectButton="True" SelectText="Select" />
                    <asp:BoundField DataField="BookingID" HeaderText="ID" />
                    <asp:BoundField DataField="TourDate" HeaderText="Tour date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" />
                    <asp:BoundField DataField="AttractionName" HeaderText="Attraction" />
                    <asp:BoundField DataField="Parish" HeaderText="Parish" />
                    <asp:BoundField DataField="TransportProvider" HeaderText="Transport provider" />
                    <asp:BoundField DataField="PickupLocation" HeaderText="Pickup location" />
                    <asp:BoundField DataField="PickupDateTime" HeaderText="Pickup date and time" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                    <asp:BoundField DataField="BookingStatus" HeaderText="Booking status" />
                </Columns>
            </asp:GridView>

            <asp:Panel ID="pnlStaffEditor" runat="server" CssClass="mt-4">
                <asp:HiddenField ID="hfSelectedBookingId" runat="server" />

                <div class="row g-3">
                    <div class="col-md-4">
                        <label for="ddlTransportProvider" class="form-label text-light">Transport provider</label>
                        <asp:DropDownList ID="ddlTransportProvider" runat="server" CssClass="form-select">
                            <asp:ListItem Text="-- Select provider --" Value=""></asp:ListItem>
                            <asp:ListItem Text="IslandExplore Shuttle" Value="IslandExplore Shuttle"></asp:ListItem>
                            <asp:ListItem Text="Yaad Vibes Transport" Value="Yaad Vibes Transport"></asp:ListItem>
                            <asp:ListItem Text="Sunrise Tours Transfers" Value="Sunrise Tours Transfers"></asp:ListItem>
                            <asp:ListItem Text="Blue Lagoon Transfers" Value="Blue Lagoon Transfers"></asp:ListItem>
                            <asp:ListItem Text="Chukka Transport" Value="Chukka Transport"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-4">
                        <label for="txtPickupLocation" class="form-label text-light">Pickup location</label>
                        <asp:TextBox ID="txtPickupLocation" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="col-md-2">
                        <label for="txtPickupDate" class="form-label text-light">Pickup date</label>
                        <asp:TextBox ID="txtPickupDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>

                    <div class="col-md-2">
                        <label for="txtPickupTime" class="form-label text-light">Pickup time</label>
                        <asp:TextBox ID="txtPickupTime" runat="server" CssClass="form-control" TextMode="Time"></asp:TextBox>
                    </div>

                    <div class="col-12">
                        <label for="txtNotes" class="form-label text-light">Transport notes</label>
                        <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                    </div>

                    <div class="col-12 d-flex gap-2">
                        <asp:Button ID="btnSaveAssignment" runat="server" Text="Save assignment"
                            CssClass="btn btn-warning"
                            OnClick="btnSaveAssignment_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="Clear"
                            CssClass="btn btn-secondary"
                            OnClick="btnClear_Click" CausesValidation="False" />
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>