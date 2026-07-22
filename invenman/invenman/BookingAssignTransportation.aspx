<%@ Page Title="Transportation planning" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingAssignTransportation.aspx.cs"
    Inherits="invenman.BookingAssignTransportation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-heading">
        <div>
            <span class="eyebrow">Operations</span>
            <h1>Transportation planning</h1>
            <asp:Label ID="lblMode" runat="server" CssClass="page-intro"></asp:Label>
        </div>
    </section>

    <asp:Label ID="lblMessage" runat="server" CssClass="notice"></asp:Label>

    <asp:Panel ID="pnlPlanningTools" runat="server">
        <section class="planning-callout">
            <div>
                <h2>Plan the upcoming fleet</h2>
                <p>Assigns active vehicles by capacity, route time, and schedule conflicts. Large batches use a bounded-cost heuristic; smaller batches are optimized exactly.</p>
            </div>
            <asp:Button ID="btnAutoPlan" runat="server" Text="Plan next 30 days"
                CssClass="button button-primary"
                OnClick="btnAutoPlan_Click"
                CausesValidation="False" />
        </section>
        <asp:Label ID="lblPlanSummary" runat="server" CssClass="notice"></asp:Label>
    </asp:Panel>

    <section class="surface">
        <div class="section-heading">
            <div>
                <span class="eyebrow">Schedule</span>
                <h2>Upcoming bookings</h2>
            </div>
            <p>Select a row to review or change its assignment.</p>
        </div>

        <div class="table-wrap">
            <asp:GridView ID="gvBookings" runat="server"
                CssClass="data-table"
                AutoGenerateColumns="False"
                DataKeyNames="BookingID"
                OnSelectedIndexChanged="gvBookings_SelectedIndexChanged"
                EmptyDataText="No upcoming bookings found."
                GridLines="None">
                <Columns>
                    <asp:CommandField ShowSelectButton="True" SelectText="Review" ControlStyle-CssClass="table-action" />
                    <asp:BoundField DataField="BookingID" HeaderText="ID" />
                    <asp:BoundField DataField="TourDate" HeaderText="Tour date" DataFormatString="{0:dd MMM yyyy}" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" />
                    <asp:BoundField DataField="AttractionName" HeaderText="Attraction" />
                    <asp:BoundField DataField="Parish" HeaderText="Destination" />
                    <asp:BoundField DataField="PartySize" HeaderText="Party" />
                    <asp:BoundField DataField="TransportProvider" HeaderText="Vehicle" NullDisplayText="Unassigned" />
                    <asp:BoundField DataField="PickupLocation" HeaderText="Pickup" NullDisplayText="Not set" />
                    <asp:BoundField DataField="PickupDateTime" HeaderText="Pickup time" DataFormatString="{0:dd MMM, HH:mm}" NullDisplayText="Not set" />
                    <asp:BoundField DataField="BookingStatus" HeaderText="Status" />
                </Columns>
            </asp:GridView>
        </div>
    </section>

    <asp:Panel ID="pnlStaffEditor" runat="server" CssClass="surface editor-surface">
        <asp:HiddenField ID="hfSelectedBookingId" runat="server" />

        <div class="section-heading">
            <div>
                <span class="eyebrow">Manual assignment</span>
                <h2>Review selected booking</h2>
            </div>
            <p>All pickup fields are required before saving.</p>
        </div>

        <div class="form-grid">
            <div class="field field-span-2">
                <label for="<%= ddlTransportProvider.ClientID %>">Transport provider</label>
                <asp:DropDownList ID="ddlTransportProvider" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Select a provider" Value=""></asp:ListItem>
                    <asp:ListItem Text="Island Shuttle Co." Value="Island Shuttle Co."></asp:ListItem>
                    <asp:ListItem Text="North Coast Transit" Value="North Coast Transit"></asp:ListItem>
                    <asp:ListItem Text="Sunrise Tours Transfers" Value="Sunrise Tours Transfers"></asp:ListItem>
                    <asp:ListItem Text="Blue Lagoon Transfers" Value="Blue Lagoon Transfers"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="field field-span-2">
                <label for="<%= txtPickupLocation.ClientID %>">Pickup location</label>
                <asp:TextBox ID="txtPickupLocation" runat="server" CssClass="form-control" MaxLength="200"></asp:TextBox>
            </div>

            <div class="field">
                <label for="<%= txtPickupDate.ClientID %>">Pickup date</label>
                <asp:TextBox ID="txtPickupDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
            </div>

            <div class="field">
                <label for="<%= txtPickupTime.ClientID %>">Pickup time</label>
                <asp:TextBox ID="txtPickupTime" runat="server" CssClass="form-control" TextMode="Time"></asp:TextBox>
            </div>

            <div class="field field-span-full">
                <label for="<%= txtNotes.ClientID %>">Transport notes</label>
                <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="1000"></asp:TextBox>
            </div>
        </div>

        <div class="button-row">
            <asp:Button ID="btnSaveAssignment" runat="server" Text="Save assignment"
                CssClass="button button-primary"
                OnClick="btnSaveAssignment_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear selection"
                CssClass="button button-secondary"
                OnClick="btnClear_Click" CausesValidation="False" />
        </div>
    </asp:Panel>
</asp:Content>
