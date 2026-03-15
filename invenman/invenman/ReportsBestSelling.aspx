<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportsBestSelling.aspx.cs" Inherits="invenman.ReportsBestSelling" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="p-2">
        <h2 class="mb-2" style="font-weight:800;">Best Selling Tour Packages</h2>
        <div class="mb-3" style="color:#cbd5e1;">Top attractions by bookings and revenue for a selected date range.</div>

        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger">
            <asp:Label ID="lblError" runat="server"></asp:Label>
        </asp:Panel>

        <div class="card" style="background: rgba(2, 6, 23, 0.45); border: 1px solid rgba(255,255,255,0.10); border-radius: 14px;">
            <div class="card-body">
                <div class="row g-3 align-items-end">
                    <div class="col-12 col-md-3">
                        <label class="form-label" style="color:#cbd5e1;">From date</label>
                        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>

                    <div class="col-12 col-md-3">
                        <label class="form-label" style="color:#cbd5e1;">To date</label>
                        <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>

                    <div class="col-12 col-md-3">
                        <label class="form-label" style="color:#cbd5e1;">Top</label>
                        <asp:DropDownList ID="ddlTop" runat="server" CssClass="form-select">
                            <asp:ListItem Text="5" Value="5"></asp:ListItem>
                            <asp:ListItem Text="10" Value="10" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="20" Value="20"></asp:ListItem>
                            <asp:ListItem Text="50" Value="50"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-12 col-md-3">
                        <asp:Button ID="btnGenerate" runat="server" Text="Generate" CssClass="btn btn-warning w-100" OnClick="btnGenerate_Click" />
                    </div>
                </div>

                <div class="mt-3" style="color:#cbd5e1; font-size: 13px;">
                    <span style="font-weight:800;">Filters:</span>
                    <asp:Label ID="lblFilterSummary" runat="server" Text=""></asp:Label>
                </div>
            </div>
        </div>

        <div class="mt-3">
            <div class="row g-3">
                <div class="col-12 col-md-4">
                    <div class="p-3" style="border: 1px solid rgba(255,255,255,0.10); border-radius: 14px; background: rgba(17, 24, 39, 0.85);">
                        <div style="color:#93c5fd; font-weight:800; font-size:12px; letter-spacing:0.8px; text-transform:uppercase;">Total bookings</div>
                        <div style="font-size:26px; font-weight:900;">
                            <asp:Label ID="lblTotalBookings" runat="server" Text="0"></asp:Label>
                        </div>
                    </div>
                </div>

                <div class="col-12 col-md-4">
                    <div class="p-3" style="border: 1px solid rgba(255,255,255,0.10); border-radius: 14px; background: rgba(17, 24, 39, 0.85);">
                        <div style="color:#93c5fd; font-weight:800; font-size:12px; letter-spacing:0.8px; text-transform:uppercase;">Total revenue (JMD)</div>
                        <div style="font-size:26px; font-weight:900;">
                            <asp:Label ID="lblTotalRevenue" runat="server" Text="0.00"></asp:Label>
                        </div>
                    </div>
                </div>

                <div class="col-12 col-md-4">
                    <div class="p-3" style="border: 1px solid rgba(255,255,255,0.10); border-radius: 14px; background: rgba(17, 24, 39, 0.85);">
                        <div style="color:#93c5fd; font-weight:800; font-size:12px; letter-spacing:0.8px; text-transform:uppercase;">Average booking (JMD)</div>
                        <div style="font-size:26px; font-weight:900;">
                            <asp:Label ID="lblAvgBooking" runat="server" Text="0.00"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <asp:Label ID="lblEmpty" runat="server" Visible="false" CssClass="mt-3 d-block" style="color:#cbd5e1;"></asp:Label>

        <div class="mt-3">
            <asp:GridView ID="gvBestSelling" runat="server" AutoGenerateColumns="false"
                CssClass="table table-dark table-striped align-middle"
                GridLines="None">
                <Columns>
                    <asp:BoundField DataField="RankNo" HeaderText="#" />
                    <asp:BoundField DataField="PackageName" HeaderText="Package" />
                    <asp:BoundField DataField="Parish" HeaderText="Parish" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />
                    <asp:BoundField DataField="BookingsCount" HeaderText="Bookings" />
                    <asp:BoundField DataField="TotalRevenue" HeaderText="Revenue (JMD)" DataFormatString="{0:N2}" />
                    <asp:BoundField DataField="AvgRevenue" HeaderText="Avg (JMD)" DataFormatString="{0:N2}" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>