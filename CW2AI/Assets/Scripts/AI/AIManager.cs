// Controls the Pathfinding and Movement for each individual Unit.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    GridManager Grid;
    public GameObject Target;
    public bool isMoving;
    public bool isStuck;
    [HideInInspector] public float movementSpeed;

    List<Vector2Int> Path;
    Vector3 oldPosition;

    public List<Sprite> emotePrefabs;

    void Start()
    {
        Grid = GameObject.Find("pathfindingManager").GetComponent<GridManager>();
        oldPosition = transform.position;
    }
    // If the Target position is different then the Unit position start moving automatically.
    void Update() { if (Target.transform.position != oldPosition) FindPath(); }
    // Finds a Path from the Unit to the Target.
    void FindPath()
    {
        StopCoroutine("FollowPath");
        // Remove the Wall from where the Unit is currently standing to calculate.
        Path = new List<Vector2Int>();
        Path = Grid.ReturnPath(
            new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)),
            new Vector2Int(Mathf.RoundToInt(Target.transform.position.x), Mathf.RoundToInt(Target.transform.position.z))
            );

        if (Path == null) { Debug.Log("No path available"); isStuck = true; }
        else
        {
            isMoving = true;
            // Adds a Wall where the Unit will end up to prevent overlapping.
            Grid.RemoveWall(Mathf.RoundToInt(oldPosition.x), Mathf.RoundToInt(oldPosition.z));
            oldPosition = Target.transform.position;
            Grid.SetWall(Mathf.RoundToInt(Target.transform.position.x), Mathf.RoundToInt(Target.transform.position.z));
            StartCoroutine("FollowPath");
        }
    }
    // Continously Follow that Path until it reaches its destination.
    IEnumerator FollowPath()
    {
        Debug.Log("Following Path");
        // Removes the current position to prevent backtracking.
        Path.Remove(Path[0]);
        while (Path.Count > 0)
        {           
            // Moves the Unit to the closest path, before removing it and moving on.
            float maxDistance = movementSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(Path[0].x, 0.5f, Path[0].y), maxDistance);
            // Upon reaching the destination, subtract one from Paths and repeat.
            if (Vector3.Distance(transform.position, new Vector3(Path[0].x, 0.5f, Path[0].y)) < 0.001f)
            {
                Path.Remove(Path[0]);
            }
            yield return null;
        }
        isMoving = false;
    }
}