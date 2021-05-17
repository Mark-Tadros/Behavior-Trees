// Creates the Pathfinding Grid to be accessed and controlled.
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Grid2D : IPathfinding<Vector2Int>
    {
        // The directions in which you can move in a 2D grid.
        private static Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        // The grid data.
        private bool[,] Wall;
        private bool[,] Road;

        public Grid2D (Vector2Int size)
        {
            Wall = new bool[size.x, size.y];
            Road = new bool[size.x, size.y];
        }
        // Sets or Removes all the bools.
        public void SetWall (Vector2Int position)
        {
            Wall[position.x, position.y] = true;
        }
        public void SetRoad (Vector2Int position)
        {
            Road[position.x, position.y] = true;
        }
        public void RemoveWall(Vector2Int position)
        {
            Wall[position.x, position.y] = false;
        }
        public bool IsWall(Vector2Int position)
        {
            return Wall[position.x, position.y];
        }
        public bool IsRoad(Vector2Int position)
        {
            return Road[position.x, position.y];
        }

        // Considered wall if out of bounds.
        private bool IsFree (Vector2Int position)
        {
            // Out of bounds: it is a wall.
            if (position.x < 0 || position.x >= Wall.GetLength(0))
                return false;
            if (position.y < 0 || position.y >= Wall.GetLength(1))
                return false;

            return ! Wall[position.x, position.y];
        }

        public IEnumerable<Vector2Int> Outgoing (Vector2Int position)
        {
            // Out of bounds.
            if (position.x < 0 || position.x >= Wall.GetLength(0))
                yield break;
            if (position.y < 0 || position.y >= Wall.GetLength(1))
                yield break;

            foreach (Vector2Int direction in Directions)
                if (IsFree(position + direction))
                    yield return position + direction;
        }
    }
}