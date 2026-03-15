<%@ Page Title="UserMgmt" Language="C#" MasterPageFile="~/Site.Master" %>

<asp:Content ID="HeadStuff" ContentPlaceHolderID="HeadContent" runat="server">
    <meta name="description" content="Clients input form" />
</asp:Content>

<asp:Content ID="BodyStuff" ContentPlaceHolderID="MainContent" runat="server">
    <h1>UserMgmt</h1>
    <p>Enter client information for the Clients table.</p>

    <div>
        <div>
            <strong>ClientID:</strong>
            <span>Auto-generated</span>
        </div>

        <div style="margin-top:10px;">
            <label for="txtFirstName">First Name</label><br />
            <asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox>
        </div>

        <div style="margin-top:10px;">
            <label for="txtLastName">Last Name</label><br />
            <asp:TextBox ID="txtLastName" runat="server"></asp:TextBox>
        </div>

        <div style="margin-top:10px;">
            <label for="txtEmail">Email</label><br />
            <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
        </div>

        <div style="margin-top:10px;">
            <label for="txtPhone">Phone</label><br />
            <asp:TextBox ID="txtPhone" runat="server"></asp:TextBox>
        </div>

        <div style="margin-top:10px;">
            <label for="txtCountry">Country</label><br />
            <asp:TextBox ID="txtCountry" runat="server"></asp:TextBox>
        </div>

        <div style="margin-top:10px;">
            <strong>DateCreated:</strong>
            <span>Auto-generated</span>
        </div>

        <div style="margin-top:14px;">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" />
        </div>
    </div>
</asp:Content>
