using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTemp : MonoBehaviour
{
    public GameObject creepPrefab;
    public float radius = 5.0f;
    public int creepCount = 100;
    public List<Creep> creeps; // default non-flocking behavior
    public float minDistanceFromPlayerCreepSpawn = 10f;
    private List<Transform> playerPositions;
    // Start is called before the first frame update
    void Start()
    {
        // do not spawn very near the players
        playerPositions = new List<Transform>();
        foreach (var player in GetComponents<Player>())
            playerPositions.Add(player.transform);

        for (int i = 0; i < creepCount; ++i)
            SpawnCreeps();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnCreeps()
    {
        bool isOk = false;
        Vector2 point = Vector2.zero;
        while (!isOk)
        {
            point = Random.insideUnitCircle * radius;
            if (Vector3.Distance(new Vector3(point.x, 0f, point.y), transform.position) > minDistanceFromPlayerCreepSpawn)
                isOk = true;
        }
        GameObject newCreep = Instantiate(creepPrefab, new Vector3(point.x, 0.0f, point.y), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)) as GameObject;
        newCreep.transform.parent = GameObject.Find("Creeps").transform;
        newCreep.name = "Creep_" + creeps.Count.ToString();
        creeps.Add(newCreep.GetComponent<Creep>());
    }
}
