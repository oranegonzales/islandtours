<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportsAttractionPopularity.aspx.cs" Inherits="invenman.ReportsAttractionPopularity" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="p-2">
        <h2 class="mb-2" style="font-weight:800;">Attraction Popularity Report</h2>
        <div class="mb-3" style="color:#cbd5e1;">Most popular attractions based on bookings within a selected date range.</div>

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
                        <label class="form-label" style="color:#cbd5e1;">Parish</label>
                        <asp:DropDownList ID="ddlParish" runat="server" CssClass="form-select"></asp:DropDownList>
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
                        <div style="color:#93c5fd; font-weight:800; font-size:12px; letter-spacing:0.8px; text-transform:uppercase;">Most popular</div>
                        <div style="font-size:16px; font-weight:900;">
                            <asp:Label ID="lblTopAttraction" runat="server" Text="N/A"></asp:Label>
                        </div>
                        <div style="color:#cbd5e1; font-size:13px;">
                            <asp:Label ID="lblTopAttractionMeta" runat="server" Text=""></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <asp:Label ID="lblEmpty" runat="server" Visible="false" CssClass="mt-3 d-block" style="color:#cbd5e1;"></asp:Label>

        <div class="mt-3">
            <asp:GridView ID="gvPopularity" runat="server" AutoGenerateColumns="false"
                CssClass="table table-dark table-striped align-middle"
                GridLines="None">
                <Columns>
                    <asp:BoundField DataField="RankNo" HeaderText="#" />
                    <asp:BoundField DataField="Name" HeaderText="Attraction" />
                    <asp:BoundField DataField="Parish" HeaderText="Parish" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />
                    <asp:BoundField DataField="BookingsCount" HeaderText="Bookings" />
                    <asp:BoundField DataField="TotalRevenue" HeaderText="Revenue (JMD)" DataFormatString="{0:N2}" />
                    <asp:TemplateField HeaderText="Share">
                        <ItemTemplate>
                            <div style="min-width: 180px;">
                                <div class="progress" style="height: 10px; background: rgba(255,255,255,0.10);">
                                    <div class="progress-bar bg-warning" role="progressbar"
                                         style='<%# "width:" + Eval("ShareWidth") + "%;" %>'
                                         aria-valuenow='<%# Eval("ShareWidth") %>' aria-valuemin="0" aria-valuemax="100"></div>
                                </div>
                                <div style="font-size: 12px; color:#cbd5e1; margin-top:6px;">
                                    <%# Eval("ShareText") %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>