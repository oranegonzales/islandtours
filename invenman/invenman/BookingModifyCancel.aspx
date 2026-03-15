<%@ Page Title="Modify or Cancel Booking" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="BookingModifyCancel.aspx.cs"
    Inherits="invenman.BookingModifyCancel" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Modify or cancel booking</h2>

            <asp:Label ID="lblMode" runat="server" CssClass="mb-2 d-block text-info"></asp:Label>

            <asp:GridView ID="gvBookings" runat="server"
                CssClass="table table-dark table-striped table-sm"
                AutoGenerateColumns="False"
                DataKeyNames="BookingID"
                OnRowEditing="gvBookings_RowEditing"
                OnRowCancelingEdit="gvBookings_RowCancelingEdit"
                OnRowUpdating="gvBookings_RowUpdating"
                OnRowCommand="gvBookings_RowCommand">
                <Columns>
                    <asp:BoundField DataField="BookingID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="ClientName" HeaderText="Client" ReadOnly="True" />
                    <asp:BoundField DataField="AttractionName" HeaderText="Attraction" ReadOnly="True" />
                    <asp:BoundField DataField="TourDate" HeaderText="Tour date" DataFormatString="{0:yyyy-MM-dd}" ReadOnly="True" />
                    <asp:BoundField DataField="TotalAmount" HeaderText="Total (JMD)" DataFormatString="{0:N2}" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Payment status">
                        <ItemTemplate>
                            <%# Eval("PaymentStatus") %>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:DropDownList ID="ddlPaymentStatusEdit" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Pending" Value="Pending" />
                                <asp:ListItem Text="Paid" Value="Paid" />
                                <asp:ListItem Text="Cancelled" Value="Cancelled" />
                            </asp:DropDownList>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Booking status">
                        <ItemTemplate>
                            <%# Eval("BookingStatus") %>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:DropDownList ID="ddlBookingStatusEdit" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Active" Value="Active" />
                                <asp:ListItem Text="Completed" Value="Completed" />
                                <asp:ListItem Text="Cancelled" Value="Cancelled" />
                                <asp:ListItem Text="Transport assigned" Value="Transport assigned" />
                            </asp:DropDownList>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" />
                    <asp:ButtonField ButtonType="Button" CommandName="CancelBooking" Text="Cancel booking" />
                </Columns>
            </asp:GridView>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
