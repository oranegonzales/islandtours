<%@ Page Title="Create booking" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingCreate.aspx.cs"
    Inherits="invenman.BookingCreate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-heading">
        <div>
            <span class="eyebrow">Bookings</span>
            <h1>Create a booking</h1>
            <asp:Label ID="lblMode" runat="server" CssClass="page-intro"></asp:Label>
        </div>
    </section>

    <section class="surface">
        <asp:ValidationSummary ID="vsBooking" runat="server" CssClass="notice notice-error" />
        <asp:Label ID="lblMessage" runat="server" CssClass="notice"></asp:Label>

        <div class="form-grid">
            <div class="field field-span-2">
                <label for="<%= ddlClient.ClientID %>">Client</label>
                <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-select"></asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvClient" runat="server"
                    ControlToValidate="ddlClient" InitialValue=""
                    ErrorMessage="Select a client." CssClass="field-error" Display="Dynamic" />
            </div>

            <div class="field field-span-2">
                <label for="<%= ddlAttraction.ClientID %>">Attraction</label>
                <asp:DropDownList ID="ddlAttraction" runat="server" CssClass="form-select"></asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvAttraction" runat="server"
                    ControlToValidate="ddlAttraction" InitialValue=""
                    ErrorMessage="Select an attraction." CssClass="field-error" Display="Dynamic" />
            </div>

            <div class="field">
                <label for="<%= txtTourDate.ClientID %>">Tour date</label>
                <asp:TextBox ID="txtTourDate" runat="server" CssClass="form-control" TextMode="Date" />
                <asp:RequiredFieldValidator ID="rfvTourDate" runat="server"
                    ControlToValidate="txtTourDate" ErrorMessage="Enter a tour date."
                    CssClass="field-error" Display="Dynamic" />
            </div>

            <div class="field">
                <label for="<%= txtPartySize.ClientID %>">Party size</label>
                <asp:TextBox ID="txtPartySize" runat="server" CssClass="form-control" TextMode="Number" min="1" max="50" />
                <asp:RequiredFieldValidator ID="rfvPartySize" runat="server"
                    ControlToValidate="txtPartySize" ErrorMessage="Enter the party size."
                    CssClass="field-error" Display="Dynamic" />
                <asp:RangeValidator ID="rvPartySize" runat="server"
                    ControlToValidate="txtPartySize" Type="Integer" MinimumValue="1" MaximumValue="50"
                    ErrorMessage="Party size must be from 1 to 50." CssClass="field-error" Display="Dynamic" />
            </div>

            <div class="field">
                <label for="<%= txtTotalAmount.ClientID %>">Total amount (JMD)</label>
                <asp:TextBox ID="txtTotalAmount" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvTotalAmount" runat="server"
                    ControlToValidate="txtTotalAmount" ErrorMessage="Enter the total amount."
                    CssClass="field-error" Display="Dynamic" />
                <asp:RegularExpressionValidator ID="revTotalAmount" runat="server"
                    ControlToValidate="txtTotalAmount" ValidationExpression="^\d+(\.\d{1,2})?$"
                    ErrorMessage="Enter a valid amount." CssClass="field-error" Display="Dynamic" />
                <asp:Label ID="lblClientAmountNote" runat="server" CssClass="field-note"></asp:Label>
            </div>

            <div class="field">
                <label for="<%= ddlPaymentStatus.ClientID %>">Payment status</label>
                <asp:DropDownList ID="ddlPaymentStatus" runat="server" CssClass="form-select">
                    <asp:ListItem Text="Pending" Value="Pending" />
                    <asp:ListItem Text="Paid" Value="Paid" />
                    <asp:ListItem Text="Cancelled" Value="Cancelled" />
                </asp:DropDownList>
            </div>

            <div class="field">
                <label for="<%= ddlBookingStatus.ClientID %>">Booking status</label>
                <asp:DropDownList ID="ddlBookingStatus" runat="server" CssClass="form-select">
                    <asp:ListItem Text="Active" Value="Active" />
                    <asp:ListItem Text="Completed" Value="Completed" />
                    <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    <asp:ListItem Text="Transport assigned" Value="Transport assigned" />
                </asp:DropDownList>
            </div>
        </div>

        <div class="button-row">
            <asp:Button ID="btnSave" runat="server" Text="Create booking"
                CssClass="button button-primary" OnClick="btnSave_Click" />
            <asp:HyperLink ID="lnkViewBookings" runat="server"
                NavigateUrl="~/BookingModifyCancel.aspx"
                CssClass="button button-secondary">View bookings</asp:HyperLink>
        </div>
    </section>
</asp:Content>
