<%@ Page Title="Attraction Status" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="AttractionStatus.aspx.cs"
    Inherits="invenman.AttractionStatus" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card bg-transparent border-0">
        <div class="card-body">
            <h2 class="mb-3">Deactivate or activate attractions</h2>

            <asp:GridView ID="gvAttractions" runat="server"
                CssClass="table table-dark table-striped table-sm"
                AutoGenerateColumns="False"
                DataKeyNames="AttractionID"
                OnRowCommand="gvAttractions_RowCommand">
                <Columns>
                    <asp:BoundField DataField="AttractionID" HeaderText="ID" />
                    <asp:BoundField DataField="Name" HeaderText="Attraction" />
                    <asp:BoundField DataField="Parish" HeaderText="Parish" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />
                    <asp:CheckBoxField DataField="IsActive" HeaderText="Active" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnToggle" runat="server"
                                CommandName="Toggle"
                                CommandArgument='<%# Eval("AttractionID") %>'
                                Text='<%# (bool)Eval("IsActive") ? "Deactivate" : "Activate" %>'
                                CssClass="btn btn-warning btn-sm" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <asp:Label ID="lblMessage" runat="server" CssClass="mt-2 d-block"></asp:Label>
        </div>
    </div>
</asp:Content>
