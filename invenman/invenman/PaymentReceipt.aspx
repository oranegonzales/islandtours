<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaymentReceipt.aspx.cs" Inherits="invenman.PaymentReceipt" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Payment Receipt</title>

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />

    <style type="text/css">
        body {
            margin: 0;
            font-family: Arial, Helvetica, sans-serif;
            background: #0b1220;
            color: #e5e7eb;
        }

        .receipt-wrap {
            max-width: 900px;
            margin: 24px auto;
            padding: 0 14px;
        }

        .receipt-header {
            background: #0f172a;
            border: 1px solid rgba(255,255,255,0.10);
            border-radius: 14px;
            padding: 14px;
            text-align: center;
        }

        .receipt-header .header-img {
            max-width: 100%;
            height: auto;
            display: inline-block;
            border-radius: 12px;
            border: 1px solid rgba(255,255,255,0.10);
        }

        .receipt-title {
            margin-top: 10px;
            font-size: 20px;
            font-weight: 800;
        }

        .receipt-card {
            margin-top: 14px;
            background: rgba(17, 24, 39, 0.85);
            border: 1px solid rgba(255,255,255,0.10);
            border-radius: 14px;
            padding: 14px;
        }

        .kv {
            display: grid;
            grid-template-columns: 180px 1fr;
            gap: 10px 12px;
        }

        .k {
            color: #cbd5e1;
            font-weight: 700;
            font-size: 13px;
        }

        .v {
            color: #e5e7eb;
            font-size: 13px;
            word-break: break-word;
        }

        .section-title {
            margin-top: 14px;
            margin-bottom: 10px;
            font-size: 12px;
            font-weight: 900;
            color: #93c5fd;
            text-transform: uppercase;
            letter-spacing: 0.8px;
        }

        .alert-darkish {
            background: rgba(2, 6, 23, 0.45);
            border: 1px solid rgba(255,255,255,0.10);
            color: #e5e7eb;
            border-radius: 12px;
            padding: 12px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="receipt-wrap">

            <div class="receipt-header">
                <asp:Image ID="imgHeader" runat="server" CssClass="header-img" ImageUrl="~/Images/header.png" AlternateText="Header" />
                <div class="receipt-title">IslandExplore Jamaica Tours</div>
            </div>

            <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="receipt-card">
                <div class="alert-darkish">
                    <asp:Label ID="lblError" runat="server"></asp:Label>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlReceipt" runat="server" Visible="false" CssClass="receipt-card">
                <div class="section-title">Receipt</div>

                <div class="kv">
                    <div class="k">Receipt number</div>
                    <div class="v"><asp:Label ID="lblReceiptNumber" runat="server"></asp:Label></div>

                    <div class="k">Payment date</div>
                    <div class="v"><asp:Label ID="lblPaymentDate" runat="server"></asp:Label></div>

                    <div class="k">Payment method</div>
                    <div class="v"><asp:Label ID="lblPaymentMethod" runat="server"></asp:Label></div>

                    <div class="k">Transaction ref</div>
                    <div class="v"><asp:Label ID="lblTxnRef" runat="server"></asp:Label></div>

                    <div class="k">Amount</div>
                    <div class="v"><asp:Label ID="lblAmountJmd" runat="server"></asp:Label></div>

                    <div class="k">Amount (USD)</div>
                    <div class="v"><asp:Label ID="lblAmountUsd" runat="server"></asp:Label></div>
                </div>

                <div class="section-title">Booking</div>

                <div class="kv">
                    <div class="k">Booking ID</div>
                    <div class="v"><asp:Label ID="lblBookingId" runat="server"></asp:Label></div>

                    <div class="k">Tour date</div>
                    <div class="v"><asp:Label ID="lblTourDate" runat="server"></asp:Label></div>

                    <div class="k">Booking status</div>
                    <div class="v"><asp:Label ID="lblBookingStatus" runat="server"></asp:Label></div>

                    <div class="k">Payment status</div>
                    <div class="v"><asp:Label ID="lblPaymentStatus" runat="server"></asp:Label></div>
                </div>

                <div class="section-title">Client</div>

                <div class="kv">
                    <div class="k">Client name</div>
                    <div class="v"><asp:Label ID="lblClientName" runat="server"></asp:Label></div>

                    <div class="k">Email</div>
                    <div class="v"><asp:Label ID="lblClientEmail" runat="server"></asp:Label></div>
                </div>

                <div class="section-title">Attraction</div>

                <div class="kv">
                    <div class="k">Attraction</div>
                    <div class="v"><asp:Label ID="lblAttractionName" runat="server"></asp:Label></div>

                    <div class="k">Location</div>
                    <div class="v"><asp:Label ID="lblAttractionLocation" runat="server"></asp:Label></div>
                </div>
            </asp:Panel>

        </div>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    </form>
</body>
</html>