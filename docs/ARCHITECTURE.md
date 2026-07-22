# Architecture

IslandExplore is an incremental modernization of an ASP.NET Web Forms system. The operational application remains on .NET Framework 4.7.2, while new domain logic lives in an independently testable .NET Standard library and its verification tooling runs on .NET 10.

## Boundaries

| Area | Responsibility |
| --- | --- |
| `invenman/invenman` | Web Forms UI, role-aware workflows, SQL Server persistence and application configuration |
| `src/IslandTours.Scheduling` | Framework-independent graph and fleet-scheduling algorithms |
| `tests/IslandTours.Scheduling.Tests` | Behavioral, constraint and determinism tests |
| `benchmarks/IslandTours.Scheduling.Benchmarks` | Reproducible throughput and allocation measurements |
| `invenman/invenman/Services` | Application services that translate database records to domain inputs and apply results transactionally |
| `invenman/invenman/Security` | Authentication, password hashing, cookie lifecycle and authorization policy |

The Web Forms project references `IslandTours.Scheduling` through its `netstandard2.0` contract. Tests and benchmarks consume that same assembly from .NET 10, so the algorithm is not coupled to `System.Web`, SQL Server or UI controls.

## Transportation planning flow

1. `TransportationPlanningService` loads at most 500 unassigned bookings in a bounded 30-day window and active fleet vehicles.
2. Parish links are represented as a weighted graph.
3. Dijkstra shortest-path lookups calculate repositioning time and are cached for repeated location pairs.
4. `FleetScheduler` validates capacity, route reachability and non-overlapping vehicle windows.
5. Small workloads use branch-and-bound search; larger workloads use a deterministic cost-ordered heuristic.
6. Assignments are applied inside a SQL transaction with a conditional update, so a concurrent manual assignment is not overwritten.
7. The page reports applied, unscheduled and concurrently changed counts and writes an audit event.

## Scale and failure behavior

- Planning queries are bounded and supported by a covering booking index.
- Shortest paths are cached in a thread-safe dictionary.
- Exact search is capped to prevent exponential work on large batches.
- The fallback heuristic is deterministic and approximately `O(B × V × log L)`, where `B` is bookings, `V` is vehicles and `L` is graph work for an uncached location pair.
- SQL writes reuse one prepared command and one transaction per batch.
- Bookings that cannot be routed or carried remain unassigned with an explicit reason; the planner does not invent a route.

For a multi-instance deployment, use a shared machine key, distributed session state, centralized rate limiting and managed secret storage. Those deployment concerns are intentionally not hard-coded into the repository.
