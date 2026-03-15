<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PublicAttractions.aspx.cs"
    Inherits="invenman.PublicAttractions" MasterPageFile="~/Site.Master" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Jamaican Attractions</h2>

    <asp:Label ID="lblFilter" runat="server" Text="Filter by parish: " />

    <asp:DropDownList ID="ddlParish" runat="server" AutoPostBack="True"
        OnSelectedIndexChanged="ddlParish_SelectedIndexChanged">
        <asp:ListItem Value="">All Parishes</asp:ListItem>
        <asp:ListItem>St. Ann</asp:ListItem>
        <asp:ListItem>St. James</asp:ListItem>
        <asp:ListItem>Trelawny</asp:ListItem>
        <asp:ListItem>Kingston</asp:ListItem>
        <asp:ListItem>Portland</asp:ListItem>
        <asp:ListItem>St. Elizabeth</asp:ListItem>
        <asp:ListItem>Westmoreland</asp:ListItem>
        <asp:ListItem>Clarendon</asp:ListItem>
        <asp:ListItem>St. Catherine</asp:ListItem>
    </asp:DropDownList>

    <br /><br />

    <asp:GridView ID="gvAttractions" runat="server"
        AutoGenerateColumns="True" CssClass="table table-striped table-dark" />

    <br />

    <asp:Label ID="lblError" runat="server" ForeColor="Red" />
</asp:Content>
