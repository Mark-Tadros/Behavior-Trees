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
    public GameObject Actions;
    public GameObject gameText;
    // Stores the Kingdoms Resources.
    public float Religion;
    RectTransform ReligionFill; 
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
    public Transform Church;
    public List<Transform> churchUnits;
    int isPraying;

    private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    void Awake()
    {
        Grid = GameObject.Find("pathfindingManager").GetComponent<GridManager>();
        isPraying = 0;
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
        InvokeRepeating("FindKing", 15f, 10f);
        InvokeRepeating("FindWorkers", 2.5f, 2.5f);
        InvokeRepeating("FindKnights", 5f, 5f);
        StartCoroutine(CreateEnemies());
    }
    public void UpdateResources()
    {
        // Updates the physical visuals once subtracted or added to.
        ReligionFill.localScale = new Vector3(1, Religion / 100, 1);
        if (Religion < 0) Religion = 0; if (Religion > 100) Religion = 100;
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
        Religion -= 0.1f;
        UpdateResources();
    }
    void SubtractReligion()
    {
        Religion -= 1f;
        UpdateResources();
    }
    // Create a new Unit.
    public void CreateUnit(string Role) { coroutineQueue.Enqueue(createUnit(Role)); }
    IEnumerator createUnit(string Role)
    {
        yield return new WaitUntil(() => !Grid.CheckWall(12, 11));
        int whatUnit = 0; float movementSpeed = 0;
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

        // Reveals the Unit slowly - quality of life feature.
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
        // Creates the Prefab and updates its values.
        Transform Building = Instantiate(buildingPrefabs[0]) as Transform;
        Building.position = new Vector3(whatPosition.x, 0, whatPosition.y);
        Building.parent = GameObject.Find("Buildings").transform;
        Building.name = whatPosition.x + "," + whatPosition.y + " " + whatBuilding;
        Grid.SetWall(Mathf.RoundToInt(whatPosition.x), Mathf.RoundToInt(whatPosition.y));
        Buildings.Add(Building);
        // If its a Castle, set the initial variables to call them later.
        switch (whatBuilding)
        {
            case "Castle":
                ReligionFill = Building.GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>();
                PublicFill = Building.GetChild(1).GetChild(1).GetChild(0).GetComponent<RectTransform>();
                MilitaryFill = Building.GetChild(1).GetChild(2).GetChild(0).GetComponent<RectTransform>();
                GoldFill = Building.GetChild(1).GetChild(3).GetChild(0).GetComponent<RectTransform>();
                break;
            case "Farm":
                Resources.Add(Building.GetChild(0).GetComponent<ResourceManager>());
                Building.GetChild(0).GetComponent<ResourceManager>().Grid = Grid;
                Building.GetChild(0).GetComponent<ResourceManager>().Position = new Vector3(whatPosition.x, 0, whatPosition.y - 1);
                Building.GetChild(0).GetComponent<ResourceManager>().StartCoroutine(Building.GetChild(0).GetComponent<ResourceManager>().Gathered());
                break;
        }
        // Creates a Road underneath all these buildings.
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
            LeanTween.alpha(Road.gameObject, 0.15f, 2.5f).setEase(LeanTweenType.easeInOutQuad);
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
    // Runs this check every second to see if the King has enough Gold to make a new Unit.
    void FindKing()
    {
        // If theres a lot of Gold, create a Unit - when they build a House they increase Public.
        if (!Units[0].onSchedule)
        {
            switch (Random.value)
            {
                case 0.1f:
                    // Can't make Units once above a certain amount of Public.
                    if (Public >= 75) break;
                    Units[0].Schedule.Add("Create Unit Worker");
                    Gold -= unitCost; Public += 25;
                    break;
                case 0.2f:
                    // Can't make Units once above a certain amount of Public.
                    if (Random.value > 0.85f) { Units[0].Schedule.Add("Create Unit Scout"); }
                    else Units[0].Schedule.Add("Create Unit Knight");
                    Gold -= 25; Public -= 35; Military += 25;
                    break;
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
                // If the Military is larger than Religion, then go Pray.
                else if (Military > Religion && Religion < 50f && Church != null && Random.value > 0.25f && isPraying < 10)
                {
                    Units[i].Schedule.Add("Pray");
                    isPraying++;
                    if (isPraying == 1) StartCoroutine(startPraying());
                }
                // Otherwise make one of the other Buildings to increase Economy using Military might.
                else if (Military > 40 && Random.value > 0.25f) { Military -= 25; Units[i].Schedule.Add("Create Random"); }
            }
        }
    }
    public void Praying(Transform prayingUnit)
    {
        churchUnits.Add(prayingUnit);
        prayingUnit.gameObject.SetActive(false);
        Grid.RemoveWall(Mathf.RoundToInt(Church.transform.position.x), Mathf.RoundToInt(Church.transform.position.z - 1));
    }
    // Starts Praying, once time is over lets out each Unit one by one.
    IEnumerator startPraying()
    {
        UpdateResources();
        CancelInvoke("SubtractReligion");
        yield return new WaitUntil(() => churchUnits.Count > 0);
        Church.GetChild(1).gameObject.SetActive(true);
        ShowText(" Start Mass", "[Religion++, Military--]");
        yield return new WaitForSeconds(30f);
        Religion = 100;
        isPraying = 10;
        yield return new WaitForSeconds(Random.Range(60f, 95f));
        Grid.RemoveWall(Mathf.RoundToInt(Church.transform.position.x), Mathf.RoundToInt(Church.transform.position.z - 1));
        while (churchUnits.Count != 0)
        {
            // Waits until the entrance to the Church is empty to start releasing them.
            yield return new WaitUntil(() => !Grid.CheckWall(Mathf.RoundToInt(Church.transform.position.x), Mathf.RoundToInt(Church.transform.position.z - 1)));
            // Gives the recently released Unit time to move.
            churchUnits[0].gameObject.SetActive(true);
            churchUnits[0].GetComponent<unitManager>().Schedule.RemoveAt(0); churchUnits[0].GetComponent<unitManager>().onSchedule = false;
            yield return new WaitUntil(() => churchUnits[0].GetComponent<unitManager>().Model.GetComponent<AIManager>().isMoving);
            churchUnits.RemoveAt(0);
            yield return null;
        }
        isPraying = 0;
        InvokeRepeating("SubtractReligion", 0f, 5f);
        Church.GetChild(1).gameObject.SetActive(false);
    }
    void FindKnights()
    {
        for (int i = 1; i < Units.Count; i++)
        {
            if (Units[i].Role == "Knight" && !Units[i].onSchedule)
            {
                // If the Castle has no guards, prioritise Guards with a random check to choose left or right.
                switch (Random.value)
                {
                    case 0.1f:
                        Units[i].Schedule.Add("Guard " + Mathf.RoundToInt(Buildings[0].position.x + 1) + "," + Mathf.RoundToInt(Buildings[0].position.z - 1));
                        break;
                    case 0.2f:
                        Units[i].Schedule.Add("Guard " + Mathf.RoundToInt(Buildings[0].position.x - 1) + "," + Mathf.RoundToInt(Buildings[0].position.z - 1));
                        break;
                    case 0.3f:
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
                        break;
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
    // Create a Text based on each action.
    public void ShowText(string text, string cost)
    {
        if (Actions.transform.GetChild(0).childCount > 9) Destroy(Actions.transform.GetChild(0).GetChild(0).gameObject);
        StartCoroutine(showText(text, cost));
    }
    IEnumerator showText(string text, string cost)
    {
        GameObject tempChat = Instantiate(gameText);
        LeanTween.alphaCanvas(tempChat.GetComponent<CanvasGroup>(), 1, 5f).setEase(LeanTweenType.easeInQuad);
        tempChat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        tempChat.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = cost;
        tempChat.transform.SetParent(Actions.transform.GetChild(0), false);
        yield return new WaitForSeconds(25f);
        if (tempChat != null) LeanTween.alphaCanvas(tempChat.GetComponent<CanvasGroup>(), 0, 2.5f).setEase(LeanTweenType.easeInQuad);
        yield return new WaitForSeconds(3f);
        if (tempChat != null) Destroy(tempChat);
    }
    // Once the King is dead, start the game over.
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