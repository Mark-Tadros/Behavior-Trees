// Holds all Pathfinding variables and instantiates the AI after the PCG is finished.
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.PF;

public class GridManager : MonoBehaviour
{
    Grid2D Grid;
    public Transform Planes;
    public titleScreen TitleScreen;
    public kingdomManager Kingdom;
    public int worldSize = 50;

    void Awake() { Grid = new Grid2D(new Vector2Int(worldSize, worldSize)); }
    // Gets called when the Grid is updated or changed.
    public void SetWall(int x, int y) { Grid.SetWall(new Vector2Int(x, y)); }
    public void RemoveWall(int x, int y) { Grid.RemoveWall(new Vector2Int(x, y)); }
    public bool CheckWall(int x, int y)
    {
        if (x < 0 || x > 24 || y < 0 || y > 24) return false;
        else return Grid.IsWall(new Vector2Int(x, y));
    }
    public void SetRoad(int x, int y) { Grid.SetRoad(new Vector2Int(x, y)); }
    public bool CheckRoad(int x, int y)
    {
        if (x < 0 || x > 24 || y < 0 || y > 24) return false;
        else return Grid.IsRoad(new Vector2Int(x, y));
    }
    // Calculates and returns the most efficient pathway for the AI.
    public List<Vector2Int> ReturnPath(Vector2Int Start, Vector2Int End)
    {
        List<Vector2Int> Path = Grid.BreadthFirstSearch(Start, End);
        if (Path == null) return null;
        else return Path;
    }
    public void SpawnTown()
    {
        // Spawns Units and initial Castle template.
        Kingdom.CreateBuilding("Castle", new Vector2(12, 12));
        TitleScreen.StartCoroutine(TitleScreen.StartGame());
    }
    // Randomly generates names for each Unit.
    public string GenerateName()
    {
        string name = "";
        int nameLength = Random.Range(2, 5);
        string[] firstComponent = new string[] { "W", "J", "R", "H", "T", "A", "M", "I", "E", "B", "C", "Y", "P" };
        string[] secondComponent = new string[] { "ill", "ohn", "ich", "ob", "en", "al", "om", "alt", "og", "u", "ali", "at", "agn", "ar", "o", "isa", "ea", "ab", "ec", "ul", "el", "es" };
        string[] thirdComponent = new string[] { "ia", "ai", "ar", "er", "ry", "ph", "as", "er", "gh", "ce", "il", "es", "gar", "an", "bella", "ma", "tri", "el", "ilia", "y", "iz", "h" };
        string[] fourthComponent = new string[] { "m", "n", "d", "t", "y", "h", "s", "r", "e", "a", "l" };
        for (int i = 0; i < nameLength; i++)
        {
            if (i == 0) name += firstComponent[Random.Range(0, firstComponent.Length)];
            if (i == 1) name += secondComponent[Random.Range(0, secondComponent.Length)];
            if (i == 2) name += thirdComponent[Random.Range(0, thirdComponent.Length)];
            if (i == 3) name += fourthComponent[Random.Range(0, fourthComponent.Length)];
        }
        return name;
    }
}