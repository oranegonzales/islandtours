<%@ Page Title="Client List" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="ClientList.aspx.cs" Inherits="invenman.ClientList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Client list</h2>

            <asp:GridView ID="gvClients" runat="server"
                CssClass="table table-dark table-striped table-sm"
                AutoGenerateColumns="False"
                AllowPaging="True" PageSize="15"
                OnPageIndexChanging="gvClients_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="ClientID" HeaderText="ID" />
                    <asp:BoundField DataField="FirstName" HeaderText="First name" />
                    <asp:BoundField DataField="LastName" HeaderText="Last name" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Phone" HeaderText="Phone" />
                    <asp:BoundField DataField="Country" HeaderText="Country" />
                    <asp:BoundField DataField="DateCreated" HeaderText="Created"
                        DataFormatString="{0:yyyy-MM-dd}" />
                </Columns>
                <PagerStyle CssClass="pagination" />
            </asp:GridView>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
