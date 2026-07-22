using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IslandTours.Scheduling
{
    /// <summary>
    /// Builds deterministic transportation plans. Small inputs use branch-and-bound exact search;
    /// larger inputs use a cost-ordered greedy heuristic to keep runtime predictable.
    /// </summary>
    public sealed class FleetScheduler
    {
        private readonly TravelTimeGraph travelTimes;
        private readonly SchedulerOptions options;

        public FleetScheduler(TravelTimeGraph travelTimes, SchedulerOptions? options = null)
        {
            this.travelTimes = travelTimes ?? throw new ArgumentNullException(nameof(travelTimes));
            this.options = options ?? new SchedulerOptions();
            this.options.Validate();
        }

        public ScheduleResult Plan(
            IEnumerable<TourRequest> requests,
            IEnumerable<FleetVehicle> vehicles,
            CancellationToken cancellationToken = default)
        {
            if (requests == null) throw new ArgumentNullException(nameof(requests));
            if (vehicles == null) throw new ArgumentNullException(nameof(vehicles));

            List<TourRequest> orderedRequests = requests
                .OrderBy(request => request.PickupAt)
                .ThenByDescending(request => request.PartySize)
                .ThenBy(request => request.BookingId)
                .ToList();
            List<FleetVehicle> orderedVehicles = vehicles
                .OrderBy(vehicle => vehicle.VehicleId)
                .ToList();

            ValidateUniqueIds(orderedRequests, orderedVehicles);
            PlanState greedy = BuildGreedy(orderedRequests, orderedVehicles, cancellationToken);

            if (orderedRequests.Count > options.ExactSearchLimit)
            {
                return BuildResult(orderedRequests, orderedVehicles, greedy, orderedRequests.Count, false);
            }

            var search = new ExactSearch(
                orderedRequests,
                orderedVehicles,
                travelTimes,
                options,
                greedy,
                cancellationToken);
            PlanState exact = search.Run();
            return BuildResult(orderedRequests, orderedVehicles, exact, search.ExploredStates, true);
        }

        private PlanState BuildGreedy(
            IReadOnlyList<TourRequest> requests,
            IReadOnlyList<FleetVehicle> vehicles,
            CancellationToken cancellationToken)
        {
            var lastTours = new TourRequest?[vehicles.Count];
            var state = new PlanState(requests.Count);

            for (int requestIndex = 0; requestIndex < requests.Count; requestIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TourRequest request = requests[requestIndex];
                List<Candidate> candidates = GetCandidates(request, vehicles, lastTours);

                if (candidates.Count == 0)
                {
                    state.VehicleIndexes[requestIndex] = -1;
                    state.Score += options.UnscheduledPenalty;
                    continue;
                }

                Candidate chosen = candidates[0];
                state.VehicleIndexes[requestIndex] = chosen.VehicleIndex;
                state.DeadheadMinutes[requestIndex] = chosen.DeadheadMinutes;
                state.Score += chosen.Cost;
                lastTours[chosen.VehicleIndex] = request;
            }

            return state;
        }

        private List<Candidate> GetCandidates(
            TourRequest request,
            IReadOnlyList<FleetVehicle> vehicles,
            IReadOnlyList<TourRequest?> lastTours)
        {
            var candidates = new List<Candidate>();
            if (travelTimes.GetShortestMinutes(request.Origin, request.Destination) == int.MaxValue)
                return candidates;

            for (int vehicleIndex = 0; vehicleIndex < vehicles.Count; vehicleIndex++)
            {
                FleetVehicle vehicle = vehicles[vehicleIndex];
                if (vehicle.Capacity < request.PartySize) continue;

                TourRequest? previous = lastTours[vehicleIndex];
                string startingPoint = previous == null ? vehicle.HomeBase : previous.Destination;
                int deadhead = travelTimes.GetShortestMinutes(startingPoint, request.Origin);
                if (deadhead == int.MaxValue) continue;

                if (previous != null && previous.DropoffAt.AddMinutes(deadhead) > request.PickupAt) continue;

                long cost = deadhead + ((long)(vehicle.Capacity - request.PartySize) * options.EmptySeatPenalty);
                candidates.Add(new Candidate(vehicleIndex, deadhead, cost, vehicle.VehicleId));
            }

            candidates.Sort(CandidateComparer.Instance);
            return candidates;
        }

        private ScheduleResult BuildResult(
            IReadOnlyList<TourRequest> requests,
            IReadOnlyList<FleetVehicle> vehicles,
            PlanState state,
            long exploredStates,
            bool usedExactSearch)
        {
            var assignments = new List<VehicleAssignment>();
            var unscheduled = new List<UnscheduledTour>();

            for (int index = 0; index < requests.Count; index++)
            {
                int vehicleIndex = state.VehicleIndexes[index];
                if (vehicleIndex >= 0)
                {
                    assignments.Add(new VehicleAssignment(
                        requests[index],
                        vehicles[vehicleIndex],
                        state.DeadheadMinutes[index]));
                }
                else
                {
                    unscheduled.Add(new UnscheduledTour(
                        requests[index],
                        GetUnscheduledReason(requests[index], vehicles)));
                }
            }

            return new ScheduleResult(assignments, unscheduled, state.Score, exploredStates, usedExactSearch);
        }

        private string GetUnscheduledReason(TourRequest request, IReadOnlyList<FleetVehicle> vehicles)
        {
            if (!vehicles.Any(vehicle => vehicle.Capacity >= request.PartySize))
                return "No active vehicle has enough seats for this party.";
            if (travelTimes.GetShortestMinutes(request.Origin, request.Destination) == int.MaxValue)
                return "No configured route connects the pickup and destination.";
            if (!vehicles.Any(vehicle => travelTimes.GetShortestMinutes(vehicle.HomeBase, request.Origin) != int.MaxValue))
                return "No configured route reaches the pickup location.";
            return "Every suitable vehicle has a conflicting assignment or repositioning window.";
        }

        private static void ValidateUniqueIds(
            IReadOnlyList<TourRequest> requests,
            IReadOnlyList<FleetVehicle> vehicles)
        {
            if (requests.Any(request => request == null))
                throw new ArgumentException("Requests cannot contain null values.", nameof(requests));
            if (vehicles.Any(vehicle => vehicle == null))
                throw new ArgumentException("Vehicles cannot contain null values.", nameof(vehicles));

            if (requests.Select(request => request.BookingId).Distinct().Count() != requests.Count)
                throw new ArgumentException("Booking IDs must be unique.", nameof(requests));
            if (vehicles.Select(vehicle => vehicle.VehicleId).Distinct().Count() != vehicles.Count)
                throw new ArgumentException("Vehicle IDs must be unique.", nameof(vehicles));
        }

        private sealed class ExactSearch
        {
            private readonly IReadOnlyList<TourRequest> requests;
            private readonly IReadOnlyList<FleetVehicle> vehicles;
            private readonly TravelTimeGraph travelTimes;
            private readonly SchedulerOptions options;
            private readonly CancellationToken cancellationToken;
            private readonly TourRequest?[] lastTours;
            private readonly PlanState current;
            private PlanState best;

            public ExactSearch(
                IReadOnlyList<TourRequest> requests,
                IReadOnlyList<FleetVehicle> vehicles,
                TravelTimeGraph travelTimes,
                SchedulerOptions options,
                PlanState initialBest,
                CancellationToken cancellationToken)
            {
                this.requests = requests;
                this.vehicles = vehicles;
                this.travelTimes = travelTimes;
                this.options = options;
                this.cancellationToken = cancellationToken;
                lastTours = new TourRequest?[vehicles.Count];
                current = new PlanState(requests.Count);
                best = initialBest.Clone();
            }

            public long ExploredStates { get; private set; }

            public PlanState Run()
            {
                Search(0);
                return best;
            }

            private void Search(int requestIndex)
            {
                ExploredStates++;
                if ((ExploredStates & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
                if (current.Score >= best.Score) return;

                if (requestIndex == requests.Count)
                {
                    best = current.Clone();
                    return;
                }

                TourRequest request = requests[requestIndex];
                List<Candidate> candidates = GetCandidates(request);

                foreach (Candidate candidate in candidates)
                {
                    TourRequest? previous = lastTours[candidate.VehicleIndex];
                    lastTours[candidate.VehicleIndex] = request;
                    current.VehicleIndexes[requestIndex] = candidate.VehicleIndex;
                    current.DeadheadMinutes[requestIndex] = candidate.DeadheadMinutes;
                    current.Score += candidate.Cost;

                    Search(requestIndex + 1);

                    current.Score -= candidate.Cost;
                    current.VehicleIndexes[requestIndex] = -1;
                    current.DeadheadMinutes[requestIndex] = 0;
                    lastTours[candidate.VehicleIndex] = previous;
                }

                current.VehicleIndexes[requestIndex] = -1;
                current.Score += options.UnscheduledPenalty;
                Search(requestIndex + 1);
                current.Score -= options.UnscheduledPenalty;
            }

            private List<Candidate> GetCandidates(TourRequest request)
            {
                var candidates = new List<Candidate>();
                if (travelTimes.GetShortestMinutes(request.Origin, request.Destination) == int.MaxValue)
                    return candidates;

                for (int vehicleIndex = 0; vehicleIndex < vehicles.Count; vehicleIndex++)
                {
                    FleetVehicle vehicle = vehicles[vehicleIndex];
                    if (vehicle.Capacity < request.PartySize) continue;

                    TourRequest? previous = lastTours[vehicleIndex];
                    string startingPoint = previous == null ? vehicle.HomeBase : previous.Destination;
                    int deadhead = travelTimes.GetShortestMinutes(startingPoint, request.Origin);
                    if (deadhead == int.MaxValue) continue;
                    if (previous != null && previous.DropoffAt.AddMinutes(deadhead) > request.PickupAt) continue;

                    long cost = deadhead + ((long)(vehicle.Capacity - request.PartySize) * options.EmptySeatPenalty);
                    candidates.Add(new Candidate(vehicleIndex, deadhead, cost, vehicle.VehicleId));
                }

                candidates.Sort(CandidateComparer.Instance);
                return candidates;
            }
        }

        private sealed class PlanState
        {
            public PlanState(int requestCount)
            {
                VehicleIndexes = Enumerable.Repeat(-1, requestCount).ToArray();
                DeadheadMinutes = new int[requestCount];
            }

            public int[] VehicleIndexes { get; }
            public int[] DeadheadMinutes { get; }
            public long Score { get; set; }

            public PlanState Clone()
            {
                var clone = new PlanState(VehicleIndexes.Length) { Score = Score };
                Array.Copy(VehicleIndexes, clone.VehicleIndexes, VehicleIndexes.Length);
                Array.Copy(DeadheadMinutes, clone.DeadheadMinutes, DeadheadMinutes.Length);
                return clone;
            }
        }

        private sealed class Candidate
        {
            public Candidate(int vehicleIndex, int deadheadMinutes, long cost, int vehicleId)
            {
                VehicleIndex = vehicleIndex;
                DeadheadMinutes = deadheadMinutes;
                Cost = cost;
                VehicleId = vehicleId;
            }

            public int VehicleIndex { get; }
            public int DeadheadMinutes { get; }
            public long Cost { get; }
            public int VehicleId { get; }
        }

        private sealed class CandidateComparer : IComparer<Candidate>
        {
            public static readonly CandidateComparer Instance = new CandidateComparer();

            public int Compare(Candidate? first, Candidate? second)
            {
                if (ReferenceEquals(first, second)) return 0;
                if (first == null) return -1;
                if (second == null) return 1;
                int byCost = first.Cost.CompareTo(second.Cost);
                return byCost != 0 ? byCost : first.VehicleId.CompareTo(second.VehicleId);
            }
        }
    }
}
