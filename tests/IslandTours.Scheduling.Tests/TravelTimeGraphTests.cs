using IslandTours.Scheduling;

namespace IslandTours.Scheduling.Tests;

public sealed class TravelTimeGraphTests
{
    [Fact]
    public void ShortestPath_UsesCheaperIndirectRoute()
    {
        var graph = new TravelTimeGraph(
            new[]
            {
                new RouteEdge("Kingston", "Ocho Rios", 120),
                new RouteEdge("Kingston", "Spanish Town", 30),
                new RouteEdge("Spanish Town", "Ocho Rios", 60)
            });

        int minutes = graph.GetShortestMinutes("Kingston", "Ocho Rios");

        Assert.Equal(90, minutes);
    }

    [Fact]
    public void ShortestPath_ReturnsMaxValueForDisconnectedLocations()
    {
        var graph = new TravelTimeGraph(new[] { new RouteEdge("Kingston", "Portmore", 25) });

        Assert.Equal(int.MaxValue, graph.GetShortestMinutes("Kingston", "Montego Bay"));
    }
}
