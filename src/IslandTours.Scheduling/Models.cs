using System;
using System.Collections.Generic;

namespace IslandTours.Scheduling
{
    public sealed class TourRequest
    {
        public TourRequest(int bookingId, int partySize, string origin, string destination, DateTime pickupAt, DateTime dropoffAt)
        {
            if (bookingId <= 0) throw new ArgumentOutOfRangeException(nameof(bookingId));
            if (partySize <= 0) throw new ArgumentOutOfRangeException(nameof(partySize));
            if (string.IsNullOrWhiteSpace(origin)) throw new ArgumentException("An origin is required.", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException("A destination is required.", nameof(destination));
            if (dropoffAt <= pickupAt) throw new ArgumentException("Drop-off must be after pickup.", nameof(dropoffAt));

            BookingId = bookingId;
            PartySize = partySize;
            Origin = origin.Trim();
            Destination = destination.Trim();
            PickupAt = pickupAt;
            DropoffAt = dropoffAt;
        }

        public int BookingId { get; }
        public int PartySize { get; }
        public string Origin { get; }
        public string Destination { get; }
        public DateTime PickupAt { get; }
        public DateTime DropoffAt { get; }
    }

    public sealed class FleetVehicle
    {
        public FleetVehicle(int vehicleId, string providerName, int capacity, string homeBase)
        {
            if (vehicleId <= 0) throw new ArgumentOutOfRangeException(nameof(vehicleId));
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentException("A provider name is required.", nameof(providerName));
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (string.IsNullOrWhiteSpace(homeBase)) throw new ArgumentException("A home base is required.", nameof(homeBase));

            VehicleId = vehicleId;
            ProviderName = providerName.Trim();
            Capacity = capacity;
            HomeBase = homeBase.Trim();
        }

        public int VehicleId { get; }
        public string ProviderName { get; }
        public int Capacity { get; }
        public string HomeBase { get; }
    }

    public sealed class RouteEdge
    {
        public RouteEdge(string from, string to, int minutes)
        {
            if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException("A start location is required.", nameof(from));
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException("An end location is required.", nameof(to));
            if (minutes < 0) throw new ArgumentOutOfRangeException(nameof(minutes));
            From = from.Trim();
            To = to.Trim();
            Minutes = minutes;
        }

        public string From { get; }
        public string To { get; }
        public int Minutes { get; }
    }

    public sealed class VehicleAssignment
    {
        internal VehicleAssignment(TourRequest request, FleetVehicle vehicle, int deadheadMinutes)
        {
            Request = request;
            Vehicle = vehicle;
            DeadheadMinutes = deadheadMinutes;
        }

        public TourRequest Request { get; }
        public FleetVehicle Vehicle { get; }
        public int DeadheadMinutes { get; }
    }

    public sealed class UnscheduledTour
    {
        internal UnscheduledTour(TourRequest request, string reason)
        {
            Request = request;
            Reason = reason;
        }

        public TourRequest Request { get; }
        public string Reason { get; }
    }

    public sealed class ScheduleResult
    {
        internal ScheduleResult(
            IReadOnlyList<VehicleAssignment> assignments,
            IReadOnlyList<UnscheduledTour> unscheduled,
            long score,
            long exploredStates,
            bool usedExactSearch)
        {
            Assignments = assignments;
            Unscheduled = unscheduled;
            Score = score;
            ExploredStates = exploredStates;
            UsedExactSearch = usedExactSearch;
        }

        public IReadOnlyList<VehicleAssignment> Assignments { get; }
        public IReadOnlyList<UnscheduledTour> Unscheduled { get; }
        public long Score { get; }
        public long ExploredStates { get; }
        public bool UsedExactSearch { get; }
    }

    public sealed class SchedulerOptions
    {
        public int ExactSearchLimit { get; set; } = 10;
        public int UnscheduledPenalty { get; set; } = 100000;
        public int EmptySeatPenalty { get; set; } = 2;

        internal void Validate()
        {
            if (ExactSearchLimit < 0 || ExactSearchLimit > 20)
                throw new ArgumentOutOfRangeException(nameof(ExactSearchLimit), "The exact-search limit must be between 0 and 20.");
            if (UnscheduledPenalty <= 0)
                throw new ArgumentOutOfRangeException(nameof(UnscheduledPenalty));
            if (EmptySeatPenalty < 0)
                throw new ArgumentOutOfRangeException(nameof(EmptySeatPenalty));
        }
    }
}
