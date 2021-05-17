// Contains the Kingdom HiveMind AI and gives out Schedules to each Unit depending on needs.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class kingdomManager : MonoBehaviour
{
    [HideInInspector] public GridManager Grid;
    public GameObject GameOverObjectTitle;
    public GameObject GameOverObjectSubTitle;
    // Stores the Kingdoms Resources.
    public float Science;
    RectTransform ScienceFill; 
    public float Public;
    RectTransform PublicFill;
    public float Military;
    RectTransform MilitaryFill;
    public float Gold;
    RectTransform GoldFill;
    public float roamRadius; public float unitCost;
    // Stores all the Prefabs
    public Transform unitPrefab;
    public List<Transform> buildingPrefabs;
    public Transform roadPrefab;
    public List<Sprite> roadPrefabs;
    public List<Sprite> unitModelPrefabs;
    // Stores all created Buildings and Units.
    public List<ResourceManager> Resources;
    public List<unitManager> Units;
    public List<unitManager> Enemies;
    public List<Transform> Buildings;
    public List<Transform> Roads;
    public Transform ResearchBuilding;
    public List<Transform> researchUnits;
    int isResearching;

    private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    void Awake()
    {
        Grid = GameObject.Find("pathfindingManager").GetComponent<GridManager>();
        isResearching = 0;
        StartCoroutine(CoroutineCoordinator());
    }
    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0) yield return StartCoroutine(coroutineQueue.Dequeue());
            yield return null;
        }
    }
    // Calls the incremental checks to control the HiveMind AI.
    public void LateStart()
    {
        InvokeRepeating("FindCaptain", 15f, 10f);
        InvokeRepeating("FindWorkers", 2.5f, 2.5f);
        InvokeRepeating("FindSoldiers", 5f, 5f);
        StartCoroutine(CreateEnemies());
    }
    public void UpdateResources()
    {
        // Updates the physical visuals once subtracted or added to.
        ScienceFill.localScale = new Vector3(1, Science / 100, 1);
        if (Science < 0) Science = 0; if (Science > 100) Science = 100;
        PublicFill.localScale = new Vector3(1, Public / 100, 1);
        if (Public < 0) Public = 0; if (Public > 100) Public = 100;
        MilitaryFill.localScale = new Vector3(1, Military / 100, 1);
        if (Military < 0) Military = 0; if (Military > 100) Military = 100;
        GoldFill.localScale = new Vector3(1, Gold / 100, 1);
        if (Gold < 0) Gold = 0; if (Gold > 100) Gold = 100;
    }
    // Invoked to update Resource values.
    void SubtractGold()
    {
        Gold -= 0.25f;
        Military += 0.25f;
        Public -= 0.1f;
        Science -= 0.1f;
        UpdateResources();
    }
    void SubtractScience()
    {
        Science -= 1f;
        UpdateResources();
    }
    // Create a new Unit.
    public void CreateUnit(string Role) { coroutineQueue.Enqueue(createUnit(Role)); }
    IEnumerator createUnit(string Role)
    {
        yield return new WaitUntil(() => !Grid.CheckWall(12, 11));
        int whatUnit = 0; float movementSpeed = 0;
        // Determines what Unit it is.
        if (Role == "Captain") { whatUnit = 0; movementSpeed = 0.35f; roamRadius = 2.25f; unitCost = 15; }
        else if (Role == "Worker") { whatUnit = 1; movementSpeed = 0.5f; roamRadius += 0.5f; unitCost += 2.5f; }
        else if (Role == "Soldier") { whatUnit = 4; movementSpeed = 0.5f; roamRadius += 0.75f; InvokeRepeating("SubtractGold", 0f, 5f); unitCost += 5; }
        else if (Role == "Scout") { whatUnit = 3; movementSpeed = 0.75f; roamRadius += 1.0f; InvokeRepeating("SubtractGold", 0f, 7.5f); unitCost += 2.5f; }
        UpdateResources();
        if (roamRadius >= 7) roamRadius = 7;
        if (unitCost > 60) unitCost = 60;
        // Creates the Prefab.
        Transform Unit = Instantiate(unitPrefab) as Transform;
        // Updates its position, parent, and name.
        Unit.position = new Vector3(12, 0, 11);
        Unit.parent = GameObject.Find("Units").transform;
        // Updates the pathfinding Grid to calculate it as an obstacle.
        Grid.SetWall(12, 11);
        Units.Add(Unit.GetComponent<unitManager>());
        // Updates the Units roles and sets its Sprite.
        Unit.GetChild(0).GetComponent<SpriteRenderer>().sprite = unitModelPrefabs[whatUnit];
        Unit.GetComponent<unitManager>().Model.GetComponent<AIManager>().movementSpeed = movementSpeed;
        Unit.GetComponent<unitManager>().Kingdom = this;
        Unit.GetComponent<unitManager>().Name = Grid.GenerateName();
        Unit.GetComponent<unitManager>().Role = Role;
        Unit.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = Role + "\n" + Unit.GetComponent<unitManager>().Name;
        Unit.name = Role + " " + Unit.GetComponent<unitManager>().Name;
        if (Role == "Captain") { Unit.GetComponent<unitManager>().Health = 7; }
        else if (Role == "Worker") { Unit.GetComponent<unitManager>().Schedule.Add("Create House"); Unit.GetComponent<unitManager>().Health = 3; }
        else if (Role == "Scout") { Unit.GetComponent<unitManager>().Schedule.Add("Scout"); Unit.GetComponent<unitManager>().Health = 5; }
        else if (Role == "Soldier" && Random.value > 0.75f) { Unit.GetComponent<unitManager>().Schedule.Add("Create Tower"); Unit.GetComponent<unitManager>().Health = 7; }
        // Reveals the Unit slowly, quality of life feature.
        LeanTween.value(Unit.gameObject, SetAlpha, 0, 1f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);
        FinishedAlpha();
    }
    void SetAlpha(float value)
    {
        Units[Units.Count - 1].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        Units[Units.Count - 1].transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, value);
        if (value < 0.75f) Units[Units.Count - 1].transform.GetChild(0).GetChild(1).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, value);
        else Units[Units.Count - 1].transform.GetChild(0).GetChild(1).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 0.75f);
    }
    void FinishedAlpha()
    {
        Units[Units.Count - 1].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Units[Units.Count - 1].transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 1f);
        Units[Units.Count - 1].transform.GetChild(0).GetChild(1).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 0.75f);
    }
    // Creates a new Building.
    public void CreateBuilding(string whatBuilding, Vector2 whatPosition)
    {
        int WhatBuilding = 0;
        // Determines what Building it is.
        if (whatBuilding == "Castle") WhatBuilding = 0;
        else if (whatBuilding == "Tower") WhatBuilding = 1;
        else if (whatBuilding == "House") { if (Random.value > 0.7f) WhatBuilding = 2; else WhatBuilding = 3; }
        else if (whatBuilding == "Research") WhatBuilding = 13;
        else if (whatBuilding == "Farm") WhatBuilding = 14;
        else if (whatBuilding == "Random") WhatBuilding = Random.Range(5, 13);
        // Creates the Prefab and updates its values.
        Transform Building = Instantiate(buildingPrefabs[WhatBuilding]) as Transform;
        Building.position = new Vector3(whatPosition.x, 0, whatPosition.y);
        Building.parent = GameObject.Find("Buildings").transform;
        Building.name = whatPosition.x + "," + whatPosition.y + " " + whatBuilding;
        Grid.SetWall(Mathf.RoundToInt(whatPosition.x), Mathf.RoundToInt(whatPosition.y));
        Buildings.Add(Building);
        // If its a Castle, set the initial variables to call them later.
        if (whatBuilding == "Castle")
        {
            ScienceFill = Building.GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>();
            PublicFill = Building.GetChild(1).GetChild(1).GetChild(0).GetComponent<RectTransform>();
            MilitaryFill = Building.GetChild(1).GetChild(2).GetChild(0).GetComponent<RectTransform>();
            GoldFill = Building.GetChild(1).GetChild(3).GetChild(0).GetComponent<RectTransform>();
        }
        else if (whatBuilding == "Research") { ResearchBuilding = Building; InvokeRepeating("SubtractScience", 0f, 5f); }
        else if (whatBuilding == "Farm")
        {
            Resources.Add(Building.GetChild(0).GetComponent<ResourceManager>());
            Building.GetChild(0).GetComponent<ResourceManager>().Grid = Grid;
            Building.GetChild(0).GetComponent<ResourceManager>().Position = new Vector3(whatPosition.x, 0, whatPosition.y - 1);
            Building.GetChild(0).GetComponent<ResourceManager>().StartCoroutine(Building.GetChild(0).GetComponent<ResourceManager>().Gathered());
        }
        // Creates a Road underneath all these buildings.
        if (WhatBuilding == 0 || WhatBuilding == 1 || WhatBuilding == 2  || WhatBuilding == 8 || WhatBuilding == 9  || WhatBuilding == 11 || WhatBuilding == 13)
            StartCoroutine(CreateRoads(whatPosition));
    }
    IEnumerator CreateRoads(Vector2 whatPosition)
    {        
        List<Transform> CreatedRoads = new List<Transform>();
        // Adds the original building position to the CreatedRoads list.
        CreatedRoads.Add(CreateRoad(Mathf.RoundToInt(whatPosition.x), Mathf.RoundToInt(whatPosition.y), true, false));

        // Bug check to verify Castles.
        if (whatPosition.x == 12 && whatPosition.y == 12) CreatedRoads.Add(CreateRoad(12, 11, false, true));
        else
        {
            List<Vector2Int> Path = new List<Vector2Int>();
            Vector2 Position = new Vector3(12, 11);
            // Calculcates the nearest current road to attach itself to.
            float minDistance = Mathf.Infinity;
            foreach (Transform Road in Roads)
            {
                float Distance = Vector3.Distance(new Vector3(Road.position.x, 0, Road.position.z), new Vector3(whatPosition.x, 0, whatPosition.y - 1));
                if (Distance < minDistance && !Grid.CheckWall(Mathf.RoundToInt(Road.position.x), Mathf.RoundToInt(Road.position.z)))
                {
                    Position = new Vector2(Road.position.x, Road.position.z);
                    minDistance = Distance;
                }
            }
            if (Position.x == 12 && Position.y == 11) yield return new WaitUntil(() => !Grid.CheckWall(12, 11));
            Grid.RemoveWall(Mathf.RoundToInt(whatPosition.x), Mathf.RoundToInt(whatPosition.y - 1));
            // Creates a new path from the Buildings to the nearest Road.
            while (Path == null || Path.Count == 0)
            {
                Path = Grid.ReturnPath(
                    new Vector2Int(Mathf.RoundToInt(Position.x), Mathf.RoundToInt(Position.y)),
                    new Vector2Int(Mathf.RoundToInt(whatPosition.x), Mathf.RoundToInt(whatPosition.y - 1))
                    );
                yield return null;
            }
            // Sets a valid Road and then calls CheckRoads to change the visuals.
            while (Path.Count > 0)
            {
                // Creates Road Object, before changing its visuals at CheckRoads.
                if (Path[0].x == whatPosition.x && Path[0].y == whatPosition.y - 1) CreatedRoads.Add(CreateRoad(Path[0].x, Path[0].y, false, true));
                else CreatedRoads.Add(CreateRoad(Path[0].x, Path[0].y, false, false));                
                Path.Remove(Path[0]);
                yield return null;
            }
        }
        // Updates visuals for all the Road transforms after it creates them.
        for (int i = 0; i < Roads.Count; i++)
        {
            if (Roads[i] != null) UpdateRoad(Roads[i]);
        }
        yield return new WaitForSeconds(2.5f);
        for (int i = 0; i < CreatedRoads.Count; i++)
        {
            if (CreatedRoads[i] != null) CreatedRoads[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }
    Transform CreateRoad(int x, int y, bool Building, bool BuildingEntrance)
    {
        // Creates Roads on that specific position after checking if there is already a road there.
        if (!Grid.CheckRoad(x, y))
        {
            Transform Road = Instantiate(roadPrefab) as Transform;
            Road.position = new Vector3(x, 0, y);
            Road.parent = GameObject.Find("Roads").transform;
            Road.name = x + "," + y;
            if (Building) Road.GetComponent<RoadManager>().Building = true;
            else Grid.SetRoad(x, y);
            Road.GetComponent<RoadManager>().BuildingEntrance = BuildingEntrance;
            Roads.Add(Road);
            return Road;
        }
        // Theres already a road here so locate it in the Roads list.
        else
        {
            for (int i = 0; i < Roads.Count; i++)
            {
                if (Roads[i].name == x + "," + y)
                {
                    if (Building) return null;
                    if (BuildingEntrance) Roads[i].GetComponent<RoadManager>().BuildingEntrance = true;
                    return Roads[i];
                }
            }
            return null;
        }
    }
    // Loops through every road value and update its visuals based on the other roads around it.
    void UpdateRoad(Transform Road)
    {
        int x = Mathf.RoundToInt(Road.position.x); int y = Mathf.RoundToInt(Road.position.z);
        // If the road is on top of a Building.
        if (Road.GetComponent<RoadManager>().Building) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[1];
        // If the road has no other roads near it assume theres one directly above it for BuildingEntrances.
        else if (!Grid.CheckRoad(x, y + 1) && !Grid.CheckRoad(x, y - 1) && !Grid.CheckRoad(x + 1, y) && !Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[0];
        }
        // If there is a road to the left and to the right.
        else if (!Grid.CheckRoad(x, y + 1) && !Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[11];
            else Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[5];
        }
        // If there is a road to the left, right, and directly above.
        else if (Grid.CheckRoad(x, y + 1) && !Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[11];
        }
        // If there is a road above and below.
        else if (Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && !Grid.CheckRoad(x + 1, y) && !Grid.CheckRoad(x - 1, y))
        {
            Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[4];
        }
        // If the road has no road above it but one directly below assume its both ways for BuildingEntrances.
        else if (!Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && !Grid.CheckRoad(x + 1, y) && !Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[4];
        }
        // If the road has one above it, to the right and below.
        else if (Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && !Grid.CheckRoad(x - 1, y))
        {
            Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[12];
        }
        // If the road has one above it, to the left and below.
        else if (Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && !Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[13];
        }
        // If the road goes to the right and below.
        else if (!Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && !Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[12];
            else Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[10];
        }
        // If the road goes to the left and below.
        else if (!Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && !Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[13];
            else Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[7];
        }
        // If the road has one above it and to the right.
        else if (Grid.CheckRoad(x, y + 1) && !Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && !Grid.CheckRoad(x - 1, y))
        {
            Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[9];
        }
        // If the road has one above it and to the left.
        else if (Grid.CheckRoad(x, y + 1) && !Grid.CheckRoad(x, y - 1) && !Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[8];
        }
        // If the road has no road above it but one to the right assume its both ways for BuildingEntrances.
        else if (!Grid.CheckRoad(x, y + 1) && !Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && !Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[9];
        }
        // If the road has no road above it but one to the left assume its both ways for BuildingEntrances.
        else if (!Grid.CheckRoad(x, y + 1) && !Grid.CheckRoad(x, y - 1) && !Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[8];
        }
        // If there is a road in every direction apart from the top.
        else if (!Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            if (Road.GetComponent<RoadManager>().BuildingEntrance) Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[6];
            else Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[14];
        }
        // If there is a road on every direction.
        else if (Grid.CheckRoad(x, y + 1) && Grid.CheckRoad(x, y - 1) && Grid.CheckRoad(x + 1, y) && Grid.CheckRoad(x - 1, y))
        {
            Road.GetComponent<SpriteRenderer>().sprite = roadPrefabs[6];
        }

        // Slowly fades in the various Sprites.
        LeanTween.alpha(Road.gameObject, 1, 2.5f).setEase(LeanTweenType.easeInOutQuad);
    }
    // Runs this check every second to see if the Captain has enough Gold to make a new Unit.
    void FindCaptain()
    {
        // If theres a lot of Gold, create a Unit - when they build a House they increase Public.
        if (!Units[0].onSchedule)
        {
            if (Gold >= unitCost && Random.value > 0.5f && Public <= 75)
            {
                // Can't make Units once above a certain amount of Public.
                Units[0].Schedule.Add("Create Unit Worker");
                Gold -= unitCost; Public += 25;
            }
            // Else if Public is above Military, create Military.
            else if (Gold >= 40 && Random.value > 0.5f && Public > Military)
            {
                if (Random.value > 0.85f) { Units[0].Schedule.Add("Create Unit Scout"); }
                else Units[0].Schedule.Add("Create Unit Soldier");
                Gold -= 25; Public -= 35; Military += 25;
            }
            // If there is no Research, build a Research.
            else if (ResearchBuilding == null && Units.Count > 3 && Science <= 15)
            {
                Science += 25;
                Units[0].Schedule.Add("Create Research");
            }
        }
    }
    // Runs this check every second to see if any Unit currently has no task.
    void FindWorkers()
    {
        for (int i = 1; i < Units.Count; i++)
        {            
            if (Units[i].Role == "Worker" && !Units[i].onSchedule && Units[i].gameObject.activeSelf)
            {
                // If desperate for income, create a Farm.
                if (Gold <= 10f) { Units[i].Schedule.Add("Create Farm"); Gold += 15; }
                // If that Unit does not currently have a Schedule and the Town needs Gold, go gather.
                else if (Gold <= 50f && Random.value > 0.5f && Resources.Count > 15) Units[i].Schedule.Add("Gather");
                // If the Military is larger than Science, then go Research.
                else if (Military > Science && Science < 50f && ResearchBuilding != null && Random.value > 0.25f && isResearching < 10)
                {
                    Units[i].Schedule.Add("Research");
                    isResearching++;
                    if (isResearching == 1) StartCoroutine(startResearching());
                }
                // Otherwise make one of the other Buildings to increase Economy using Military might.
                else if (Military > 40 && Random.value > 0.25f) { Military -= 25; Units[i].Schedule.Add("Create Random"); }
            }
        }
    }
    public void Researching(Transform researchingUnit)
    {
        researchUnits.Add(researchingUnit);
        researchingUnit.gameObject.SetActive(false);
        Grid.RemoveWall(Mathf.RoundToInt(ResearchBuilding.transform.position.x), Mathf.RoundToInt(ResearchBuilding.transform.position.z - 1));
    }
    // Starts Research, once time is over lets out each Unit one by one.
    IEnumerator startResearching()
    {
        UpdateResources();
        CancelInvoke("SubtractScience");
        yield return new WaitUntil(() => researchUnits.Count > 0);
        ResearchBuilding.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(30f);
        Science = 100;
        isResearching = 10;
        yield return new WaitForSeconds(Random.Range(60f, 95f));
        Grid.RemoveWall(Mathf.RoundToInt(ResearchBuilding.transform.position.x), Mathf.RoundToInt(ResearchBuilding.transform.position.z - 1));
        while (researchUnits.Count != 0)
        {
            // Waits until the entrance to the Research is empty to start releasing them.
            yield return new WaitUntil(() => !Grid.CheckWall(Mathf.RoundToInt(ResearchBuilding.transform.position.x), Mathf.RoundToInt(ResearchBuilding.transform.position.z - 1)));
            // Gives the recently released Unit time to move.
            researchUnits[0].gameObject.SetActive(true);
            researchUnits[0].GetComponent<unitManager>().Schedule.RemoveAt(0); researchUnits[0].GetComponent<unitManager>().onSchedule = false;
            yield return new WaitUntil(() => researchUnits[0].GetComponent<unitManager>().Model.GetComponent<AIManager>().isMoving);
            researchUnits.RemoveAt(0);
            yield return null;
        }
        isResearching = 0;
        InvokeRepeating("SubtractScience", 0f, 5f);
        ResearchBuilding.GetChild(1).gameObject.SetActive(false);
    }
    void FindSoldiers()
    {
        for (int i = 1; i < Units.Count; i++)
        {
            if (Units[i].Role == "Soldier" && !Units[i].onSchedule)
            {
                // If the Castle has no guards, prioritise Guards with a random check to choose left or right.
                if (Random.value > 0.5f && !Grid.CheckWall(Mathf.RoundToInt(Buildings[0].position.x + 1), Mathf.RoundToInt(Buildings[0].position.z - 1)))
                {
                    // If that Unit does not currently have a Schedule defend Castle.
                    Units[i].Schedule.Add("Guard " + Mathf.RoundToInt(Buildings[0].position.x + 1) + "," + Mathf.RoundToInt(Buildings[0].position.z - 1));
                }
                else if (Random.value > 0.5f && !Grid.CheckWall(Mathf.RoundToInt(Buildings[0].position.x - 1), Mathf.RoundToInt(Buildings[0].position.z - 1)))
                {
                    // If that Unit does not currently have a Schedule defend Castle.
                    Units[i].Schedule.Add("Guard " + Mathf.RoundToInt(Buildings[0].position.x - 1) + "," + Mathf.RoundToInt(Buildings[0].position.z - 1));
                }
                // Otherwise guard a random building for a while.
                else if (Random.value > 0.75f)
                {
                    Transform Building = null;
                    Building = Buildings[Random.Range(0, Buildings.Count)];

                    while (Grid.CheckWall(Mathf.RoundToInt(Building.position.x - 1), Mathf.RoundToInt(Building.position.z - 1))
                           && Grid.CheckWall(Mathf.RoundToInt(Building.position.x + 1), Mathf.RoundToInt(Building.position.z - 1)))
                    {
                        Building = Buildings[Random.Range(0, Buildings.Count)];
                        return;
                    }
                    if (Building != null)
                    {
                        if (!Grid.CheckWall(Mathf.RoundToInt(Building.position.x - 1), Mathf.RoundToInt(Building.position.z - 1)))
                            Units[i].Schedule.Add("Guard " + Mathf.RoundToInt(Building.position.x - 1) + "," + Mathf.RoundToInt(Building.position.z - 1));
                        else
                            Units[i].Schedule.Add("Guard " + Mathf.RoundToInt(Building.position.x + 1) + "," + Mathf.RoundToInt(Building.position.z - 1));
                    }
                }
            }
            // Allows Scouts to roam.
            else if (Units[i].Role == "Scout" && !Units[i].onSchedule && Random.value > 0.5f) { Units[i].Schedule.Add("Scout"); }
        }
    }
    // Creates an enemy on the outskirts of the map, allow them to roam randomly.
    IEnumerator CreateEnemies()
    {
        yield return new WaitForSeconds(Random.Range(120f - (Units.Count * 2), 360f - (Units.Count * 2)));
        Debug.Log("Creating Enemy");
        // Selects a random radius on the outskirts of the map.
        int x = Random.Range(1, Grid.worldSize - 1);
        int y = Random.Range(1, Grid.worldSize - 1);

        // Makes sure the enemy lands on a position that is empty, and on the outskirts of the map.
        while (Grid.CheckWall(x, y) || (x > 6 && x < 18) || (y > 6 && y < 18))
        {
            x = Random.Range(1, Grid.worldSize - 1);
            y = Random.Range(1, Grid.worldSize - 1);
            yield return null;
        }

        // Creates the Prefab.
        Transform Enemy = Instantiate(unitPrefab) as Transform;
        // Updates its position, parent, and name.
        Enemy.position = new Vector3(x, 0, y);
        Enemy.parent = GameObject.Find("Enemies").transform;
        // Updates the pathfinding Grid to calculate it as an obstacle.
        Grid.SetWall(x, y);
        Enemies.Add(Enemy.GetComponent<unitManager>());
        // Updates the Units roles and sets its Sprite.
        Enemy.GetChild(0).GetComponent<SpriteRenderer>().sprite = unitModelPrefabs[Random.Range(5, 7)];
        Enemy.GetComponent<unitManager>().Model.GetComponent<AIManager>().movementSpeed = 0.5f;
        Enemy.GetComponent<unitManager>().Kingdom = this;
        Enemy.GetComponent<unitManager>().Name = Grid.GenerateName();
        Enemy.GetComponent<unitManager>().Role = "Raider";
        Enemy.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = "Raider\n" + Enemy.GetComponent<unitManager>().Name;
        Enemy.name = "Raider " + Enemy.GetComponent<unitManager>().Name;
        Enemy.GetComponent<unitManager>().Health = Random.Range(2, 7);
        Enemy.GetComponent<unitManager>().Schedule.Add("Raider Roam");

        // Makes its UI visible
        Enemy.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Enemy.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 1f);
        Enemy.GetChild(0).GetChild(1).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 0.75f);
        // And loops again to keep spawning opponents.
        StartCoroutine(CreateEnemies());
    }
    // Once the Captain is dead, start the game over.
    public IEnumerator GameOver()
    {
        GameObject.Find("Canvas").GetComponent<titleScreen>().Loaded = false; Time.timeScale = 0.5f; 
        LeanTween.alphaCanvas(GameOverObjectTitle.GetComponent<CanvasGroup>(), 1, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);
        LeanTween.alphaCanvas(GameOverObjectSubTitle.GetComponent<CanvasGroup>(), 1, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => Input.GetKey(KeyCode.R));
        LeanTween.alphaCanvas(GameObject.Find("Transition").GetComponent<CanvasGroup>(), 1, 0.5f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(0.5f);
        GameObject.Find("Canvas").GetComponent<titleScreen>().RestartScene();
    }
}