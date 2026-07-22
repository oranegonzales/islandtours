using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using IslandTours.Scheduling;

namespace invenman.Services
{
    public sealed class TransportationPlanningService
    {
        private const int MaximumPlanningBatch = 500;
        private readonly string connectionString;
        private readonly TravelTimeGraph travelTimes;

        public TransportationPlanningService()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["TravelTime"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("Connection string 'TravelTime' is not configured.");

            connectionString = settings.ConnectionString;
            travelTimes = BuildJamaicaTravelGraph();
        }

        public TransportationPlanningSummary PlanUpcoming(DateTime today, int planningDays)
        {
            if (planningDays < 1 || planningDays > 90)
                throw new ArgumentOutOfRangeException("planningDays", "The planning window must be between 1 and 90 days.");

            DateTime windowStart = today.Date;
            DateTime windowEnd = windowStart.AddDays(planningDays + 1);
            List<TourRequest> requests;
            List<FleetVehicle> vehicles;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                requests = LoadRequests(connection, windowStart, windowEnd);
                vehicles = LoadVehicles(connection);
            }

            if (requests.Count == 0)
                return TransportationPlanningSummary.Empty("No unassigned bookings were found in this planning window.");
            if (vehicles.Count == 0)
                return TransportationPlanningSummary.Empty("No active vehicles are configured.");

            var scheduler = new FleetScheduler(
                travelTimes,
                new SchedulerOptions
                {
                    ExactSearchLimit = 10,
                    EmptySeatPenalty = 2,
                    UnscheduledPenalty = 100000
                });
            ScheduleResult plan = scheduler.Plan(requests, vehicles);
            int applied = ApplyAssignments(plan.Assignments);

            return new TransportationPlanningSummary(
                requests.Count,
                applied,
                plan.Assignments.Count - applied,
                plan.Unscheduled,
                plan.UsedExactSearch,
                plan.ExploredStates);
        }

