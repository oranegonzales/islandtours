<%@ Page Title="Create New Booking" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingCreate.aspx.cs"
    Inherits="invenman.BookingCreate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Create new booking</h2>

            <asp:Label ID="lblMode" runat="server" CssClass="mb-2 d-block text-info"></asp:Label>

            <asp:ValidationSummary ID="vsBooking" runat="server" CssClass="text-danger mb-3" />

            <div class="mb-3">
                <label for="ddlClient" class="form-label">Client</label>
                <asp:DropDownList ID="ddlClient" runat="server" CssClass="form-select">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvClient" runat="server"
                    ControlToValidate="ddlClient"
                    InitialValue=""
                    ErrorMessage="Please select a client."
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="mb-3">
                <label for="ddlAttraction" class="form-label">Attraction</label>
                <asp:DropDownList ID="ddlAttraction" runat="server" CssClass="form-select">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvAttraction" runat="server"
                    ControlToValidate="ddlAttraction"
                    InitialValue=""
                    ErrorMessage="Please select an attraction."
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="row g-3">
                <div class="col-md-4">
                    <label for="txtTourDate" class="form-label">Tour date</label>
                    <asp:TextBox ID="txtTourDate" runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:RequiredFieldValidator ID="rfvTourDate" runat="server"
                        ControlToValidate="txtTourDate"
                        ErrorMessage="Tour date is required."
                        CssClass="text-danger" Display="Dynamic" />
                </div>
                <div class="col-md-4">
                    <label for="txtTotalAmount" class="form-label">Total amount (JMD)</label>
                    <asp:TextBox ID="txtTotalAmount" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvTotalAmount" runat="server"
                        ControlToValidate="txtTotalAmount"
                        ErrorMessage="Total amount is required."
                        CssClass="text-danger" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="revTotalAmount" runat="server"
                        ControlToValidate="txtTotalAmount"
                        ValidationExpression="^\d+(\.\d{1,2})?$"
                        ErrorMessage="Enter a valid amount."
                        CssClass="text-danger" Display="Dynamic" />
                    <asp:Label ID="lblClientAmountNote" runat="server" CssClass="d-block text-muted"></asp:Label>
                </div>
                <div class="col-md-4">
                    <label for="ddlPaymentStatus" class="form-label">Payment status</label>
                    <asp:DropDownList ID="ddlPaymentStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Paid" Value="Paid" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="row g-3 mt-3">
                <div class="col-md-4">
                    <label for="ddlBookingStatus" class="form-label">Booking status</label>
                    <asp:DropDownList ID="ddlBookingStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Completed" Value="Completed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        <asp:ListItem Text="Transport assigned" Value="Transport assigned" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="mt-4 d-flex gap-2">
                <asp:Button ID="btnSave" runat="server" Text="Save booking"
                    CssClass="btn btn-warning fw-bold"
                    OnClick="btnSave_Click" />
                <asp:HyperLink ID="lnkViewBookings" runat="server"
                    NavigateUrl="~/BookingModifyCancel.aspx"
                    CssClass="btn btn-outline-light">View bookings</asp:HyperLink>
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-3 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
