<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaymentRecord.aspx.cs" Inherits="invenman.PaymentRecord" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Make payment</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="text-danger"></asp:Label><br />
    <asp:Label ID="lblSuccess" runat="server" CssClass="text-success"></asp:Label>

    <table style="margin-top:15px;">
        <tr>
            <td style="padding:4px 8px;">Select booking</td>
            <td style="padding:4px 8px;">
                <asp:DropDownList ID="ddlBooking" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBooking_SelectedIndexChanged"></asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td style="padding:4px 8px;">Payment method</td>
            <td style="padding:4px 8px;">
                <asp:DropDownList ID="ddlPaymentMethod" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPaymentMethod_SelectedIndexChanged">
                    <asp:ListItem Text="Card" Value="Card"></asp:ListItem>
                    <asp:ListItem Text="Cash at office" Value="Cash"></asp:ListItem>
                    <asp:ListItem Text="Bank transfer" Value="Bank"></asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td style="padding:4px 8px;">Payment amount (JMD)</td>
            <td style="padding:4px 8px;">
                <asp:TextBox ID="txtAmount" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvAmount" runat="server"
                    ControlToValidate="txtAmount"
                    ErrorMessage="Amount is required"
                    Display="Dynamic"
                    CssClass="text-danger"
                    ValidationGroup="CardPayment"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revAmount" runat="server"
                    ControlToValidate="txtAmount"
                    ErrorMessage="Enter a valid amount like 4500 or 4500.50"
                    ValidationExpression="^[0-9]+(\.[0-9]{1,2})?$"
                    Display="Dynamic"
                    CssClass="text-danger"
                    ValidationGroup="CardPayment"></asp:RegularExpressionValidator>
            </td>
        </tr>
    </table>

    <asp:Panel ID="pnlInstructions" runat="server" Visible="false" Style="margin-top:10px; border:1px solid #888; padding:8px;">
        <asp:Label ID="lblInstructions" runat="server"></asp:Label>
    </asp:Panel>

    <asp:Panel ID="pnlCardDetails" runat="server" Style="margin-top:16px; border:1px solid #888; padding:10px;">
        <h3>Card details</h3>

        <table>
            <tr>
                <td style="padding:4px 8px;">Name on card</td>
                <td style="padding:4px 8px;">
                    <asp:TextBox ID="txtCardName" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvCardName" runat="server"
                        ControlToValidate="txtCardName"
                        ErrorMessage="Name on card is required"
                        Display="Dynamic"
                        CssClass="text-danger"
                        ValidationGroup="CardPayment"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td style="padding:4px 8px;">Card number</td>
                <td style="padding:4px 8px;">
                    <asp:TextBox ID="txtCardNumber" runat="server" MaxLength="16"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="revCardNumber" runat="server"
                        ControlToValidate="txtCardNumber"
                        ErrorMessage="Enter a valid 16 digit card number"
                        ValidationExpression="^[0-9]{16}$"
                        Display="Dynamic"
                        CssClass="text-danger"
                        ValidationGroup="CardPayment"></asp:RegularExpressionValidator>
                </td>
            </tr>
            <tr>
                <td style="padding:4px 8px;">Expiry (MM/YY)</td>
                <td style="padding:4px 8px;">
                    <asp:TextBox ID="txtExpiry" runat="server" MaxLength="5"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="revExpiry" runat="server"
                        ControlToValidate="txtExpiry"
                        ErrorMessage="Enter expiry as MM/YY, year not past 55"
                        ValidationExpression="^(0[1-9]|1[0-2])\/([2-4][0-9]|5[0-5])$"
                        Display="Dynamic"
                        CssClass="text-danger"
                        ValidationGroup="CardPayment"></asp:RegularExpressionValidator>
                </td>
            </tr>
            <tr>
                <td style="padding:4px 8px;">Security code</td>
                <td style="padding:4px 8px;">
                    <asp:TextBox ID="txtCvv" runat="server" MaxLength="4"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="revCvv" runat="server"
                        ControlToValidate="txtCvv"
                        ErrorMessage="Enter a 3 or 4 digit code"
                        ValidationExpression="^[0-9]{3,4}$"
                        Display="Dynamic"
                        CssClass="text-danger"
                        ValidationGroup="CardPayment"></asp:RegularExpressionValidator>
                </td>
            </tr>
            <tr>
                <td style="padding:4px 8px;">Billing address</td>
                <td style="padding:4px 8px;">
                    <asp:TextBox ID="txtBillingAddress" runat="server" Width="260px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvBillingAddress" runat="server"
                        ControlToValidate="txtBillingAddress"
                        ErrorMessage="Billing address is required"
                        Display="Dynamic"
                        CssClass="text-danger"
                        ValidationGroup="CardPayment"></asp:RequiredFieldValidator>
                </td>
            </tr>
        </table>
    </asp:Panel>

    <div style="margin-top:16px;">
        <asp:Button ID="btnPay" runat="server" Text="Pay now" CssClass="btn btn-warning"
            OnClick="btnPay_Click" ValidationGroup="CardPayment" />
    </div>
</asp:Content>