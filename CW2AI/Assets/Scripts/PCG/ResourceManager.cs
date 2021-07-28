// Stores the current value of each Resource.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [HideInInspector] public GridManager Grid;
    public string Object;
    public Vector3 Position;
    public List<Sprite> Sprites;

    // Once that resource is Gathered, start a short timer to reset it internally (e.g. Trees regrow after time).
    public IEnumerator Gathered()
    {
        if (Grid.Kingdom.Resources.Contains(this)) Grid.Kingdom.Resources.Remove(this);
        GetComponent<SpriteRenderer>().sprite = Sprites[1];
        yield return new WaitForSeconds(Random.Range(45, 95));
        // Creates a new resource that can now be gathered again.
        Grid.Kingdom.Resources.Add(this);
        GetComponent<SpriteRenderer>().sprite = Sprites[0];
    }
}