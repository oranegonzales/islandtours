<%@ Page Title="Tour operations" Language="C#" MasterPageFile="~/Site.Master" %>

<asp:Content ID="HeadStuff" ContentPlaceHolderID="HeadContent" runat="server">
    <meta name="description" content="IslandExplore booking, payment, attraction, and fleet operations." />
</asp:Content>

<asp:Content ID="BodyStuff" ContentPlaceHolderID="MainContent" runat="server">
    <section class="home-hero">
        <div>
            <span class="eyebrow">Island tour operations</span>
            <h1>One place to move every trip forward.</h1>
            <p>IslandExplore keeps client records, attraction availability, bookings, payments, and transportation planning connected from the first enquiry to pickup day.</p>
            <div class="button-row">
                <a class="button button-primary" runat="server" href="~/PublicAttractions.aspx">Browse attractions</a>
                <a class="button button-secondary" runat="server" href="~/ClientLogin.aspx">Manage a booking</a>
            </div>
        </div>
        <aside class="home-note">
            <span class="eyebrow">For the operations team</span>
            <h2>Plan around real constraints</h2>
            <p>The fleet planner evaluates vehicle capacity, route travel time, and schedule conflicts before assignments are written back to a booking.</p>
            <a runat="server" href="~/Login.aspx">Staff sign in</a>
        </aside>
    </section>

    <section class="home-services" aria-labelledby="service-heading">
        <div class="section-heading">
            <div>
                <span class="eyebrow">Connected workflow</span>
                <h2 id="service-heading">From enquiry to excursion</h2>
            </div>
            <p>Each area uses the same booking and client record, reducing repeated entry and hand-off mistakes.</p>
        </div>
        <div class="service-list">
            <article>
                <span>01</span>
                <h3>Client and attraction records</h3>
                <p>Keep contact details, pricing, availability, and destination information in one searchable place.</p>
            </article>
            <article>
                <span>02</span>
                <h3>Bookings and payments</h3>
                <p>Track upcoming tours, invoices, payment history, and controlled refund activity.</p>
            </article>
            <article>
                <span>03</span>
                <h3>Fleet planning</h3>
                <p>Assign transport manually or calculate a deterministic plan for the next 30 days.</p>
            </article>
        </div>
    </section>
</asp:Content>
