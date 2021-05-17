// Allows for Procedural Generation for the Environment.
using System.Collections.Generic;
using UnityEngine;

public class PCG : MonoBehaviour
{
    GridManager Grid;
    public cameraStop cameraScript;
    // Stores resource Prefabs.
    public List<Transform> grassPrefab;
    public Transform bushPrefab;
    public List<Transform> orePrefab;
    public List<Transform> treePrefab;
    public List<Transform> otherPrefab;
    public Transform waterPrefab;

    void Start()
    {
        Grid = GameObject.Find("pathfindingManager").GetComponent<GridManager>();
        CreateGrid();
    }
    // Generates the Grids visuals based on randomly generated values.
    void CreateGrid()
    {
        // Creates the initial layer.
        for (int y = 0; y < Grid.worldSize; y++)
        {
            for (int x = 0; x < Grid.worldSize; x++)
            {
                Transform Plane = Instantiate(grassPrefab[Random.Range(0, 2)]) as Transform;
                Plane.position = new Vector3(x, 0, y);
                Plane.parent = this.transform.GetChild(0);
                Plane.name = x + "," + y;
            }
        }
        // Creates the external objects. Excludes area at the edges.
        for (int y = 1; y < Grid.worldSize - 1; y++)
        {
            for (int x = 1; x < Grid.worldSize - 1; x++)
            {
                if ((x > (Grid.worldSize / 2) - 3 && x < (Grid.worldSize / 2) + 3) && (y > (Grid.worldSize / 2) - 3 && y < (Grid.worldSize / 2) + 3))
                { /* Excludes area in the centre */ }
                else { 
                    int randomNumber = Random.Range(0, 100);
                    // Creates Bushes.
                    if (randomNumber >= 0 && randomNumber < 8 && !Grid.CheckWall(x, y))
                    {
                        // Creates the Prefab.
                        Transform Plane = Instantiate(bushPrefab) as Transform;
                        // Updates its position, parent, and name.
                        Plane.position = new Vector3(x, 0.25f, y);
                        Plane.parent = this.transform.GetChild(1);
                        Plane.name = x + "," + y;
                        // Updates the pathfinding Grid to calculate it as an obstacle.
                        Grid.SetWall(x, y);
                        Grid.Kingdom.Resources.Add(Plane.GetComponent<ResourceManager>());
                        Plane.GetComponent<ResourceManager>().Grid = Grid;
                        Plane.GetComponent<ResourceManager>().Position = new Vector3(x, 0, y - 1);
                        // Add a random chance for it to be empty for visual effect.
                        if (Random.value > 0.5f)
                            Plane.GetComponent<ResourceManager>().StartCoroutine(Plane.GetComponent<ResourceManager>().Gathered());
                    }
                    // Creates Ores.
                    else if(randomNumber >= 10 && randomNumber < 13 && !Grid.CheckWall(x, y))
                    {
                        int random = Random.Range(0, orePrefab.Count);
                        Transform Plane = Instantiate(orePrefab[random]) as Transform;
                        Plane.position = new Vector3(x, 0.25f, y);
                        Plane.parent = this.transform.GetChild(1);
                        Plane.name = x + "," + y;
                        Grid.SetWall(x, y);
                        Grid.Kingdom.Resources.Add(Plane.GetComponent<ResourceManager>());
                        Plane.GetComponent<ResourceManager>().Grid = Grid;
                        Plane.GetComponent<ResourceManager>().Position = new Vector3(x, 0, y - 1);
                        if (Random.value > 0.5f)
                            Plane.GetComponent<ResourceManager>().StartCoroutine(Plane.GetComponent<ResourceManager>().Gathered());
                    }
                    // Creates Trees.
                    else if(randomNumber >= 20 && randomNumber < 24 && !Grid.CheckWall(x, y))
                    {
                        int random = Random.Range(0, treePrefab.Count);
                        Transform Plane = Instantiate(treePrefab[random]) as Transform;
                        Plane.position = new Vector3(x, 0.25f, y);
                        Plane.parent = this.transform.GetChild(1);
                        Plane.name = x + "," + y;
                        Grid.SetWall(x, y);
                        Grid.Kingdom.Resources.Add(Plane.GetComponent<ResourceManager>());
                        Plane.GetComponent<ResourceManager>().Grid = Grid;
                        Plane.GetComponent<ResourceManager>().Position = new Vector3(x, 0, y - 1);
                        if (Random.value > 0.5f)
                            Plane.GetComponent<ResourceManager>().StartCoroutine(Plane.GetComponent<ResourceManager>().Gathered());
                    }
                    // Creates Others.
                    else if (randomNumber >= 30 && randomNumber < 35 && !Grid.CheckWall(x, y))
                    {
                        int random = Random.Range(0, otherPrefab.Count);
                        Transform Plane = Instantiate(otherPrefab[random]) as Transform;
                        Plane.position = new Vector3(x, 0.25f, y);
                        Plane.parent = this.transform.GetChild(1);
                        Plane.name = x + "," + y;
                        Grid.SetWall(x, y);
                    }
                    // Creates Water
                    else if (randomNumber >= 40 && randomNumber < 41)
                    {
                        for (int y2 = y + 1; y2 < y + Random.Range(3, 5); y2++)
                        {
                            for (int x2 = x + 1; x2 < x + Random.Range(3, 7); x2++)
                            {
                                if (x2 < 1 || x2 > Grid.worldSize - 2 || y2 < 1 || y2 > Grid.worldSize - 2 ||
                                    ((x2 > (Grid.worldSize / 2) - 6 && x2 < (Grid.worldSize / 2) + 6) && (y2 > (Grid.worldSize / 2) - 6 && y2 < (Grid.worldSize / 2) + 6)))
                                { /* Excludes outside of the Grid and inside */ }
                                else if (!Grid.CheckWall(x2, y2))
                                {
                                    Transform Plane = Instantiate(waterPrefab) as Transform;
                                    Plane.position = new Vector3(x2, 0, y2);
                                    Plane.parent = this.transform.GetChild(1);
                                    Plane.name = x2 + "," + y2;
                                    Grid.SetWall(x2, y2);
                                }
                            }
                        }
                    }
                }
            }
        }
        // Reveals the Scene after the PCG has generated.
        //transform.position = Vector3.zero;
        Grid.SpawnTown();
    }
}