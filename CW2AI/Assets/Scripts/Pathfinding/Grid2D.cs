// Stores the Grid data to quickly verify the pathfinding path.
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.AI.PF
{
    public class Grid2D : IPathfinding<Vector2Int>
    {
        // The directions in which you can move in a 2D grid
        private static Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        // Holds the x,y values for each tile to verify if theres an obstacle or not.
        private bool[,] Wall;
        // Initialises each Wall to become accesible.
        public Grid2D (Vector2Int size)
        {
            Wall = new bool[size.x, size.y];
        }
        // Gets called externally to change or check each tile value.
        public void SetWall (Vector2Int position) { Wall[position.x, position.y] = true; }
        public void RemoveWall(Vector2Int position) { Wall[position.x, position.y] = false; }
        public bool IsWall(Vector2Int position) { return Wall[position.x, position.y]; }

        // Gets called by the pathfinding to check if out of bounds.
        private bool IsFree (Vector2Int position)
        {
            if (position.x < 0 || position.x >= Wall.GetLength(0)) return false;
            if (position.y < 0 || position.y >= Wall.GetLength(1)) return false;
            return ! Wall[position.x, position.y];
        }
        public IEnumerable<Vector2Int> Outgoing (Vector2Int position)
        {
            // Out of bounds.
            if (position.x < 0 || position.x >= Wall.GetLength(0))
                yield break;
            if (position.y < 0 || position.y >= Wall.GetLength(1))
                yield break;
            // Returns a verifiable path.
            foreach (Vector2Int direction in Directions)
            {
                if (IsFree(position + direction)) yield return position + direction;
            }
        }
    }
}