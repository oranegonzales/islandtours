<%@ Page Title="View All Attractions" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="AttractionList.aspx.cs"
    Inherits="invenman.AttractionList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">View all attractions</h2>

            <asp:GridView ID="gvAttractions" runat="server"
                CssClass="table table-dark table-striped table-sm"
                AutoGenerateColumns="False"
                DataKeyNames="AttractionID"
                AllowPaging="True" PageSize="15"
                OnPageIndexChanging="gvAttractions_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="AttractionID" HeaderText="ID" />
                    <asp:BoundField DataField="Name" HeaderText="Attraction" />
                    <asp:BoundField DataField="Parish" HeaderText="Parish" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />
                    <asp:BoundField DataField="BasePrice" HeaderText="Base price"
                        DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="ChildPrice" HeaderText="Child price"
                        DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="OpenDays" HeaderText="Open days" />
                    <asp:CheckBoxField DataField="IsActive" HeaderText="Active" />
                    <asp:HyperLinkField HeaderText="Edit"
                        Text="Edit"
                        DataNavigateUrlFields="AttractionID"
                        DataNavigateUrlFormatString="AttractionAddEdit.aspx?id={0}" />
                </Columns>
            </asp:GridView>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
