using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalFlocks : MonoBehaviour
{
    Dictionary<string, HashSet<GameObject>> flocks;
    Dictionary<string, Transform> flocksGoals; // player transform in each of the flocks
    // Start is called before the first frame update
    void Start()
    {
        flocks = new Dictionary<string, HashSet<GameObject>>();
        flocksGoals = new Dictionary<string, Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddFlock(string tag, HashSet<GameObject> flock)
    {
        flocks.Add(tag, flock);
    }

    public void UpdateFlock(string tag, HashSet<GameObject> flock)
    {
        // update the flog at player index
        if (flocks.ContainsKey(tag))
            flocks[tag] = flock;
        else
            Debug.LogError("UPDATE Could not find the key: " + tag);
    }

    public HashSet<GameObject> GetFlock(string tag)
    {
        if (flocks.ContainsKey(tag))
            return flocks[tag];
        else
        {
            Debug.LogError("GET Could not find the key: " + tag);
            return new HashSet<GameObject>();
        }
    }
    public void AddPlayerTransform(string tag, Transform transform)
    {
        flocksGoals.Add(tag, transform);
    }

    public void UpdatePlayerTransform(string tag, Transform transform)
    {
        if (flocksGoals.ContainsKey(tag))
            flocksGoals[tag] = transform;
        else
            Debug.LogError("UPDATE Could not find the key: " + tag);
    }

    public Transform GetPlayerTransform(string tag)
    {
        if (flocksGoals.ContainsKey(tag))
            return flocksGoals[tag];
        else
        {
            Debug.LogError("GET Could not find the key: " + tag);
            return gameObject.AddComponent<Transform>(); // HARD ERROR LOL
        }
    }
}
