<%@ Page Title="Attraction Pricing" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="AttractionPricing.aspx.cs"
    Inherits="invenman.AttractionPricing" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Set attraction base pricing</h2>

            <asp:GridView ID="gvPricing" runat="server"
                CssClass="table table-dark table-striped table-sm"
                AutoGenerateColumns="False"
                DataKeyNames="AttractionID"
                OnRowEditing="gvPricing_RowEditing"
                OnRowCancelingEdit="gvPricing_RowCancelingEdit"
                OnRowUpdating="gvPricing_RowUpdating">
                <Columns>
                    <asp:BoundField DataField="AttractionID" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="Name" HeaderText="Attraction" ReadOnly="True" />
                    <asp:BoundField DataField="Parish" HeaderText="Parish" ReadOnly="True" />
                    <asp:BoundField DataField="Category" HeaderText="Category" ReadOnly="True" />
                    <asp:BoundField DataField="BasePrice" HeaderText="Base price"
                        DataFormatString="{0:N2}" ReadOnly="False" />
                    <asp:BoundField DataField="ChildPrice" HeaderText="Child price"
                        DataFormatString="{0:N2}" ReadOnly="False" />
                    <asp:CommandField ShowEditButton="True" />
                </Columns>
            </asp:GridView>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
