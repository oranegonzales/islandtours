using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IslandTours.Scheduling
{
    /// <summary>Immutable weighted graph with cached Dijkstra shortest-path lookups.</summary>
    public sealed class TravelTimeGraph
    {
        private readonly Dictionary<string, List<Neighbor>> adjacency;
        private readonly ConcurrentDictionary<string, int> cache =
            new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public TravelTimeGraph(IEnumerable<RouteEdge> edges, bool bidirectional = true)
        {
            if (edges == null) throw new ArgumentNullException(nameof(edges));
            adjacency = new Dictionary<string, List<Neighbor>>(StringComparer.OrdinalIgnoreCase);

            foreach (RouteEdge edge in edges)
            {
                if (edge == null) throw new ArgumentException("Route edges cannot contain null values.", nameof(edges));
                AddEdge(edge.From, edge.To, edge.Minutes);
                if (bidirectional) AddEdge(edge.To, edge.From, edge.Minutes);
            }
        }

        public int GetShortestMinutes(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException("A start location is required.", nameof(from));
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException("An end location is required.", nameof(to));
            if (string.Equals(from.Trim(), to.Trim(), StringComparison.OrdinalIgnoreCase)) return 0;

            string normalizedFrom = from.Trim();
            string normalizedTo = to.Trim();
            string key = normalizedFrom + "\u001f" + normalizedTo;
            return cache.GetOrAdd(key, _ => Dijkstra(normalizedFrom, normalizedTo));
        }

        private void AddEdge(string from, string to, int minutes)
        {
            List<Neighbor> neighbors;
            if (!adjacency.TryGetValue(from, out neighbors))
            {
                neighbors = new List<Neighbor>();
                adjacency.Add(from, neighbors);
            }

            neighbors.Add(new Neighbor(to, minutes));
            if (!adjacency.ContainsKey(to)) adjacency.Add(to, new List<Neighbor>());
        }

        private int Dijkstra(string start, string target)
        {
            if (!adjacency.ContainsKey(start) || !adjacency.ContainsKey(target)) return int.MaxValue;

            var distances = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { [start] = 0 };
            var queue = new MinHeap();
            queue.Push(start, 0);

            while (queue.Count > 0)
            {
                HeapNode current = queue.Pop();
                int known;
                if (!distances.TryGetValue(current.Location, out known) || current.Cost != known) continue;
                if (string.Equals(current.Location, target, StringComparison.OrdinalIgnoreCase)) return current.Cost;

                foreach (Neighbor neighbor in adjacency[current.Location])
                {
                    int candidate = checked(current.Cost + neighbor.Minutes);
                    int old;
                    if (!distances.TryGetValue(neighbor.Location, out old) || candidate < old)
                    {
                        distances[neighbor.Location] = candidate;
                        queue.Push(neighbor.Location, candidate);
                    }
                }
            }

            return int.MaxValue;
        }

        private sealed class Neighbor
        {
            public Neighbor(string location, int minutes) { Location = location; Minutes = minutes; }
            public string Location { get; }
            public int Minutes { get; }
        }

        private sealed class HeapNode
        {
            public HeapNode(string location, int cost) { Location = location; Cost = cost; }
            public string Location { get; }
            public int Cost { get; }
        }

        private sealed class MinHeap
        {
            private readonly List<HeapNode> values = new List<HeapNode>();
            public int Count => values.Count;

            public void Push(string location, int cost)
            {
                values.Add(new HeapNode(location, cost));
                int index = values.Count - 1;
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (values[parent].Cost <= values[index].Cost) break;
                    Swap(parent, index);
                    index = parent;
                }
            }

            public HeapNode Pop()
            {
                if (values.Count == 0) throw new InvalidOperationException("The heap is empty.");
                HeapNode result = values[0];
                int last = values.Count - 1;
                values[0] = values[last];
                values.RemoveAt(last);

                int index = 0;
                while (index < values.Count)
                {
                    int left = (index * 2) + 1;
                    int right = left + 1;
                    if (left >= values.Count) break;
                    int smallest = right < values.Count && values[right].Cost < values[left].Cost ? right : left;
                    if (values[index].Cost <= values[smallest].Cost) break;
                    Swap(index, smallest);
                    index = smallest;
                }

                return result;
            }

            private void Swap(int first, int second)
            {
                HeapNode value = values[first];
                values[first] = values[second];
                values[second] = value;
            }
        }
    }
}
