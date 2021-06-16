// Individual Unit behaviour trees that take commands from the kingdomManager.cs.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class unitManager : MonoBehaviour
{
    [HideInInspector] public kingdomManager Kingdom;
    public GameObject Model;
    AIManager AI;
    public Transform Target;
    public string Name;
    public float Health;
    public string Role;
    public TextMeshPro Status;
    public SpriteRenderer Emote;

    public bool onSchedule;
    public List<string> Schedule;

    public Transform Enemy;

    void Start()
    {
        AI = Model.GetComponent<AIManager>();
        // Start checking that Units schedule every tick.
        InvokeRepeating("CheckSchedule", 0.25f, 2.5f);
    }
    // Runs every Second to check if any commands are currently in the Schedule;
    public void CheckSchedule()
    {
        // Small check to reset AI if needed.
        if (Status.text == "deceased")
        {
            CancelInvoke("CheckSchedule");
            return;
        }
        else if (Schedule.Count > 0 && !onSchedule)
        {
            Debug.Log(Schedule[0]);
            // Determines what Task was set and carries it out.
            switch (Schedule[0])
            {
                case "Create Unit":
                    string whatUnit = Schedule[0].Replace("Create Unit ", "");
                    StartCoroutine(CreateUnit(whatUnit));
                    break;
                case "Create Building":
                    string whatBuilding = Schedule[0].Replace("Create ", "");
                    StartCoroutine(CreateBuilding(whatBuilding));
                    break;
                case "Gather":
                    StartCoroutine(Gather());
                    break;
                case "Pray":
                    StartCoroutine(Pray());
                    break;
                case "Scout":
                    StartCoroutine(Scout());
                    break;
                case "Guard":
                    // Recieves the positions its required to stand Guard.
                    char[] whatPosition = Schedule[0].Replace("Guard ", "").ToCharArray();
                    string x = ""; string y = ""; bool xy = false;
                    for (int i = 0; i < whatPosition.Length; i++)
                    {
                        if (whatPosition[i].ToString() == ",") xy = true;
                        else if (!xy) x += whatPosition[i].ToString();
                        else if (xy) y += whatPosition[i].ToString();
                    }
                    // One final check to see if it's impossible to reach there.
                    if (Kingdom.Grid.CheckWall(int.Parse(x), int.Parse(y))) { Schedule.RemoveAt(0); onSchedule = false; }
                    else StartCoroutine(Guard(int.Parse(x), int.Parse(y)));
                    break;
                case "Raider Roam": 
                    if (Status.text != "wandering" && Status.text != "attacking") StartCoroutine(RaiderRoam());
                    break;
                case "Wandering":
                    // Runs a check to see if any Units are nearby in order to attack them.
                    Enemy = null;
                    float minDistance = Mathf.Infinity;
                    float attackDistance = 2 + Kingdom.Enemies.Count;
                    if (Kingdom.Enemies.Count > 8) attackDistance = 10;
                    if (Kingdom.Units[0].Health <= 0) attackDistance = 100;
                    foreach (unitManager Unit in Kingdom.Units)
                    {
                        float Distance = Vector3.Distance(Unit.transform.GetChild(0).position, Model.transform.position);
                        if (Distance < minDistance && Distance <= attackDistance && Unit.Health > 0)
                        {
                            Enemy = Unit.transform;
                            minDistance = Distance;
                        }
                    }
                    //if (Kingdom.Units.Count == 1) Enemy = Kingdom.Units[0].transform;
                    // Start the attacking process.
                    if (Enemy != null)
                    {
                        Enemy.GetComponent<unitManager>().Attacked(this);
                        StartCoroutine(Attacking());
                    }
                    break;
            }
        }
        // Continues roaming if there are no more Tasks.
        else if (!onSchedule && Status.text != "waiting") StartCoroutine(Roam());
    }
    IEnumerator Bugged()
    {
        yield return new WaitForSeconds(25f);
        if (Status.text == "thinking") Status.text = "";
    }
    // Allows the Unit to slowly and randomly roam around the map if Schedule is empty.
    IEnumerator Roam()
    {
        Status.text = "waiting";
        // Starts the loop to continue Roaming until something breaks it.
        while (!onSchedule)
        {
            float updatedRadius = Kingdom.roamRadius;
            if (Role == "King") { if (updatedRadius > 3) updatedRadius = 3; yield return new WaitForSeconds(1.5f); }
            if (onSchedule) yield break;
            // Selects a random radius between the roam radius and the Castle.
            int x = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.x - updatedRadius), Mathf.RoundToInt(Kingdom.Buildings[0].position.x + updatedRadius));
            int y = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.z - (updatedRadius * 1.5f)), Mathf.RoundToInt(Kingdom.Buildings[0].position.z + (updatedRadius * 0.5f)));

            // Makes sure the roam lands on a position that Unit can currently walk.
            while (Kingdom.Grid.CheckWall(x, y))
            {
                x = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.x - updatedRadius), Mathf.RoundToInt(Kingdom.Buildings[0].position.x + updatedRadius));
                y = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.z - (updatedRadius * 1.5f)), Mathf.RoundToInt(Kingdom.Buildings[0].position.z + (updatedRadius * 0.5f)));
                yield return null;
            }

            if (onSchedule) yield break;
            Target.position = new Vector3(x, 0.5f, y);

            // Make sure to wait until it starts moving, and then reaches its destination and sometimes plays an Emote.
            yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
            yield return new WaitUntil(() => !AI.isMoving);
            if (onSchedule) yield break;
            // Shows a emote every couple of times.
            if (Random.value > 0.9f) ShowEmote(4);
            yield return new WaitForSeconds(Random.Range(1.5f, 5f));
            HideEmote();
        }
    }
    // Creates a Unit;
    IEnumerator CreateUnit(string Role)
    {
        onSchedule = true;
        // Ensure the Castle entrance is free, before moving to make a Unit.
        yield return new WaitUntil(() => !AI.isMoving);
        Status.text = "thinking";
        StartCoroutine(Bugged());
        // Waits until the Castle entrance is empty in order to create More, or checks if Unit is currently standing on top of the Castle..
        yield return new WaitUntil(() => !Kingdom.Grid.CheckWall(12, 11) || Target.transform.position == new Vector3(12, Target.transform.position.y, 11) || Status.text == "");
        if (Status.text == "")
        {
            Target.transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z - 1);
            onSchedule = false;
            yield break;
        }

        Status.text = "hiring";
        Target.position = new Vector3(12f, 0.5f, 11f);

        // Make sure to wait until it starts moving, and then reaches its destination.
        yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
        if (AI.isMoving) ShowEmote(0);
        yield return new WaitUntil(() => !AI.isMoving);
        HideEmote();
        yield return new WaitForSeconds(2.5f);
        Kingdom.CreateUnit(Role);
        yield return new WaitForSeconds(1.5f);
        if (Schedule.Count > 0) Schedule.RemoveAt(0); onSchedule = false;
    }
    IEnumerator CreateBuilding(string whatBuilding)
    {
        onSchedule = true;
        // Ensure the Unit is not moving.
        yield return new WaitUntil(() => !AI.isMoving);
        Status.text = "thinking";
        StartCoroutine(Bugged());
        // Selects a random radius between the roam radius and the Castle.
        int x = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.x - (Kingdom.roamRadius * 0.75f)), Mathf.RoundToInt(Kingdom.Buildings[0].position.x + (Kingdom.roamRadius * 0.75f)));
        int y = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.z - ((Kingdom.roamRadius * 0.75f) * 1.5f)), Mathf.RoundToInt(Kingdom.Buildings[0].position.z + ((Kingdom.roamRadius * 0.75f) * 0.5f)));

        // Makes sure the spot to move and the spot to place the building are both Empty, and not in front of a Building.
        bool Contains = false;
        for (int i = 0; i < Kingdom.Buildings.Count; i++) { if (x == Kingdom.Buildings[i].position.x && y == Kingdom.Buildings[i].position.z - 2) Contains = true; }
        while (Kingdom.Grid.CheckWall(x, y) || Kingdom.Grid.CheckWall(x, y + 1) || Contains || x == 12 || x == 0 || y == 0 || x == 24 || y == 24 || Kingdom.Grid.CheckRoad(x, y) || Kingdom.Grid.CheckRoad(x, y + 1))
        {
            x = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.x - (Kingdom.roamRadius * 0.75f)), Mathf.RoundToInt(Kingdom.Buildings[0].position.x + (Kingdom.roamRadius * 0.75f)));
            y = Random.Range(Mathf.RoundToInt(Kingdom.Buildings[0].position.z - ((Kingdom.roamRadius * 0.75f) * 1.5f)), Mathf.RoundToInt(Kingdom.Buildings[0].position.z + ((Kingdom.roamRadius * 0.75f) * 0.5f)));
            Contains = false;
            for (int i = 0; i < Kingdom.Buildings.Count; i++) { if (x == Kingdom.Buildings[i].position.x && y == Kingdom.Buildings[i].position.z - 2) Contains = true; }
            yield return null;
        }
        Target.position = new Vector3(x, 0.5f, y);
        Kingdom.Grid.SetWall(x, y + 1);
        
        // Make sure to wait until it starts moving, and then reaches its destination.
        yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
        if (AI.isMoving)
        {
            if (whatBuilding == "House") ShowEmote(2);
            else ShowEmote(0);
        }
        yield return new WaitUntil(() => !AI.isMoving);
        Status.text = "building";
        HideEmote();
        yield return new WaitForSeconds(10f);
        Kingdom.CreateBuilding(whatBuilding, new Vector2(x, y + 1));
        yield return new WaitForSeconds(1.5f);
        if (Schedule.Count > 0) Schedule.RemoveAt(0); onSchedule = false;
    }
    IEnumerator Gather()
    {
        onSchedule = true;
        // Ensure the Unit is not moving.
        yield return new WaitUntil(() => !AI.isMoving);
        Status.text = "thinking";
        StartCoroutine(Bugged());
        // Picks the closest resource, and removes it from the current list.
        ResourceManager Object = null;
        float minDistance = Mathf.Infinity;

        foreach (ResourceManager Resource in Kingdom.Resources)
        {
            float Distance = Vector3.Distance(Resource.Position, Kingdom.Buildings[0].transform.position);
            if (Distance < minDistance && Distance < Kingdom.roamRadius * 1.5f)
            {
                if (!Kingdom.Grid.CheckWall(Mathf.RoundToInt(Resource.Position.x), Mathf.RoundToInt(Resource.Position.z)))
                {
                    Object = Resource;
                    minDistance = Distance;
                }
            }
        }
        if (Object == null) { Schedule.RemoveAt(0); onSchedule = false; yield break; }

        Kingdom.Resources.Remove(Object);
        // Makes sure the position to gather the Resource is empty.
        yield return new WaitUntil(() => !Kingdom.Grid.CheckWall(Mathf.RoundToInt(Object.Position.x), Mathf.RoundToInt(Object.Position.z))
                                         || Target.transform.position == new Vector3(Mathf.RoundToInt(Object.Position.x), Target.transform.position.y, Mathf.RoundToInt(Object.Position.z)));

        Status.text = "gathering";
        Target.position = new Vector3(Mathf.RoundToInt(Object.Position.x), 0.5f, Mathf.RoundToInt(Object.Position.z));

        // Make sure to wait until it starts moving, and then reaches its destination.
        yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
        if (AI.isMoving) ShowEmote(0);
        yield return new WaitUntil(() => !AI.isMoving);
        HideEmote();
        yield return new WaitForSeconds(Random.Range(7.5f, 10f));
        Kingdom.Gold += Random.Range(3, 7.5f);
        Kingdom.UpdateResources();
        Object.StartCoroutine(Object.Gathered());
        yield return new WaitForSeconds(1.5f);
        Schedule.RemoveAt(0); onSchedule = false;
    }
    IEnumerator Pray()
    {
        onSchedule = true;
        // Ensure the Unit is not moving.
        yield return new WaitUntil(() => !AI.isMoving);
        Status.text = "thinking";
        StartCoroutine(Bugged());
        // Waits until the Church entrance is empty.
        yield return new WaitUntil(() => !Kingdom.Grid.CheckWall(Mathf.RoundToInt(Kingdom.Church.position.x), Mathf.RoundToInt(Kingdom.Church.position.z - 1))
                                         || Target.transform.position == new Vector3(Mathf.RoundToInt(Kingdom.Church.position.x), Target.transform.position.y, Mathf.RoundToInt(Kingdom.Church.position.z - 1)));

        Status.text = "praying";
        Target.position = new Vector3(Mathf.RoundToInt(Kingdom.Church.position.x), 0.5f, Mathf.RoundToInt(Kingdom.Church.position.z - 1));

        // Make sure to wait until it starts moving, and then reaches its destination.
        yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
        if (AI.isMoving)
        {
            if (Random.value > 0.9f) ShowEmote(2);
            else if (Random.value > 0.8f) ShowEmote(3);
        }
        yield return new WaitUntil(() => !AI.isMoving);
        HideEmote();
        yield return new WaitForSeconds(2.5f);
        Kingdom.Praying(this.transform);
    }
    // Allows the Scouts to look around the map and reveal enemies or resources.
    IEnumerator Scout()
    {
        onSchedule = true;
        // Ensure the Unit is not moving.
        yield return new WaitUntil(() => !AI.isMoving);
        Status.text = "thinking";
        // Selects a random radius between the roam radius and the Castle.
        int x = Random.Range(3, 22); int y = Random.Range(3, 22);

        // Makes sure the roam lands on a position that Unit can currently walk.
        while (Kingdom.Grid.CheckWall(x, y))
        {
            x = Random.Range(3, 22); y = Random.Range(3, 22);
            yield return null;
        }

        Status.text = "scouting";
        Target.position = new Vector3(x, 0.5f, y);
        
        // Make sure to wait until it starts moving, and then reaches its destination.
        yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
        yield return new WaitUntil(() => !AI.isMoving);
        if (!AI.isMoving && Random.value > 0.5f) ShowEmote(1);
        yield return new WaitForSeconds(Random.Range(2.5f, 7.5f));
        HideEmote();
        yield return new WaitForSeconds(1.5f);
        Schedule.RemoveAt(0); onSchedule = false;
    }
    IEnumerator Guard(int x, int y)
    {
        onSchedule = true;
        // Ensure the Unit is not moving.
        yield return new WaitUntil(() => !AI.isMoving);
        Status.text = "thinking";
        // Checks if that Guarding spot is empty to Guard it.
        yield return new WaitUntil(() => !Kingdom.Grid.CheckWall(x, y) || Target.transform.position == new Vector3(x, Target.transform.position.y, y));
        Status.text = "guarding";
        Target.position = new Vector3(x, 0.5f, y);

        // Make sure to wait until it starts moving, and then reaches its destination.
        yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
        if (AI.isMoving) ShowEmote(1);
        yield return new WaitUntil(() => !AI.isMoving);
        HideEmote();
        // Remains guarding for x amount of time.
        yield return new WaitForSeconds(Random.Range(35f, 60f));
        Schedule.RemoveAt(0); onSchedule = false;
    }
    public void Attacked(unitManager Enemy)
    {
        StopAllCoroutines();
        // Upon getting attacked wait until combat is over.
        if (Random.value > 0.5f) ShowEmote(3);
        Status.text = "defending";
        Target.transform.position = new Vector3(Model.transform.position.x, Target.transform.position.y, Model.transform.position.z);
        onSchedule = true; AI.isStuck = false;
    }
    // Allows the Unit to slowly and randomly roam around the map if Schedule is empty.
    IEnumerator RaiderRoam()
    {
        Status.text = "wandering";
        // Starts the loop to continue Roaming until something breaks it.
        while (!onSchedule)
        {
            if (onSchedule) yield break;
            // Selects a random radius between the roam radius and the Castle.
            int x = Random.Range(1, 24); int y = Random.Range(1, 24);

            // Makes sure the roam lands on a position that Unit can currently walk.
            while (Kingdom.Grid.CheckWall(x, y))
            {
                x = Random.Range(1, 24); y = Random.Range(1, 24);
                yield return null;
            }

            if (onSchedule) yield break;
            Target.position = new Vector3(x, 0.5f, y);

            // Make sure to wait until it starts moving, and then reaches its destination and sometimes plays an Emote.
            yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z));
            yield return new WaitUntil(() => !AI.isMoving);
            if (onSchedule) yield break;
            // Shows a emote every couple of times.
            if (Random.value > 0.5f) ShowEmote(3);
            yield return new WaitForSeconds(Random.Range(2.5f, 7.5f));
            HideEmote();
        }
    }
    IEnumerator Attacking()
    {
        onSchedule = true;
        Status.text = "attacking";

        // Calculates the closest position to the Enemy.
        List<Vector3> possiblePositions = new List<Vector3>();
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x + 1, 0, Enemy.GetChild(0).position.z));
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x - 1, 0, Enemy.GetChild(0).position.z));
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x, 0, Enemy.GetChild(0).position.z + 1));
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x, 0, Enemy.GetChild(0).position.z - 1));
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x + 1, 0, Enemy.GetChild(0).position.z + 1));
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x + 1, 0, Enemy.GetChild(0).position.z - 1));
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x - 1, 0, Enemy.GetChild(0).position.z + 1));
        possiblePositions.Add(new Vector3(Enemy.GetChild(0).position.x - 1, 0, Enemy.GetChild(0).position.z - 1));

        // Runs a check to see if any Units are nearby in order to attack them.
        Vector3 enemyPosition = Vector3.zero;
        float minDistance = Mathf.Infinity;

        foreach (Vector3 Positions in possiblePositions)
        {
            float Distance = Vector3.Distance(Positions, Model.transform.position);
            if (Distance < minDistance && !Kingdom.Grid.CheckWall(Mathf.RoundToInt(Positions.x), Mathf.RoundToInt(Positions.z)))
            {
                enemyPosition = Positions;
                minDistance = Distance;
            }
        }
        // Start the attacking process, if it can't find a position then reset.
        if (enemyPosition == Vector3.zero || Kingdom.Grid.CheckWall(Mathf.RoundToInt(enemyPosition.x), Mathf.RoundToInt(enemyPosition.z)))
        {
            Enemy.GetComponent<unitManager>().Status.text = "";
            Enemy.GetComponent<unitManager>().onSchedule = false;
            Enemy = null;
            Status.text = "";
            onSchedule = false;
            yield break;
        }

        if (Random.value > 0.5f) ShowEmote(3);
        Target.position = new Vector3(enemyPosition.x, 0.5f, enemyPosition.z);

        // Make sure to wait until it starts moving, and then reaches its destination and sometimes plays an Emote.
        yield return new WaitUntil(() => AI.isMoving || Model.transform.position == new Vector3(Target.transform.position.x, Model.transform.position.y, Target.transform.position.z) || AI.isStuck);
        // Creates a small check to see if the Unit can reach the Enemy.
        if (AI.isStuck)
        {
            Enemy.GetComponent<unitManager>().Status.text = "";
            Enemy.GetComponent<unitManager>().onSchedule = false;
            Enemy = null;
            Status.text = "";
            onSchedule = false;
            yield break;
        }
        yield return new WaitUntil(() => !AI.isMoving);
        Enemy.GetComponent<unitManager>().HideEmote();
        Kingdom.ShowText(" " + Enemy.GetComponent<unitManager>().name + " is being attacked!", "");

        // Fight until either the Enemy, or this Unit end up dying.
        while (Enemy.GetComponent<unitManager>().Health > 0 && Health > 0)
        {
            yield return new WaitForSeconds(1f);
            Enemy.GetComponent<unitManager>().Health -= Random.Range(0, 3);
            yield return new WaitForSeconds(1f);
            Health--;
        }
        if (Health <= 0)
        {
            Enemy.GetComponent<unitManager>().Status.text = "";
            Enemy.GetComponent<unitManager>().onSchedule = false;
            Dead();
        }
        else
        {
            Kingdom.Units.Remove(Enemy.GetComponent<unitManager>());
            Kingdom.ShowText(" " + Enemy.GetComponent<unitManager>().name + " has died.", "");
            Enemy.GetComponent<unitManager>().Dead();
            Kingdom.Military -= 25;
            Kingdom.Public -= 5;
            Kingdom.UpdateResources();
            Enemy = null;
            Status.text = "";
            onSchedule = false;
        }
    }
    public void Dead()
    {
        // Resets variables for that Unit and makes it deceased.
        StopAllCoroutines();
        HideEmote();
        Status.text = "deceased";
        Target.transform.position = new Vector3(Model.transform.position.x, Target.transform.position.y, Model.transform.position.z);
        onSchedule = false; AI.isStuck = false;
        name += " Deceased";

        Model.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        Model.transform.GetChild(0).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 0.5f);
        Model.transform.GetChild(1).GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 0.4f);
        Model.GetComponent<SpriteRenderer>().sortingOrder = -1;

        Model.transform.Rotate(45, 0, 0);

        Kingdom.Grid.RemoveWall(Mathf.RoundToInt(Target.transform.position.x), Mathf.RoundToInt(Target.transform.position.z));

        // Triggers the game over scene, stopping the game.
        if (Role == "King")
        {
            Kingdom.StartCoroutine(Kingdom.GameOver());
        }
        else if (Kingdom.Units.Count == 1)
        {
            Kingdom.GameOverObjectTitle.GetComponent<TextMeshProUGUI>().text = "The King has been abandoned.";
            Kingdom.StartCoroutine(Kingdom.GameOver());
        }
    }
    // Plays an emote, before hiding it after that activity is over.
    void ShowEmote(int emoteInt)
    {
        if (!AI.isStuck)
        {
            Emote.sprite = AI.emotePrefabs[emoteInt];
            Model.transform.GetChild(0).gameObject.SetActive(false); Model.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    void HideEmote()
    {
        Emote.sprite = null;
        Model.transform.GetChild(0).gameObject.SetActive(true); Model.transform.GetChild(1).gameObject.SetActive(true);
    }
}