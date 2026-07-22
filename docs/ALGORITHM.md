# Fleet scheduling algorithm

## Objective

The planner minimizes a non-negative score:

`deadhead travel minutes + empty seats × seat penalty + unscheduled tours × unscheduled penalty`

The high unscheduled penalty makes serving a feasible tour preferable to a small routing improvement. All ties are broken by vehicle ID, producing repeatable plans from the same inputs.

## Constraints

An assignment is feasible only when:

- the vehicle has enough seats;
- the pickup and destination are connected in the route graph;
- the vehicle can reach the pickup from its home base or previous destination; and
- its previous drop-off plus repositioning time is no later than the next pickup.

Requests are ordered by pickup time, descending party size and booking ID. That ordering makes every accepted assignment append-only within a vehicle schedule and avoids repeated interval-tree mutation.

## Two execution modes

For at most ten requests, branch-and-bound explores feasible vehicle choices plus an unassigned choice. A greedy plan supplies the initial upper bound. Any partial branch whose score cannot improve that bound is pruned.

Above ten requests, the planner selects the lowest incremental-cost feasible vehicle. This bounds runtime for operational batches and avoids an exponential surprise. The threshold is configurable and covered by tests.

## Shortest paths

`TravelTimeGraph` uses Dijkstra's algorithm with a custom binary min-heap:

- graph construction: `O(E)`;
- uncached route lookup: `O((V + E) log V)`;
- cached lookup: expected `O(1)`.

The graph is immutable after construction and route results are stored in a `ConcurrentDictionary`, allowing one scheduler definition to serve concurrent requests safely.

## Verification

The test suite covers indirect shortest paths, disconnected routes, capacity rejection, repositioning conflicts, deterministic large-batch behavior and a capacity trap where exact search must improve on the greedy solution. BenchmarkDotNet scenarios exercise 50, 250 and 1,000 booking batches with memory diagnostics.
