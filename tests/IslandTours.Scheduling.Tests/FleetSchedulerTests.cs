using IslandTours.Scheduling;

namespace IslandTours.Scheduling.Tests;

public sealed class FleetSchedulerTests
{
    private static readonly DateTime Day = new(2026, 8, 10);

    [Fact]
    public void Plan_RejectsPartyThatExceedsEveryVehicle()
    {
        FleetScheduler scheduler = CreateScheduler();
        var request = new TourRequest(1, 12, "Kingston", "Ocho Rios", Day.AddHours(8), Day.AddHours(12));
        var vehicle = new FleetVehicle(1, "Blue Route", 8, "Kingston");

        ScheduleResult result = scheduler.Plan(new[] { request }, new[] { vehicle });

        Assert.Empty(result.Assignments);
        UnscheduledTour unscheduled = Assert.Single(result.Unscheduled);
        Assert.Contains("enough seats", unscheduled.Reason);
    }

    [Fact]
    public void Plan_AccountsForRepositioningBetweenTours()
    {
        var graph = new TravelTimeGraph(
            new[]
            {
                new RouteEdge("Kingston", "Ocho Rios", 90),
                new RouteEdge("Ocho Rios", "Montego Bay", 120),
                new RouteEdge("Kingston", "Montego Bay", 180)
            });
        var scheduler = new FleetScheduler(graph);
        var vehicle = new FleetVehicle(1, "North Coast Transit", 8, "Kingston");
        var requests = new[]
        {
            new TourRequest(1, 4, "Kingston", "Ocho Rios", Day.AddHours(8), Day.AddHours(10)),
            new TourRequest(2, 4, "Montego Bay", "Kingston", Day.AddHours(11), Day.AddHours(14))
        };

        ScheduleResult result = scheduler.Plan(requests, new[] { vehicle });

        Assert.Single(result.Assignments);
        Assert.Single(result.Unscheduled);
        Assert.Equal(2, result.Unscheduled[0].Request.BookingId);
    }

    [Fact]
    public void ExactSearch_AvoidsGreedyCapacityTrap()
    {
        var graph = new TravelTimeGraph(new[] { new RouteEdge("A", "B", 100) });
        var requests = new[]
        {
            new TourRequest(1, 2, "A", "A", Day.AddHours(9), Day.AddHours(12)),
            new TourRequest(2, 8, "A", "A", Day.AddHours(10), Day.AddHours(11))
        };
        var vehicles = new[]
        {
            new FleetVehicle(1, "Large Coach", 8, "A"),
            new FleetVehicle(2, "Small Shuttle", 2, "B")
        };

        ScheduleResult result = new FleetScheduler(graph).Plan(requests, vehicles);

        Assert.True(result.UsedExactSearch);
        Assert.Empty(result.Unscheduled);
        Assert.Equal(2, result.Assignments.Count);
        Assert.Equal(2, result.Assignments.Single(item => item.Request.BookingId == 1).Vehicle.VehicleId);
        Assert.Equal(1, result.Assignments.Single(item => item.Request.BookingId == 2).Vehicle.VehicleId);
    }

    [Fact]
    public void LargeWorkload_UsesPredictableHeuristicAndRemainsDeterministic()
    {
        FleetScheduler scheduler = CreateScheduler(new SchedulerOptions { ExactSearchLimit = 3 });
        var vehicles = new[]
        {
            new FleetVehicle(1, "A", 8, "Kingston"),
            new FleetVehicle(2, "B", 12, "Montego Bay")
        };
        TourRequest[] requests = Enumerable.Range(1, 20)
            .Select(index => new TourRequest(
                index,
                2 + (index % 4),
                index % 2 == 0 ? "Kingston" : "Montego Bay",
                "Ocho Rios",
                Day.AddDays(index).AddHours(8),
                Day.AddDays(index).AddHours(12)))
            .ToArray();

        ScheduleResult first = scheduler.Plan(requests, vehicles);
        ScheduleResult second = scheduler.Plan(requests.Reverse(), vehicles.Reverse());

        Assert.False(first.UsedExactSearch);
        Assert.Equal(
            first.Assignments.Select(item => (item.Request.BookingId, item.Vehicle.VehicleId)),
            second.Assignments.Select(item => (item.Request.BookingId, item.Vehicle.VehicleId)));
    }

    private static FleetScheduler CreateScheduler(SchedulerOptions? options = null)
    {
        var graph = new TravelTimeGraph(
            new[]
            {
                new RouteEdge("Kingston", "Ocho Rios", 90),
                new RouteEdge("Ocho Rios", "Montego Bay", 100),
                new RouteEdge("Kingston", "Montego Bay", 180)
            });
        return new FleetScheduler(graph, options);
    }
}
