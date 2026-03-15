<%@ Page Title="Sales Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReportsSales.aspx.cs" Inherits="invenman.ReportsSales" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-page">
        <h2 class="mb-3" style="font-weight:800;">Sales Reports</h2>

        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger">
            <asp:Label ID="lblError" runat="server"></asp:Label>
        </asp:Panel>

        <div class="card mb-3" style="background: rgba(2,6,23,0.45); border: 1px solid rgba(255,255,255,0.10); border-radius: 14px;">
            <div class="card-body">
                <div class="row g-3 align-items-end">
                    <div class="col-12 col-md-3">
                        <label class="form-label" style="color:#cbd5e1; font-weight:700;">From</label>
                        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="col-12 col-md-3">
                        <label class="form-label" style="color:#cbd5e1; font-weight:700;">To</label>
                        <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="col-12 col-md-3">
                        <label class="form-label" style="color:#cbd5e1; font-weight:700;">Group by</label>
                        <asp:DropDownList ID="ddlGroupBy" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Daily" Value="Daily" />
                            <asp:ListItem Text="Monthly" Value="Monthly" />
                            <asp:ListItem Text="Yearly" Value="Yearly" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-12 col-md-3">
                        <label class="form-label" style="color:#cbd5e1; font-weight:700;">Payment status</label>
                        <asp:DropDownList ID="ddlPaymentStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="All" Value="" />
                            <asp:ListItem Text="Paid" Value="Paid" />
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-12">
                        <asp:Button ID="btnGenerate" runat="server" Text="Generate report" CssClass="tt-btn" OnClick="btnGenerate_Click" />
                    </div>
                </div>
            </div>
        </div>

        <div class="row g-3 mb-3">
            <div class="col-12 col-md-3">
                <div class="card" style="background: rgba(17,24,39,0.85); border: 1px solid rgba(255,255,255,0.10); border-radius: 14px;">
                    <div class="card-body">
                        <div style="color:#93c5fd; font-weight:800; text-transform:uppercase; letter-spacing:0.8px; font-size:12px;">Total sales (JMD)</div>
                        <asp:Label ID="lblTotalSales" runat="server" Text="0.00" style="font-size:22px; font-weight:900;"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="col-12 col-md-3">
                <div class="card" style="background: rgba(17,24,39,0.85); border: 1px solid rgba(255,255,255,0.10); border-radius: 14px;">
                    <div class="card-body">
                        <div style="color:#93c5fd; font-weight:800; text-transform:uppercase; letter-spacing:0.8px; font-size:12px;">Payments count</div>
                        <asp:Label ID="lblPaymentsCount" runat="server" Text="0" style="font-size:22px; font-weight:900;"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="col-12 col-md-3">
                <div class="card" style="background: rgba(17,24,39,0.85); border: 1px solid rgba(255,255,255,0.10); border-radius: 14px;">
                    <div class="card-body">
                        <div style="color:#93c5fd; font-weight:800; text-transform:uppercase; letter-spacing:0.8px; font-size:12px;">Average payment</div>
                        <asp:Label ID="lblAvgPayment" runat="server" Text="0.00" style="font-size:22px; font-weight:900;"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="col-12 col-md-3">
                <div class="card" style="background: rgba(17,24,39,0.85); border: 1px solid rgba(255,255,255,0.10); border-radius: 14px;">
                    <div class="card-body">
                        <div style="color:#93c5fd; font-weight:800; text-transform:uppercase; letter-spacing:0.8px; font-size:12px;">Current filter</div>
                        <asp:Label ID="lblFilterSummary" runat="server" Text="None" style="font-size:14px; font-weight:800; color:#cbd5e1;"></asp:Label>
                    </div>
                </div>
            </div>
        </div>

        <div class="card" style="background: rgba(2,6,23,0.45); border: 1px solid rgba(255,255,255,0.10); border-radius: 14px;">
            <div class="card-body">
                <asp:GridView ID="gvSales" runat="server" AutoGenerateColumns="False" CssClass="table table-dark table-striped" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="PeriodLabel" HeaderText="Period" />
                        <asp:BoundField DataField="PaymentsCount" HeaderText="Payments" />
                        <asp:BoundField DataField="TotalAmount" HeaderText="Total (JMD)" DataFormatString="{0:N2}" />
                        <asp:BoundField DataField="AverageAmount" HeaderText="Average (JMD)" DataFormatString="{0:N2}" />
                    </Columns>
                </asp:GridView>

                <asp:Label ID="lblEmpty" runat="server" Visible="false" CssClass="text-warning" style="font-weight:800;"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>