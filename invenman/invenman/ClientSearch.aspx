<%@ Page Title="Search Clients" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="ClientSearch.aspx.cs" Inherits="invenman.ClientSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Search clients</h2>

            <div class="row g-2 align-items-end mb-3">
                <div class="col-md-6">
                    <label for="txtSearch" class="form-label">Search term</label>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control"
                        placeholder="Name, email or phone" />
                </div>
                <div class="col-md-3">
                    <asp:Button ID="btnSearch" runat="server" Text="Search"
                        CssClass="btn btn-warning fw-bold w-100"
                        OnClick="btnSearch_Click" />
                </div>
            </div>

            <asp:GridView ID="gvResults" runat="server"
                CssClass="table table-dark table-striped table-sm"
                AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="ClientID" HeaderText="ID" />
                    <asp:BoundField DataField="FirstName" HeaderText="First name" />
                    <asp:BoundField DataField="LastName" HeaderText="Last name" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Phone" HeaderText="Phone" />
                    <asp:BoundField DataField="Country" HeaderText="Country" />
                </Columns>
            </asp:GridView>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
