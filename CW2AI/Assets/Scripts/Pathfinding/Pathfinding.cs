using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace AlanZucconi.AI.PF
{
    // N is the type of the graph
    // for instance, if this is a grid, N would be the position (Vector2Int)
    public interface IPathfinding<N>
    //    where N : class
    {
        // List of connected nodes from "node"
        IEnumerable<N> Outgoing(N node);
    }

    #region NullableStructs
    // structs cannot be null
    // struct? are still structs
    // C# does not offer a generic type for nullable types
    // To be able to do Map<string> and Map<int?>
    //  we need to convert int? into a new class type Null<int>.
    // Inspired by: https://dev.to/balazsbotond/dealing-with-nothing-in-c---nullable-406k
    public class Null<S> : IEquatable<Null<S>>
    //    where S : struct
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

        #region Equals
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
        #endregion
    }
    /*
    //public interface MapStruct<S> : Map<Null<S>>
    public abstract class MapStruct<S> : IMap<Null<S>>
        where S : struct
    {
        public abstract IEnumerable<S> Outgoing (S node);

        public IEnumerable<Null<S>> Outgoing(Null<S> node)
        {
            foreach (S s in Outgoing(node.Value))
                yield return (Null<S>) s;
        }
    }
    */

    // At this point, we have two notations for classes and structs:
    //  public class GraphString : IMap       <string>
    //  public class GraphInt    :  MapStruct <int>
    // This is ugly.
    // So now we can do:
    //  public class GraphString : MapClass  <string>
    //  public class GraphInt    : MapStruct <int>
    // which is more elegant.
    /*
    public abstract class MapClass<N> : IMap<N>
        where N : class
    {
        public abstract IEnumerable<N> Outgoing(N node);
    }
    */
    #endregion

    /*
    frontier = Queue()
    frontier.put(start )
    visited = {}
    visited[start] = True

    while not frontier.empty():
        current = frontier.get()
        for next in graph.neighbors(current):
            if next not in visited:
                frontier.put(next)
                visited[next] = True
    */
    // https://www.redblobgames.com/pathfinding/a-star/introduction.html
    public static class Pathfinding
    {
        // ASSUMPTION:
        // from two nodes "A" and "B"
        // there can only be ONE edge!
        //
        // The returned path includes the start and goal nodes:
        //  BreadthFirstSearch(a, -) = null         // unreachable goal
        //  BreadthFirstSearch(-, a) = null         // unreachable start
        //  BreadthFirstSearch(a, a) = [a]          // already on the node
        //  BreadthFirstSearch(a, c) = [a, b, c]
        public static List<N> BreadthFirstSearch<N>
        (
            this IPathfinding<N> graph,
            N start, N goal
        )
        {
            // The frontier of active nodes
            Queue<N> frontier = new Queue<N>();
            frontier.Enqueue(start);

            // The list of visited nodes
            HashSet<N> visited = new HashSet<N>();
            visited.Add(start);

            // The vertex we came from
            // (we don't store just the INode,
            //  because there could be multiple IVertex connecting Source and Destination)
            //Dictionary<Node<T>, Edge<T>> visitedFrom = new Dictionary<Node<T>, Edge<T>>();

            // The node we came from
            // Dictionary<to node, from node>
            //Dictionary<N, N> visitedFrom = new Dictionary<N, N>();
            Dictionary<N, Null<N>> visitedFrom = new Dictionary<N, Null<N>>();
            visitedFrom[start] = null;
            //visitedFrom[start] = null;

            // BIG problem!
            // We want a method that works with both classes (IMap<string>)
            // and struct (IMap<Vector2Int>).
            // This is *very* tricky because struct cannot be null.
            // To go around this, we wrap the generic type N
            // ins a reference type "Null" which makes it nullable.
            // This step is redundant if N was already a class,
            // but at least it works with struct as well!

            // --------------------------------------
            // Keeps expanding the frontier
            while (frontier.Count > 0)
            {
                // Loops over all the connected nodes in the frontier...
                N current = frontier.Dequeue();
                // Early termination
                //if (current == goal)
                if (EqualityComparer<N>.Default.Equals(current, goal))
                    break;
                //Debug.Log(current);
                // Loops over the outstar of the current node...
                //foreach (N edge in current)
                //foreach (N next in current)
                foreach (N next in graph.Outgoing(current))
                {
                    //Node<T> next = edge.Destination;

                    //Debug.Log("\t" + next);

                    // Node already visited
                    if (visited.Contains(next))
                        continue;

                    // Register the new node
                    frontier.Enqueue(next);
                    visited.Add(next);
                    visitedFrom[next] = current; // the node we came from
                    //visitedFrom[next] = edge; // vertex.Source is the node we came from
                }
            }

            // Is there a path?
            if (!visitedFrom.ContainsKey(goal))
                return null;

            // --------------------------------------
            // Reconstructs the path in reverse
            List<N> path = new List<N>();
            {
                //N from = visitedFrom[goal];
                //N from = goal;
                Null<N> from = goal;
                do
                {
                    path.Add(from);
                    from = visitedFrom[from];
                    //from = visitedFrom[from];
                    //from = visitedFrom[edge.Source];

                } while (from != null);
            }

            // Reverse the path and returns it
            path.Reverse();
            return path;
        }
        /*
        // When BreathFirstSearch is used on a struct S,
        // it returns a List<Null<S>>, which is rather ugly.
        // This method returns a more natural List<S>.
        public static List<S> BreadthFirstSearch<S>
        (
            //this MapStruct<S> graph,
            this IMapStruct<S> graph,
            S start, S goal
        )
            where S : struct
        {
            List<Null<S>> list = graph.BreadthFirstSearch<Null<S>>(start, goal);
            if (list == null)
                return null;

            return
                list
                .Select(nullable => nullable.Value)
                .ToList();
        }*/
    }
}