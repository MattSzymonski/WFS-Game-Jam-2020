using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTemp : MonoBehaviour
{
    public GameObject creepPrefab;
    public float radius = 5.0f;
    public int creepCount = 100;
    public List<Creep> creeps; // default non-flocking behavior
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < creepCount; ++i)
            SpawnCreeps();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnCreeps()
    {                    
        Vector2 point = Random.insideUnitCircle * radius;
        GameObject newCreep = Instantiate(creepPrefab, new Vector3(point.x, 0.0f, point.y), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)) as GameObject;
        newCreep.transform.parent = GameObject.Find("Creeps").transform;
        newCreep.name = "Creep_" + creeps.Count.ToString();
        creeps.Add(newCreep.GetComponent<Creep>());
    }
}
