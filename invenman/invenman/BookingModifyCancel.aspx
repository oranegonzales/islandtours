<%@ Page Title="Booking changes" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingModifyCancel.aspx.cs"
    Inherits="invenman.BookingModifyCancel" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-heading">
        <div>
            <span class="eyebrow">Bookings</span>
            <h1>Changes and cancellations</h1>
            <asp:Label ID="lblMode" runat="server" CssClass="page-intro"></asp:Label>
        </div>
    </section>

    <asp:Label ID="lblMessage" runat="server" CssClass="notice"></asp:Label>
    <section class="surface">
        <div class="table-wrap">
            <asp:GridView ID="gvBookings" runat="server"
                CssClass="data-table"
                AutoGenerateColumns="False"
                DataKeyNames="BookingID"
                EmptyDataText="No current bookings found."
                OnRowEditing="gvBookings_RowEditing"
                OnRowCancelingEdit="gvBookings_RowCancelingEdit"
                OnRowUpdating="gvBookings_RowUpdating"
                OnRowCommand="gvBookings_RowCommand"
                GridLines="None">
                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" ReadOnly="True" />
                    <asp:BoundField DataField="AttractionName" HeaderText="Attraction" ReadOnly="True" />
                    <asp:BoundField DataField="TourDate" HeaderText="Tour date" DataFormatString="{0:dd MMM yyyy}" ReadOnly="True" />
                    <asp:BoundField DataField="TotalAmount" HeaderText="Total (JMD)" DataFormatString="{0:N2}" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Payment">
                        <ItemTemplate><%#: Eval("PaymentStatus") %></ItemTemplate>
                        <EditItemTemplate>
                            <asp:DropDownList ID="ddlPaymentStatusEdit" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Pending" Value="Pending" />
                                <asp:ListItem Text="Paid" Value="Paid" />
                                <asp:ListItem Text="Cancelled" Value="Cancelled" />
                            </asp:DropDownList>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Booking">
                        <ItemTemplate><%#: Eval("BookingStatus") %></ItemTemplate>
                        <EditItemTemplate>
                            <asp:DropDownList ID="ddlBookingStatusEdit" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Active" Value="Active" />
                                <asp:ListItem Text="Completed" Value="Completed" />
                                <asp:ListItem Text="Cancelled" Value="Cancelled" />
                                <asp:ListItem Text="Transport assigned" Value="Transport assigned" />
                            </asp:DropDownList>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ControlStyle-CssClass="table-action" />
                    <asp:ButtonField ButtonType="Link" CommandName="CancelBooking" Text="Cancel" ControlStyle-CssClass="table-action" />
                </Columns>
            </asp:GridView>
        </div>
    </section>
</asp:Content>
