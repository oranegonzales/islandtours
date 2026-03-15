<%@ Page Title="Client Demographics" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReportsClientDemographics.aspx.cs" Inherits="invenman.ReportsClientDemographics" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .tt-card {
            background: rgba(17, 24, 39, 0.85);
            border: 1px solid rgba(255,255,255,0.10);
            border-radius: 14px;
            padding: 16px;
        }
        .tt-title {
            font-size: 22px;
            font-weight: 800;
            margin-bottom: 6px;
        }
        .tt-sub {
            color: #cbd5e1;
            margin-bottom: 14px;
        }
        .tt-label {
            font-weight: 700;
            color: #e5e7eb;
            margin-bottom: 6px;
        }
        .tt-input, .tt-select {
            background: rgba(2, 6, 23, 0.35);
            border: 1px solid rgba(255,255,255,0.15);
            color: #e5e7eb;
            border-radius: 10px;
            padding: 10px 12px;
            width: 100%;
        }
        .tt-input:focus, .tt-select:focus {
            outline: none;
            box-shadow: 0 0 0 3px rgba(255,183,3,0.25);
            border-color: rgba(255,183,3,0.55);
        }
        .tt-btn {
            background: #ffb703;
            color: #0b1220;
            font-weight: 800;
            border: none;
            border-radius: 10px;
            padding: 10px 14px;
        }
        .tt-btn:hover {
            background: #fb8500;
            color: #0b1220;
        }
        .tt-msg {
            margin-top: 12px;
            padding: 12px;
            border-radius: 12px;
            border: 1px solid rgba(255,255,255,0.10);
            background: rgba(2, 6, 23, 0.45);
            color: #cbd5e1;
        }
        .tt-error {
            border-color: rgba(248,113,113,0.35);
            background: rgba(127,29,29,0.25);
            color: #fecaca;
        }
        .tt-table {
            margin-top: 14px;
            border-radius: 12px;
            overflow: hidden;
        }
        .tt-help {
            font-size: 12.5px;
            color: #cbd5e1;
            margin-top: 6px;
        }
        .tt-validation {
            color: #fecaca;
            font-size: 12.5px;
            display: block;
            margin-top: 6px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-card">
        <div class="tt-title">Client Demographics</div>
        <div class="tt-sub">Generate quick demographic summaries using Clients and Bookings data.</div>

        <div class="row g-3">
            <div class="col-12 col-md-4">
                <div class="tt-label">Report type</div>
                <asp:DropDownList ID="ddlReportType" runat="server" CssClass="tt-select">
                    <asp:ListItem Value="Country">Country breakdown</asp:ListItem>
                    <asp:ListItem Value="NewClients">New clients by month</asp:ListItem>
                    <asp:ListItem Value="ActiveInactive">Active vs inactive clients</asp:ListItem>
                    <asp:ListItem Value="TopSpenders">Top clients by total spend</asp:ListItem>
                    <asp:ListItem Value="MostBookings">Top clients by bookings count</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-12 col-md-4">
                <div class="tt-label">From date</div>
                <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="tt-input"></asp:TextBox>
                <div class="tt-help">Optional. Leave blank for all-time.</div>
            </div>

            <div class="col-12 col-md-4">
                <div class="tt-label">To date</div>
                <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="tt-input"></asp:TextBox>
                <div class="tt-help">Optional. Leave blank for all-time.</div>
            </div>

            <div class="col-12">
                <asp:Button ID="btnGenerate" runat="server" Text="Generate report" CssClass="tt-btn" OnClick="btnGenerate_Click" />
                <asp:Label ID="lblMessage" runat="server" CssClass="tt-msg" Visible="false"></asp:Label>
            </div>
        </div>

        <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="True" CssClass="table table-dark table-striped tt-table"
            GridLines="None" BorderStyle="None">
        </asp:GridView>
    </div>
</asp:Content>