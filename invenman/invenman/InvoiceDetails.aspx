<%@ Page Title="Invoice details" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="InvoiceDetails.aspx.cs"
    Inherits="invenman.InvoiceDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlInvoice" runat="server">
        <div class="card bg-transparent border-0">
            <div class="card-body">
                <asp:Label ID="lblError" runat="server" CssClass="d-block mb-3 text-danger"></asp:Label>

                <div class="d-flex justify-content-between align-items-start mb-3">
                    <div>
                        <h3 class="text-light mb-1">IslandExplore Jamaica Tours</h3>
                        <div class="text-secondary" style="font-size: 0.9rem;">
                            <div>Email: info@islandexplorejamaica.com</div>
                            <div>Phone: +1 (876) 555-1234</div>
                            <div>Jamaica, W.I.</div>
                        </div>
                    </div>
                    <div class="text-end">
                        <h3 class="text-warning mb-1">Invoice</h3>
                        <div class="text-secondary" style="font-size: 0.9rem;">
                            <div>Invoice number: <asp:Label ID="lblInvoiceNumber" runat="server"></asp:Label></div>
                            <div>Invoice date: <asp:Label ID="lblInvoiceDate" runat="server"></asp:Label></div>
                        </div>
                    </div>
                </div>

                <hr class="border-secondary" />

                <div class="row mb-3">
                    <div class="col-md-6">
                        <h5 class="text-light">Bill to</h5>
                        <div class="card bg-dark border-secondary">
                            <div class="card-body p-3">
                                <div class="text-light"><asp:Label ID="lblClientName" runat="server"></asp:Label></div>
                                <div class="text-secondary" style="font-size: 0.9rem;">
                                    <div>Email: <asp:Label ID="lblClientEmail" runat="server"></asp:Label></div>
                                    <div>Phone: <asp:Label ID="lblClientPhone" runat="server"></asp:Label></div>
                                    <div>Country: <asp:Label ID="lblClientCountry" runat="server"></asp:Label></div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-6">
                        <h5 class="text-light">Booking summary</h5>
                        <div class="card bg-dark border-secondary">
                            <div class="card-body p-3 text-secondary" style="font-size: 0.9rem;">
                                <div>Booking ID: <asp:Label ID="lblBookingId" runat="server"></asp:Label></div>
                                <div>Booking date: <asp:Label ID="lblBookingDate" runat="server"></asp:Label></div>
                                <div>Tour date: <asp:Label ID="lblTourDate" runat="server"></asp:Label></div>
                                <div>Attraction: <asp:Label ID="lblAttractionName" runat="server"></asp:Label></div>
                                <div>Location: <asp:Label ID="lblAttractionLocation" runat="server"></asp:Label></div>
                                <div>Booking status: <asp:Label ID="lblBookingStatus" runat="server"></asp:Label></div>
                                <div>Payment status: <asp:Label ID="lblPaymentStatus" runat="server"></asp:Label></div>
                                <div>Transport provider: <asp:Label ID="lblTransportProvider" runat="server"></asp:Label></div>
                                <div>Pickup location: <asp:Label ID="lblPickupLocation" runat="server"></asp:Label></div>
                                <div>Pickup date and time: <asp:Label ID="lblPickupDateTime" runat="server"></asp:Label></div>
                            </div>
                        </div>
                    </div>
                </div>

                <h5 class="text-light mt-3">Invoice amounts</h5>
                <div class="row mb-3">
                    <div class="col-md-6">
                        <div class="card bg-dark border-secondary">
                            <div class="card-body p-3">
                                <h6 class="text-light mb-2">Amounts in JMD</h6>
                                <table class="table table-sm table-dark mb-0">
                                    <tr>
                                        <td>Total</td>
                                        <td class="text-end">
                                            <asp:Label ID="lblTotalJmd" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Paid to date</td>
                                        <td class="text-end">
                                            <asp:Label ID="lblPaidJmd" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Balance due</td>
                                        <td class="text-end">
                                            <asp:Label ID="lblBalanceJmd" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-6">
                        <div class="card bg-dark border-secondary">
                            <div class="card-body p-3">
                                <h6 class="text-light mb-2">Approximate amounts in USD</h6>
                                <table class="table table-sm table-dark mb-0">
                                    <tr>
                                        <td>Total</td>
                                        <td class="text-end">
                                            <asp:Label ID="lblTotalUsd" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Paid to date</td>
                                        <td class="text-end">
                                            <asp:Label ID="lblPaidUsd" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Balance due</td>
                                        <td class="text-end">
                                            <asp:Label ID="lblBalanceUsd" runat="server"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                                <div class="text-secondary mt-2" style="font-size: 0.8rem;">
                                    <asp:Label ID="lblRateInfo" runat="server"></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <h5 class="text-light mt-3">Payments</h5>
                <asp:GridView ID="gvPayments" runat="server"
                    CssClass="table table-dark table-striped table-bordered table-sm"
                    AutoGenerateColumns="False"
                    EmptyDataText="No payments recorded yet."
                    GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="PaymentDate" HeaderText="Payment date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                        <asp:BoundField DataField="Amount" HeaderText="Amount (JMD)" DataFormatString="{0:N2}" />
                        <asp:BoundField DataField="PaymentMethod" HeaderText="Method" />
                        <asp:BoundField DataField="TransactionReference" HeaderText="Reference" />
                    </Columns>
                </asp:GridView>

                <div class="mt-4 text-secondary" style="font-size: 0.9rem;">
                    Thank you for booking with IslandExplore Jamaica Tours. Please present this invoice on the day of your tour.
                </div>

                <div class="mt-3 d-flex gap-2">
                    <asp:Button ID="btnPrint" runat="server" Text="Print invoice"
                        CssClass="btn btn-warning"
                        OnClientClick="window.print(); return false;" />
                    <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/BookingInvoices.aspx"
                        CssClass="btn btn-secondary">Back to invoices</asp:HyperLink>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>