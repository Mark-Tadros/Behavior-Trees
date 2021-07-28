// Controls the actual pathfinding by accessing the Grid2D.cs script to calculate a pathway.
// Adapted from Alan Zucconis 2D Pathfinding Tutorial - more information at https://www.marktadros.co.uk/astar
using System;
using System.Collections.Generic;

namespace AlanZucconi.AI.PF
{
    public interface IPathfinding<N>
    {
        // List of connected nodes from the outgoing values in Grid2D.cs.
        IEnumerable<N> Outgoing(N node);
    }
    public class Null<S> : IEquatable<Null<S>>
    {
        public S Value;
        public static implicit operator S (Null<S> s) { return s.Value; }
        public static implicit operator Null<S> (S s) { return new Null<S> { Value = s }; }
        public override bool Equals(object obj)
        {
            if (obj is Null<S>)
                return Equals((Null<S>)obj);
            return false;
        }
        public bool Equals(Null<S> nullable) { return EqualityComparer<S>.Default.Equals(Value, nullable.Value); }
        public override int GetHashCode() { return Value.GetHashCode(); }
    }    
    public static class Pathfinding
    {
        public static List<N> BreadthFirstSearch<N>( this IPathfinding<N> graph, N start, N goal )
        {
            // The frontier of active nodes
            Queue<N> frontier = new Queue<N>();
            frontier.Enqueue(start);

            // The list of visited nodes
            HashSet<N> visited = new HashSet<N>();
            visited.Add(start);

            // The previous node we came from.
            Dictionary<N, Null<N>> visitedFrom = new Dictionary<N, Null<N>>();
            visitedFrom[start] = null;

            // Keeps expanding the path.
            while (frontier.Count > 0)
            {
                // Loops over all the connected nodes.
                N current = frontier.Dequeue();
                // Early termination if we reach the goal.
                if (EqualityComparer<N>.Default.Equals(current, goal))
                    break;
                // Loops over the outstar of the current node.
                foreach (N next in graph.Outgoing(current))
                {
                    // Node already visited.
                    if (visited.Contains(next))
                        continue;

                    // Register the new node.
                    frontier.Enqueue(next);
                    visited.Add(next);
                    visitedFrom[next] = current;
                }
            }

            // Checks if there is a path otherwise return null.
            if (!visitedFrom.ContainsKey(goal)) return null;

            // Reconstructs the path in reverse so we can follow it.
            List<N> path = new List<N>();
            {
                Null<N> from = goal;
                do
                {
                    path.Add(from);
                    from = visitedFrom[from];
                }
                while (from != null);
            }

            // Reverse the path and returns its x,y positions so we can follow it in the AIManager.cs script.
            path.Reverse();
            return path;
        }
    }
}