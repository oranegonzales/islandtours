# IslandExplore

[![Build](https://github.com/oranegonzales/islandtours/actions/workflows/build.yml/badge.svg)](https://github.com/oranegonzales/islandtours/actions/workflows/build.yml)

IslandExplore is a C# tour-operations system for client records, attraction inventory, bookings, payments, refunds, reporting and fleet assignment. It combines an ASP.NET Web Forms application with an independently tested scheduling engine that solves capacity, route and timing constraints.

## Engineering highlights

| Area | Implementation |
| --- | --- |
| Algorithm design | Dijkstra shortest paths, a custom binary min-heap, branch-and-bound optimization and a deterministic large-batch heuristic |
| Systems thinking | Bounded workloads, cached graph queries, transactional writes, concurrency-aware updates and explicit failure states |
| C# design | A framework-independent `netstandard2.0` domain library consumed by both the legacy web application and .NET 10 verification projects |
| Security | PBKDF2 password hashing and migration, login throttling, encrypted strict cookies, centralized roles, hardened view state and parameterized SQL |
| Quality | xUnit v3 constraint tests, BenchmarkDotNet performance scenarios and Windows CI for both modern .NET and .NET Framework |
| Product work | A responsive, restrained operations interface with distinct client, staff and administrator workflows |

## Fleet planner

The transportation screen can assign one booking manually or plan the next 30 days. The planner:

1. loads a bounded batch of unassigned bookings and active vehicles;
2. calculates shortest travel and repositioning times over a weighted parish graph;
3. rejects unreachable, over-capacity or overlapping assignments;
4. uses exact branch-and-bound search for small inputs;
5. switches to a predictable heuristic for operational-scale batches; and
6. applies results in one transaction without overwriting a concurrent manual assignment.

The same input always produces the same plan. Infeasible bookings remain unassigned with a reason.

Read [the algorithm notes](docs/ALGORITHM.md) and [architecture guide](docs/ARCHITECTURE.md) for complexity, constraints and trade-offs.

## Repository map

```text
.
├── src/IslandTours.Scheduling/              # graph and scheduling domain
├── tests/IslandTours.Scheduling.Tests/      # behavioral and algorithm tests
├── benchmarks/IslandTours.Scheduling.Benchmarks/
├── docs/                                    # architecture, algorithm and security notes
└── invenman/
    ├── invenman.sln
    └── invenman/
        ├── Security/                        # authentication and authorization
        ├── Services/                        # application orchestration
        ├── App_Data/TravelTime.sql          # repeatable schema and indexes
        └── *.aspx                           # Web Forms workflows
```

## Local setup

### Requirements

- Windows 10 or later
- Visual Studio 2022 with **ASP.NET and web development**
- .NET Framework 4.7.2 developer pack
- .NET 10 SDK
- SQL Server Express or SQL Server

### Start the application

1. Clone the repository and open `invenman/invenman.sln`.
2. Run `invenman/invenman/App_Data/TravelTime.sql` in SQL Server Management Studio. The migration is repeatable and creates the scheduling columns, fleet table and supporting indexes.
3. Create the first administrator from PowerShell:

   ```powershell
   .\invenman\invenman\tools\New-TravelTimeAdmin.ps1 -Username admin
   ```

   The command prompts for a password of at least 12 characters and stores only its PBKDF2 hash.

4. Confirm both `TravelTime` connection strings in `invenman/invenman/Web.config`. The checked-in default targets local `SQLEXPRESS` with Windows authentication.
5. Set `invenman` as the startup project and run it with IIS Express.

### Optional exchange rates

Set `TRAVELTIME_FIXER_API_KEY` in the IIS Express or application-pool environment, or copy `AppSettings.local.config.example` to `AppSettings.local.config` and add your own key. The local file is ignored by Git.

The credential visible in an old repository revision must be revoked at the provider. Deleting a value from the current file does not remove it from Git history.

## Tests and benchmarks

Run the algorithm suite on any platform with .NET 10:

```bash
dotnet test tests/IslandTours.Scheduling.Tests/IslandTours.Scheduling.Tests.csproj -c Release
```

Run reproducible 50, 250 and 1,000-booking measurements:

```bash
dotnet run -c Release --project benchmarks/IslandTours.Scheduling.Benchmarks
```

GitHub Actions also restores and compiles the .NET Framework web application on Windows.

## Roles

- **Administrator** — users, roles, audit history and application configuration.
- **Staff** — clients, attractions, bookings, transportation, payments and reporting.
- **Client** — their linked bookings, invoices, receipts and transportation details.

## Security and deployment

The repository contains practical security controls, not a guarantee of production safety. Review [the security notes](docs/SECURITY.md) before deployment. Production requires HTTPS-only cookies, managed secrets, least-privilege database credentials, a shared protected machine key, distributed session/rate-limit state and independent security testing.

## Author

[Orane Gonzales](https://github.com/oranegonzales)