        private List<TourRequest> LoadRequests(SqlConnection connection, DateTime start, DateTime end)
        {
            const string sql = @"
SELECT TOP (@MaximumPlanningBatch)
       b.BookingID,
       b.PartySize,
       COALESCE(NULLIF(b.PickupParish, N''), NULLIF(b.PickupLocation, N''), N'Kingston') AS PickupParish,
       a.Parish AS DestinationParish,
       b.TourDate,
       a.DurationMinutes
FROM dbo.Bookings AS b
INNER JOIN dbo.Attractions AS a ON a.AttractionID = b.AttractionID
WHERE b.TourDate >= @WindowStart
  AND b.TourDate < @WindowEnd
  AND b.BookingStatus = N'Active'
  AND NULLIF(LTRIM(RTRIM(b.TransportProvider)), N'') IS NULL
ORDER BY b.TourDate, b.BookingID;";

            var requests = new List<TourRequest>();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@MaximumPlanningBatch", SqlDbType.Int).Value = MaximumPlanningBatch;
                command.Parameters.Add("@WindowStart", SqlDbType.Date).Value = start;
                command.Parameters.Add("@WindowEnd", SqlDbType.Date).Value = end;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int bookingId = reader.GetInt32(0);
                        int partySize = reader.GetInt32(1);
                        string origin = NormalizeParish(reader.GetString(2));
                        string destination = NormalizeParish(reader.GetString(3));
                        DateTime tourDate = reader.GetDateTime(4).Date;
                        int tourDuration = reader.GetInt32(5);
                        DateTime pickupAt = tourDate.AddHours(8);
                        int drivingMinutes = travelTimes.GetShortestMinutes(origin, destination);
                        int occupiedMinutes = drivingMinutes == int.MaxValue ? tourDuration : drivingMinutes + tourDuration;

                        requests.Add(new TourRequest(
                            bookingId,
                            partySize,
                            origin,
                            destination,
                            pickupAt,
                            pickupAt.AddMinutes(Math.Max(30, occupiedMinutes))));
                    }
                }
            }

            return requests;
        }

        private static List<FleetVehicle> LoadVehicles(SqlConnection connection)
        {
            const string sql = @"
SELECT VehicleID, ProviderName, VehicleCode, Capacity, HomeParish
FROM dbo.Vehicles
WHERE IsActive = 1
ORDER BY VehicleID;";

            var vehicles = new List<FleetVehicle>();
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int vehicleId = reader.GetInt32(0);
                    string displayName = reader.GetString(1) + " / " + reader.GetString(2);
                    vehicles.Add(new FleetVehicle(
                        vehicleId,
                        displayName,
                        reader.GetInt32(3),
                        NormalizeParish(reader.GetString(4))));
                }
            }

            return vehicles;
        }

        private int ApplyAssignments(IReadOnlyList<VehicleAssignment> assignments)
        {
            if (assignments.Count == 0) return 0;

            const string sql = @"
UPDATE dbo.Bookings
SET TransportProvider = @TransportProvider,
    PickupParish = @PickupParish,
    PickupLocation = COALESCE(NULLIF(PickupLocation, N''), @PickupParish),
    PickupDateTime = @PickupDateTime,
    TransportNotes = CASE
        WHEN TransportNotes IS NULL OR LTRIM(RTRIM(TransportNotes)) = N'' THEN @PlanningNote
        ELSE TransportNotes + CHAR(13) + CHAR(10) + @PlanningNote
    END,
    BookingStatus = N'Transport assigned'
WHERE BookingID = @BookingID
  AND BookingStatus = N'Active'
  AND NULLIF(LTRIM(RTRIM(TransportProvider)), N'') IS NULL;";

            int applied = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                {
                    command.Parameters.Add("@TransportProvider", SqlDbType.NVarChar, 150);
                    command.Parameters.Add("@PickupParish", SqlDbType.NVarChar, 100);
                    command.Parameters.Add("@PickupDateTime", SqlDbType.DateTime2);
                    command.Parameters.Add("@PlanningNote", SqlDbType.NVarChar, 500);
                    command.Parameters.Add("@BookingID", SqlDbType.Int);

                    foreach (VehicleAssignment assignment in assignments)
                    {
                        command.Parameters["@TransportProvider"].Value = assignment.Vehicle.ProviderName;
                        command.Parameters["@PickupParish"].Value = assignment.Request.Origin;
                        command.Parameters["@PickupDateTime"].Value = assignment.Request.PickupAt;
                        command.Parameters["@PlanningNote"].Value =
                            "Fleet planner assignment; deadhead estimate " +
                            assignment.DeadheadMinutes.ToString() +
                            " minutes.";
                        command.Parameters["@BookingID"].Value = assignment.Request.BookingId;
                        applied += command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            return applied;
        }

        private static TravelTimeGraph BuildJamaicaTravelGraph()
        {
            return new TravelTimeGraph(
                new[]
                {
                    new RouteEdge("Kingston", "St. Andrew", 20),
                    new RouteEdge("Kingston", "St. Catherine", 35),
                    new RouteEdge("Kingston", "St. Thomas", 50),
                    new RouteEdge("St. Andrew", "St. Mary", 70),
                    new RouteEdge("St. Thomas", "Portland", 80),
                    new RouteEdge("Portland", "St. Mary", 60),
                    new RouteEdge("St. Mary", "St. Ann", 55),
                    new RouteEdge("St. Catherine", "St. Ann", 75),
                    new RouteEdge("St. Ann", "Trelawny", 60),
                    new RouteEdge("Trelawny", "St. James", 45),
                    new RouteEdge("St. James", "Hanover", 45),
                    new RouteEdge("Hanover", "Westmoreland", 35),
                    new RouteEdge("St. Catherine", "Clarendon", 55),
                    new RouteEdge("Clarendon", "Manchester", 50),
                    new RouteEdge("Manchester", "St. Elizabeth", 60),
                    new RouteEdge("St. Elizabeth", "Westmoreland", 70)
                });
        }

        private static string NormalizeParish(string value)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (normalized.Length == 0) return "Kingston";
            string key = normalized.ToLowerInvariant().Replace("saint ", "st. ").Replace("st ", "st. ");
            if (key.Contains("montego bay")) return "St. James";
            if (key.Contains("ocho rios")) return "St. Ann";
            if (key.Contains("negril")) return "Westmoreland";
            if (key.Contains("port antonio")) return "Portland";
            if (key.Contains("spanish town")) return "St. Catherine";
            if (key.Contains("mandeville")) return "Manchester";
            var known = new[]
            {
                "Kingston", "St. Andrew", "St. Catherine", "St. Thomas", "Portland", "St. Mary",
                "St. Ann", "Trelawny", "St. James", "Hanover", "Westmoreland", "Clarendon",
                "Manchester", "St. Elizabeth"
            };

            string match = known.FirstOrDefault(item => string.Equals(item, key, StringComparison.OrdinalIgnoreCase));
            return match ?? normalized;
        }
    }

    public sealed class TransportationPlanningSummary
    {
        public TransportationPlanningSummary(
            int considered,
            int assigned,
            int writeConflicts,
            IReadOnlyList<UnscheduledTour> unscheduled,
            bool usedExactSearch,
            long exploredStates)
        {
            Considered = considered;
            Assigned = assigned;
            WriteConflicts = writeConflicts;
            Unscheduled = unscheduled;
            UsedExactSearch = usedExactSearch;
            ExploredStates = exploredStates;
            Message = string.Empty;
        }

        private TransportationPlanningSummary(string message)
        {
            Message = message;
            Unscheduled = new List<UnscheduledTour>();
        }

        public int Considered { get; private set; }
        public int Assigned { get; private set; }
        public int WriteConflicts { get; private set; }
        public IReadOnlyList<UnscheduledTour> Unscheduled { get; private set; }
        public bool UsedExactSearch { get; private set; }
        public long ExploredStates { get; private set; }
        public string Message { get; private set; }

        public static TransportationPlanningSummary Empty(string message)
        {
            return new TransportationPlanningSummary(message);
        }
    }
}
