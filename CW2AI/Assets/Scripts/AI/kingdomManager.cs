// Contains the Kingdom HiveMind AI and gives out Schedules to each Unit depending on needs.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class kingdomManager : MonoBehaviour
{
    public GridManager Grid;
    public GameObject GameOverObjectTitle; public GameObject GameOverObjectSubTitle;
    public GameObject Actions;
    public GameObject gameText;
    // Stores the Kingdoms Resources.
    public float Religion; RectTransform ReligionFill; 
    public float Public; RectTransform PublicFill;
    public float Military; RectTransform MilitaryFill;
    public float Gold; RectTransform GoldFill;
    public float roamRadius; public float unitCost;
    // Stores all the Prefabs
    public Transform unitPrefab;
    public List<Transform> buildingPrefabs;
    public List<Sprite> unitModelPrefabs;
    // Stores all created Buildings and Units.
    public List<ResourceManager> Resources;
    public List<unitManager> Units;
    public List<unitManager> Enemies;
    public List<Transform> Buildings;
    public List<Transform> Roads;
    [HideInInspector] public Transform Church;
    public List<Transform> churchUnits;

    private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    // Starts a coroutine queue in order to create events sequentially.
    void Awake() { StartCoroutine(CoroutineCoordinator()); }
    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0) yield return StartCoroutine(coroutineQueue.Dequeue());
            yield return null;
        }
    }
    // Calls the incremental checks to control the over-arching AI decision making.
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
        Gold -= 0.25f; Military += 0.25f; Public -= 0.1f; Religion -= 0.1f;
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
        // Determines what Unit it is.
        if (Role == "King") { whatUnit = 0; movementSpeed = 0.35f; roamRadius = 2.25f; unitCost = 15; }
        else if (Role == "Worker") { whatUnit = 1; movementSpeed = 0.5f; roamRadius += 0.5f; unitCost += 2.5f; }
        else if (Role == "Knight") { whatUnit = 4; movementSpeed = 0.5f; roamRadius += 0.75f; InvokeRepeating("SubtractGold", 0f, 5f); unitCost += 5; }
        else if (Role == "Scout") { whatUnit = 3; movementSpeed = 0.75f; roamRadius += 1.0f; InvokeRepeating("SubtractGold", 0f, 7.5f); unitCost += 2.5f; }
        if (roamRadius >= 7) roamRadius = 7; if (unitCost > 60) unitCost = 60;
        UpdateResources();
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
        if (Role == "King") { Unit.GetComponent<unitManager>().Health = 7; }
        else if (Role == "Worker") { Unit.GetComponent<unitManager>().Schedule.Add("Create House"); Unit.GetComponent<unitManager>().Health = 3; }
        else if (Role == "Scout") { Unit.GetComponent<unitManager>().Schedule.Add("Scout"); Unit.GetComponent<unitManager>().Health = 5; }
        else if (Role == "Knight" && Random.value > 0.75f) { Unit.GetComponent<unitManager>().Schedule.Add("Create Tower"); Unit.GetComponent<unitManager>().Health = 7; }
        if (Role != "King") ShowText(" Hire " + Role, "[Radius++, Gold--]");
    }
    // Creates a new Building.
    public void CreateBuilding(string whatBuilding, Vector2 whatPosition)
    {
        int WhatBuilding = 0;
        // Determines what Building it is.
        if (whatBuilding == "Castle") WhatBuilding = 0;
        else if (whatBuilding == "Tower") WhatBuilding = 1;
        else if (whatBuilding == "House") { if (Random.value > 0.25f) WhatBuilding = 2; else WhatBuilding = 3; }
        else if (whatBuilding == "Church") WhatBuilding = 13;
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
            ReligionFill = Building.GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>();
            PublicFill = Building.GetChild(1).GetChild(1).GetChild(0).GetComponent<RectTransform>();
            MilitaryFill = Building.GetChild(1).GetChild(2).GetChild(0).GetComponent<RectTransform>();
            GoldFill = Building.GetChild(1).GetChild(3).GetChild(0).GetComponent<RectTransform>();
        }
        else if (whatBuilding == "Church") { Church = Building; InvokeRepeating("SubtractReligion", 0f, 5f); }
        else if (whatBuilding == "Farm")
        {
            Resources.Add(Building.GetChild(0).GetComponent<ResourceManager>());
            Building.GetChild(0).GetComponent<ResourceManager>().Grid = Grid;
            Building.GetChild(0).GetComponent<ResourceManager>().Position = new Vector3(whatPosition.x, 0, whatPosition.y - 1);
            Building.GetChild(0).GetComponent<ResourceManager>().StartCoroutine(Building.GetChild(0).GetComponent<ResourceManager>().Gathered());
        }
    }
    // Runs this check to see if the King has enough Gold to make a new Unit.
    void FindKing()
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
                else Units[0].Schedule.Add("Create Unit Knight");
                Gold -= 25; Public -= 35; Military += 25;
            }
            // If there is no Church, build a Church.
            else if (Church == null && Units.Count > 3 && Religion <= 15)
            {
                Religion += 25;
                Units[0].Schedule.Add("Create Church");
            }
        }
    }
    // Runs this check to see if any Unit currently has no task.
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
    int isPraying = 0; public void Praying(Transform prayingUnit)
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
    // Runs this check to see if any Knight currently has no task.
    void FindKnights()
    {
        for (int i = 1; i < Units.Count; i++)
        {
            if (Units[i].Role == "Knight" && !Units[i].onSchedule)
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
        // And loops again to keep spawning opponents after a certain time.
        StartCoroutine(CreateEnemies());
    }
    // Create a Text based on each action.
    public void ShowText(string text, string cost)
    {
        if (Actions.transform.GetChild(0).childCount > 9) Destroy(Actions.transform.GetChild(0).GetChild(0).gameObject);
        StartCoroutine(showText(text, cost));
    }
    // Quality of life feature to reveal each text.
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
    // Once the King is dead, start the game over when player presses R.
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