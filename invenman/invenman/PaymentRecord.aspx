<%@ Page Title="Payments" Language="C#" AutoEventWireup="true"
    CodeBehind="PaymentRecord.aspx.cs" Inherits="invenman.PaymentRecord"
    MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-heading">
        <div>
            <span class="eyebrow">Payments</span>
            <h1>Payment options</h1>
            <asp:Label ID="lblMode" runat="server" CssClass="page-intro"></asp:Label>
        </div>
    </section>

    <asp:Label ID="lblMessage" runat="server" CssClass="notice"></asp:Label>

    <section class="surface">
        <div class="form-grid">
            <div class="field field-span-full">
                <label for="<%= ddlBooking.ClientID %>">Booking and outstanding balance</label>
                <asp:DropDownList ID="ddlBooking" runat="server" CssClass="form-select"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlBooking_SelectedIndexChanged"></asp:DropDownList>
            </div>

            <div class="field field-span-2">
                <label for="<%= ddlPaymentMethod.ClientID %>">Payment method</label>
                <asp:DropDownList ID="ddlPaymentMethod" runat="server" CssClass="form-select"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlPaymentMethod_SelectedIndexChanged"></asp:DropDownList>
            </div>

            <div class="field">
                <label for="<%= txtAmount.ClientID %>">Amount (JMD)</label>
                <asp:TextBox ID="txtAmount" runat="server" CssClass="form-control"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvAmount" runat="server"
                    ControlToValidate="txtAmount" ErrorMessage="Enter an amount."
                    Display="Dynamic" CssClass="field-error"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revAmount" runat="server"
                    ControlToValidate="txtAmount" ErrorMessage="Enter a valid amount."
                    ValidationExpression="^[0-9]+(\.[0-9]{1,2})?$"
                    Display="Dynamic" CssClass="field-error"></asp:RegularExpressionValidator>
            </div>

            <div class="field">
                <label for="<%= txtTransactionReference.ClientID %>">Transaction reference</label>
                <asp:TextBox ID="txtTransactionReference" runat="server" CssClass="form-control" MaxLength="200"></asp:TextBox>
                <span class="field-note">Required for bank transfers and card-terminal records.</span>
            </div>
        </div>

        <asp:Panel ID="pnlInstructions" runat="server" CssClass="notice notice-neutral">
            <asp:Label ID="lblInstructions" runat="server"></asp:Label>
        </asp:Panel>

        <div class="button-row">
            <asp:Button ID="btnPay" runat="server" Text="Record payment"
                CssClass="button button-primary" OnClick="btnPay_Click" />
        </div>
    </section>
</asp:Content>
