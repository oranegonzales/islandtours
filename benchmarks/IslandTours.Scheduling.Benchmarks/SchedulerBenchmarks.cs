using BenchmarkDotNet.Attributes;
using IslandTours.Scheduling;

[MemoryDiagnoser]
public class SchedulerBenchmarks
{
    private FleetScheduler scheduler = null!;
    private TourRequest[] requests = null!;
    private FleetVehicle[] vehicles = null!;

    [Params(50, 250, 1000)]
    public int BookingCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var graph = new TravelTimeGraph(
            new[]
            {
                new RouteEdge("Kingston", "Spanish Town", 30),
                new RouteEdge("Spanish Town", "Ocho Rios", 70),
                new RouteEdge("Ocho Rios", "Montego Bay", 100),
                new RouteEdge("Montego Bay", "Negril", 90),
                new RouteEdge("Kingston", "Mandeville", 100),
                new RouteEdge("Mandeville", "Negril", 150)
            });
        scheduler = new FleetScheduler(graph, new SchedulerOptions { ExactSearchLimit = 8 });
        vehicles = Enumerable.Range(1, 40)
            .Select(index => new FleetVehicle(
                index,
                "Fleet " + index,
                8 + ((index % 4) * 4),
                index % 2 == 0 ? "Kingston" : "Montego Bay"))
            .ToArray();
        DateTime start = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Unspecified);
        requests = Enumerable.Range(1, BookingCount)
            .Select(index =>
            {
                DateTime pickup = start.AddMinutes(index * 45);
                return new TourRequest(
                    index,
                    1 + (index % 8),
                    index % 2 == 0 ? "Kingston" : "Montego Bay",
                    index % 3 == 0 ? "Negril" : "Ocho Rios",
                    pickup,
                    pickup.AddHours(3));
            })
            .ToArray();
    }

    [Benchmark]
    public ScheduleResult BuildSchedule() => scheduler.Plan(requests, vehicles);
}
