<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminSystemConfig.aspx.cs" Inherits="invenman.AdminSystemConfig" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-page">
        <div class="tt-card">
            <h2 class="tt-title">System configuration</h2>
            <p class="tt-subtitle">These settings are read from and saved to Web.config appSettings. The Fixer API key is read-only here.</p>

            <asp:Label ID="lblMsg" runat="server" CssClass="tt-msg" />

            <div class="tt-grid">
                <div class="tt-section">
                    <h4 class="tt-section-title">Environment</h4>

                    <div class="tt-row">
                        <div class="tt-label">Active database</div>
                        <div class="tt-value">
                            <asp:Label ID="lblDbInfo" runat="server" />
                        </div>
                    </div>

                    <div class="tt-row">
                        <div class="tt-label">Connection string name</div>
                        <div class="tt-value">
                            <asp:Label ID="lblConnName" runat="server" Text="TravelTimeDb" />
                        </div>
                    </div>
                </div>

                <div class="tt-section">
                    <h4 class="tt-section-title">Site behavior</h4>

                    <div class="tt-form-row">
                        <div class="tt-form-left">
                            <asp:CheckBox ID="chkMaintenanceMode" runat="server" />
                        </div>
                        <div class="tt-form-right">
                            <div class="tt-form-label">Maintenance mode</div>
                            <div class="tt-help">When enabled, only Admin users should continue working. Others get redirected.</div>
                        </div>
                    </div>

                    <div class="tt-form-row">
                        <div class="tt-form-left">
                            <asp:CheckBox ID="chkAdRotatorEnabled" runat="server" />
                        </div>
                        <div class="tt-form-right">
                            <div class="tt-form-label">AdRotator enabled</div>
                            <div class="tt-help">Show or hide the Adspane content.</div>
                        </div>
                    </div>

                    <div class="tt-form-row">
                        <div class="tt-form-left">
                            <asp:CheckBox ID="chkShowUsd" runat="server" />
                        </div>
                        <div class="tt-form-right">
                            <div class="tt-form-label">Show USD conversion</div>
                            <div class="tt-help">If enabled, pages that support it will show an approximate USD value.</div>
                        </div>
                    </div>

                    <div class="tt-form-row">
                        <div class="tt-form-left tt-input-wrap">
                            <asp:DropDownList ID="ddlRounding" runat="server" CssClass="form-select tt-input">
                                <asp:ListItem Value="0">0 decimals</asp:ListItem>
                                <asp:ListItem Value="2">2 decimals</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="tt-form-right">
                            <div class="tt-form-label">USD rounding</div>
                            <div class="tt-help">Controls how many decimals the USD value displays.</div>
                        </div>
                    </div>
                </div>

                <div class="tt-section">
                    <h4 class="tt-section-title">Business details</h4>

                    <div class="mb-3">
                        <label class="tt-form-label" for="txtCompanyName">Company name</label>
                        <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control tt-input" />
                    </div>

                    <div class="mb-3">
                        <label class="tt-form-label" for="txtSupportEmail">Support email</label>
                        <asp:TextBox ID="txtSupportEmail" runat="server" CssClass="form-control tt-input" />
                    </div>

                    <div class="mb-3">
                        <label class="tt-form-label" for="txtSupportPhone">Support phone</label>
                        <asp:TextBox ID="txtSupportPhone" runat="server" CssClass="form-control tt-input" />
                    </div>

                    <div class="mb-3">
                        <label class="tt-form-label" for="txtCashInstructions">Cash payment instructions</label>
                        <asp:TextBox ID="txtCashInstructions" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control tt-input tt-textarea" />
                    </div>

                    <div class="mb-3">
                        <label class="tt-form-label" for="txtBankInstructions">Bank transfer instructions</label>
                        <asp:TextBox ID="txtBankInstructions" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control tt-input tt-textarea" />
                    </div>
                </div>

                <div class="tt-section">
                    <h4 class="tt-section-title">Transport providers</h4>

                    <div class="mb-2">
                        <div class="tt-help">One per line. Used by the Assign transportation page dropdown.</div>
                    </div>

                    <asp:TextBox ID="txtTransportProviders" runat="server" TextMode="MultiLine" Rows="7" CssClass="form-control tt-input tt-textarea" />

                    <div class="tt-help mt-2">Example: JUTA Taxi, Knutsford Express, Island Routes, Private Driver</div>
                </div>

                <div class="tt-section">
                    <h4 class="tt-section-title">External API</h4>

                    <div class="tt-row">
                        <div class="tt-label">Fixer API key</div>
                        <div class="tt-value">
                            <asp:TextBox ID="txtFixerKey" runat="server" CssClass="form-control tt-input" ReadOnly="true" />
                        </div>
                    </div>

                    <div class="tt-help mt-2">To change the key, update Web.config appSettings FixerApiKey manually.</div>
                </div>
            </div>

            <div class="tt-actions">
                <asp:Button ID="btnSave" runat="server" Text="Save settings" CssClass="tt-btn tt-btn-inline" OnClick="btnSave_Click" />
                <asp:Button ID="btnReload" runat="server" Text="Reload" CssClass="tt-btn tt-btn-inline" OnClick="btnReload_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <style type="text/css">
        .tt-page { max-width: 820px; margin: 0 auto; }
        .tt-card { border: 1px solid rgba(255,255,255,0.10); background: rgba(17, 24, 39, 0.85); border-radius: 16px; padding: 18px; }
        .tt-title { margin: 0 0 6px 0; font-weight: 900; font-size: 28px; }
        .tt-subtitle { margin: 0 0 14px 0; color: #cbd5e1; }
        .tt-msg { display: block; margin: 10px 0 12px 0; color: #93c5fd; }
        .tt-grid { display: grid; grid-template-columns: 1fr; gap: 14px; }
        .tt-section { border: 1px solid rgba(255,255,255,0.10); background: rgba(2, 6, 23, 0.35); border-radius: 14px; padding: 14px; }
        .tt-section-title { margin: 0 0 10px 0; font-weight: 900; font-size: 16px; color: #e5e7eb; }
        .tt-row { display: grid; grid-template-columns: 180px 1fr; gap: 10px; align-items: center; margin-bottom: 10px; }
        .tt-label { color: #cbd5e1; font-size: 13px; font-weight: 800; }
        .tt-value { color: #e5e7eb; font-size: 14px; }
        .tt-help { color: #94a3b8; font-size: 12.5px; }
        .tt-input { background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.14); color: #e5e7eb; border-radius: 12px; }
        .tt-input:focus { background: rgba(255,255,255,0.08); border-color: rgba(255,183,3,0.65); box-shadow: 0 0 0 3px rgba(255,183,3,0.18); color: #e5e7eb; }
        .tt-textarea { resize: vertical; }
        .tt-actions { display: flex; gap: 10px; margin-top: 14px; flex-wrap: wrap; }
        .tt-btn-inline { width: auto; min-width: 160px; }
        .tt-form-row { display: grid; grid-template-columns: 24px 1fr; gap: 12px; align-items: start; margin-bottom: 10px; }
        .tt-form-left { display: flex; align-items: center; justify-content: center; }
        .tt-form-right { display: flex; flex-direction: column; gap: 2px; }
        .tt-form-label { font-weight: 900; color: #e5e7eb; }
        .tt-input-wrap { justify-content: flex-start; }
        .tt-form-row input[type="checkbox"] { width: 18px; height: 18px; margin: 0; accent-color: #ffb703; }
    </style>
</asp:Content>
