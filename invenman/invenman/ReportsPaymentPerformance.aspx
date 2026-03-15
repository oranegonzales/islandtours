<%@ Page Title="Payment Performance Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReportsPaymentPerformance.aspx.cs" Inherits="invenman.ReportsPaymentPerformance" %>

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
            font-weight: 800;
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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-card">
        <div class="tt-title">Payment Performance Reports</div>
        <div class="tt-sub">Analyze payments over time, by method, and by booking payment status (if available).</div>

        <div class="row g-3">
            <div class="col-12 col-md-4">
                <div class="tt-label">Report view</div>
                <asp:DropDownList ID="ddlView" runat="server" CssClass="tt-select">
                    <asp:ListItem Value="PeriodSummary">Payments summary by period</asp:ListItem>
                    <asp:ListItem Value="MethodBreakdown">Payment method breakdown</asp:ListItem>
                    <asp:ListItem Value="StatusTrend">Booking payment status trend</asp:ListItem>
                </asp:DropDownList>
                <div class="tt-help">Status trend uses Bookings.PaymentStatus if it exists.</div>
            </div>

            <div class="col-12 col-md-4">
                <div class="tt-label">Grouping</div>
                <asp:DropDownList ID="ddlPeriod" runat="server" CssClass="tt-select">
                    <asp:ListItem Value="Daily">Daily</asp:ListItem>
                    <asp:ListItem Value="Monthly" Selected="True">Monthly</asp:ListItem>
                    <asp:ListItem Value="Yearly">Yearly</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-12 col-md-4">
                <div class="tt-label">Date filter mode</div>
                <asp:DropDownList ID="ddlDateMode" runat="server" CssClass="tt-select">
                    <asp:ListItem Value="AllTime" Selected="True">All time</asp:ListItem>
                    <asp:ListItem Value="Range">Date range</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-12 col-md-4">
                <div class="tt-label">From date</div>
                <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="tt-input"></asp:TextBox>
            </div>

            <div class="col-12 col-md-4">
                <div class="tt-label">To date</div>
                <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="tt-input"></asp:TextBox>
            </div>

            <div class="col-12 col-md-4 d-flex align-items-end">
                <asp:Button ID="btnGenerate" runat="server" Text="Generate report" CssClass="tt-btn w-100" OnClick="btnGenerate_Click" />
            </div>

            <div class="col-12">
                <asp:Label ID="lblMessage" runat="server" CssClass="tt-msg" Visible="false"></asp:Label>
            </div>
        </div>

        <asp:GridView ID="gvReport" runat="server" AutoGenerateColumns="True" CssClass="table table-dark table-striped tt-table"
            GridLines="None" BorderStyle="None">
        </asp:GridView>
    </div>
</asp:Content>