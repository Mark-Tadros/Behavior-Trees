// Controls and find the shortest path from the Grid2D script.
using System;
using System.Collections.Generic;

namespace Pathfinding
{
    // List of connected nodes from "node".
    public interface IPathfinding<N>
    {
        IEnumerable<N> Outgoing(N node);
    }

    // Inspired by: https://dev.to/balazsbotond/dealing-with-nothing-in-c---nullable-406k.
    public class Null<S> : IEquatable<Null<S>>
    {
        public S Value;
        public static implicit operator S (Null<S> s)
        {
            return s.Value;
        }
        public static implicit operator Null<S> (S s)
        {
            return new Null<S> { Value = s };
        }

        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type
        public override bool Equals(object obj)
        {
            if (obj is Null<S>)
                return Equals((Null<S>)obj);
            return false;
        }

        public bool Equals(Null<S> nullable)
        {
            return EqualityComparer<S>.Default.Equals(Value, nullable.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public static class Pathfinding
    {
        public static List<N> BreadthFirstSearch<N>
        (
            this IPathfinding<N> graph,
            N start, N goal
        )
        {
            // The frontier of active nodes.
            Queue<N> frontier = new Queue<N>();
            frontier.Enqueue(start);

            // The list of visited nodes.
            HashSet<N> visited = new HashSet<N>();
            visited.Add(start);

            Dictionary<N, Null<N>> visitedFrom = new Dictionary<N, Null<N>>();
            visitedFrom[start] = null;

            while (frontier.Count > 0)
            {
                // Loops over all the connected nodes in the frontier.
                N current = frontier.Dequeue();
                if (EqualityComparer<N>.Default.Equals(current, goal)) break;
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

            // Is there a path?
            if (!visitedFrom.ContainsKey(goal))
                return null;

            // Reconstructs the path in reverse.
            List<N> path = new List<N>();
            {
                Null<N> from = goal;
                do
                {
                    path.Add(from);
                    from = visitedFrom[from];

                } while (from != null);
            }

            // Reverse the path and returns it.
            path.Reverse();
            return path;
        }
    }
}