<%@ Page Title="Add Client" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="ClientAdd.aspx.cs" Inherits="invenman.ClientAdd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Add client information</h2>

            <asp:ValidationSummary ID="vsClientAdd" runat="server" CssClass="text-danger mb-3" />

            <div class="mb-3">
                <label for="txtFirstName" class="form-label">First name</label>
                <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvFirstName" runat="server"
                    ControlToValidate="txtFirstName" ErrorMessage="First name is required."
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="mb-3">
                <label for="txtLastName" class="form-label">Last name</label>
                <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvLastName" runat="server"
                    ControlToValidate="txtLastName" ErrorMessage="Last name is required."
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="mb-3">
                <label for="txtEmail" class="form-label">Email</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                    ControlToValidate="txtEmail" ErrorMessage="Email is required."
                    CssClass="text-danger" Display="Dynamic" />
                <asp:RegularExpressionValidator ID="revEmail" runat="server"
                    ControlToValidate="txtEmail"
                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                    ErrorMessage="Enter a valid email address."
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="mb-3">
                <label for="txtPhone" class="form-label">Phone</label>
                <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" />
            </div>

            <div class="mb-3">
                <label for="txtCountry" class="form-label">Country</label>
                <asp:TextBox ID="txtCountry" runat="server" CssClass="form-control" />
            </div>

            <asp:Button ID="btnSave" runat="server" Text="Save client"
                CssClass="btn btn-warning fw-bold"
                OnClick="btnSave_Click" />

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-3 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
