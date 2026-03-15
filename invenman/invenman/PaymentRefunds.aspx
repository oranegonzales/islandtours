<%@ Page Title="Issue refund" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PaymentRefunds.aspx.cs" Inherits="invenman.PaymentRefunds" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-dark text-light border-0 shadow-sm">
        <div class="card-body">
            <h2 class="mb-4">Issue refund</h2>

            <div class="mb-3">
                <label for="ddlPayments" class="form-label">Select payment to refund</label>
                <asp:DropDownList ID="ddlPayments" runat="server"
                    CssClass="form-select bg-dark text-light border border-secondary">
                </asp:DropDownList>
            </div>

            <div class="mb-3">
                <label for="txtRefundAmount" class="form-label">Refund amount (JMD)</label>
                <asp:TextBox ID="txtRefundAmount" runat="server"
                    CssClass="form-control bg-dark text-light border border-secondary"></asp:TextBox>
                <asp:RequiredFieldValidator ID="valRefundAmountRequired" runat="server"
                    ControlToValidate="txtRefundAmount"
                    CssClass="text-danger small"
                    Display="Dynamic"
                    ErrorMessage="Enter a refund amount."></asp:RequiredFieldValidator>
            </div>

            <div class="mb-3">
                <label for="txtReason" class="form-label">Reason for refund</label>
                <asp:TextBox ID="txtReason" runat="server"
                    CssClass="form-control bg-dark text-light border border-secondary"
                    TextMode="MultiLine" Rows="4"></asp:TextBox>
            </div>

            <asp:Button ID="btnProcessRefund" runat="server"
                CssClass="btn btn-warning fw-bold"
                Text="Process refund"
                OnClick="btnProcessRefund_Click" />

            <asp:Label ID="lblStatus" runat="server"
                Visible="false"></asp:Label>
        </div>
    </div>
</asp:Content>