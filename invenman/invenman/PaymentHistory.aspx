<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaymentHistory.aspx.cs"
    Inherits="invenman.PaymentHistory" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card tt-card mt-3">
        <div class="card-body">
            <h2 class="mb-2">Payment history</h2>
            <p class="mb-3 text-secondary">
                <asp:Label ID="lblIntro" runat="server"
                    Text="Staff users see all payments. Clients see only their own payments."></asp:Label>
            </p>

            <asp:Label ID="lblError" runat="server" CssClass="text-danger mb-2 d-block"></asp:Label>

            <div class="table-responsive">
                <asp:GridView ID="gvPayments" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-dark table-striped tt-table"
                    GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="PaymentID" HeaderText="Payment ID" />
                        <asp:BoundField DataField="PaymentDate" HeaderText="Date and time"
                            DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                        <asp:BoundField DataField="ClientName" HeaderText="Client" />
                        <asp:BoundField DataField="AmountJMD" HeaderText="Amount (JMD)"
                            DataFormatString="{0:N2}" HtmlEncode="False" />
                        <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
                        <asp:BoundField DataField="CurrencyCode" HeaderText="Currency" />
                        <asp:TemplateField HeaderText="Receipt">
                            <ItemTemplate>
                                <asp:HyperLink runat="server"
                                    Text="View receipt"
                                    CssClass="link-info"
                                    NavigateUrl='<%# "~/PaymentReceipt.aspx?paymentId=" + Eval("PaymentID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>