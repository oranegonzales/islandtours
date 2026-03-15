<%@ Page Title="Add or Edit Attraction" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="AttractionAddEdit.aspx.cs"
    Inherits="invenman.AttractionAddEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <asp:Label ID="lblTitle" runat="server" Text="Add attraction" CssClass="h2 mb-3 d-block"></asp:Label>

            <asp:ValidationSummary ID="vsAttraction" runat="server" CssClass="text-danger mb-3" />

            <asp:HiddenField ID="hfAttractionID" runat="server" />

            <div class="mb-3">
                <label for="txtName" class="form-label">Attraction name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvName" runat="server"
                    ControlToValidate="txtName"
                    ErrorMessage="Attraction name is required."
                    CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="mb-3">
                <label for="txtDescription" class="form-label">Description</label>
                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine"
                    Rows="4" CssClass="form-control" />
            </div>

            <div class="row g-3">
                <div class="col-md-4">
                    <label for="txtParish" class="form-label">Parish</label>
                    <asp:TextBox ID="txtParish" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvParish" runat="server"
                        ControlToValidate="txtParish"
                        ErrorMessage="Parish is required."
                        CssClass="text-danger" Display="Dynamic" />
                </div>
                <div class="col-md-4">
                    <label for="txtCategory" class="form-label">Category</label>
                    <asp:TextBox ID="txtCategory" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-4">
                    <label for="txtOpenDays" class="form-label">Open days</label>
                    <asp:TextBox ID="txtOpenDays" runat="server" CssClass="form-control"
                        Placeholder="Mon Sun" />
                </div>
            </div>

            <div class="row g-3 mt-3">
                <div class="col-md-4">
                    <label for="txtBasePrice" class="form-label">Base price (JMD)</label>
                    <asp:TextBox ID="txtBasePrice" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvBasePrice" runat="server"
                        ControlToValidate="txtBasePrice"
                        ErrorMessage="Base price is required."
                        CssClass="text-danger" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="revBasePrice" runat="server"
                        ControlToValidate="txtBasePrice"
                        ValidationExpression="^\d+(\.\d{1,2})?$"
                        ErrorMessage="Enter a valid amount."
                        CssClass="text-danger" Display="Dynamic" />
                </div>
                <div class="col-md-4">
                    <label for="txtChildPrice" class="form-label">Child price (JMD)</label>
                    <asp:TextBox ID="txtChildPrice" runat="server" CssClass="form-control" />
                    <asp:RegularExpressionValidator ID="revChildPrice" runat="server"
                        ControlToValidate="txtChildPrice"
                        ValidationExpression="^\d*(\.\d{1,2})?$"
                        ErrorMessage="Enter a valid amount or leave blank."
                        CssClass="text-danger" Display="Dynamic" />
                </div>
                <div class="col-md-4">
                    <label class="form-label d-block">Active</label>
                    <asp:CheckBox ID="chkIsActive" runat="server" CssClass="form-check-input" />
                </div>
            </div>

            <div class="mt-4 d-flex gap-2">
                <asp:Button ID="btnSave" runat="server" Text="Save attraction"
                    CssClass="btn btn-warning fw-bold"
                    OnClick="btnSave_Click" />
                <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/AttractionList.aspx"
                    CssClass="btn btn-outline-light">Back to list</asp:HyperLink>
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-3 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
