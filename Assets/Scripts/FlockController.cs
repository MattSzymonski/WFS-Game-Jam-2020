using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using MightyGamePack;

public class FlockController : MonoBehaviour
{
    public IBehavior behavior;
    public HashSet<FlockAgent> agents = new HashSet<FlockAgent>();

    public Player player;

    public float radius = 5.0f;
    public float agentSpeed = 5.0f;

    public float driveFactor = 10f;
    [Range(0.0f, 5.0f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        //behavior = ScriptableObject.CreateInstance("AlignmentBehavior") as IBehavior;
        player = gameObject.GetComponent<Player>();
        //FindFlock();
    }

    // Update is called once per frame
    void Update()
    {
        if (MainGameManager.mainGameManager.gameState == GameState.Playing)
        {
            HashSet<FlockAgent> difference = new HashSet<FlockAgent>();

            // Process player
            agents.UnionWith(GetNearby(player.gameObject).Item2);
            // Process each other member of the flock (agent)
            foreach (FlockAgent agent in agents)
            {
                if (!agent.isValid)
                    continue;
                HashSet<FlockAgent> agentsFound = ProcessNearby(agent);
                difference.UnionWith(agentsFound);
            }
            // Finally expand the set with the newly found agents
            agents.UnionWith(difference);
        }
    }

    HashSet<FlockAgent> ProcessNearby(FlockAgent source)
    {
        Tuple<List<Transform>, HashSet<FlockAgent>> nearbyTransformsAgents = GetNearby(source.gameObject);

        Vector3 move = behavior.CalculateMovement(source, nearbyTransformsAgents.Item1, this);

        //Vector3 move = player.transform.position - source.transform.position;
        //move = move.normalized * agentSpeed;
        move *= driveFactor;
        if (move.sqrMagnitude > agentSpeed*agentSpeed)
        {
            move = move.normalized;
            move *= agentSpeed;
        }
        source.Move(move);
        return nearbyTransformsAgents.Item2;
    }

    void FindFlock()
    {
        GameObject[] flock = GameObject.FindGameObjectsWithTag("Flock");
        foreach (var o in flock)
            agents.Add(o.GetComponent<FlockAgent>());
    }

    Tuple<List<Transform>, HashSet<FlockAgent>> GetNearby(GameObject agent)
    {
        List<Transform> nearbyTransforms = new List<Transform>();
        HashSet<FlockAgent> nearbyAgents = new HashSet<FlockAgent>();

        Collider[] nearbyColliders = Physics.OverlapSphere(agent.transform.position, radius);
        //Debug.LogError("Colliders found :" + nearbyColliders.Length);

        foreach (var c in nearbyColliders)
        {
            // TODO: potential problem, when del;eting objects from this flock, they will be readded here as it runs on update
            // add tags discarded etc
            // get tags only current player and neutral not discarded

            if (c == agent.GetComponent<Collider>())
            {
                // if myself, skip
                continue;
            }    

            // Only add to list if overlaps with agent
            var collAgent = c.GetComponentInParent<FlockAgent>();
            var creep = c.GetComponentInParent<Creep>();
            if (collAgent)
            {
                if (!collAgent.isValid)
                    continue;
                nearbyTransforms.Add(c.transform);
                nearbyAgents.Add(collAgent);
            }
            else if (c.GetComponentInParent<Player>())
            {
                nearbyTransforms.Add(c.transform);
            }
            else if (creep && !c.tag.Contains("Flock")) // add this newly found creep as a flock (if not other players flock)
            {
                nearbyTransforms.Add(c.transform);
                Destroy(creep);
                c.tag = gameObject.tag + "Flock";

                if(gameObject.tag == "Player1")
                {
                    c.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.red);
                }
                if (gameObject.tag == "Player2")
                {
                    c.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);
                }

                nearbyAgents.Add(c.gameObject.AddComponent<FlockAgent>());

            }
        }

        return new Tuple<List<Transform>, HashSet<FlockAgent>>(nearbyTransforms, nearbyAgents);
    }

    private float getDistanceToPlayer(FlockAgent agent)
    {
        return (agent.transform.position - player.transform.position).magnitude;
    }

    public GameObject getFurthest()
    {
        float distance;
        float greatestDistance = 0;
        FlockAgent furthest = null;
        foreach(FlockAgent agent in agents)
        {
            distance = getDistanceToPlayer(agent); 
            if(distance > greatestDistance)
            {
                furthest = agent;
                greatestDistance = distance;
            }
        }
        return furthest.gameObject;
    }


    public GameObject getFurthestInDirection(Vector3 direction)
    {
        // Get furthest point on max radius, select closest to the furthest point in direction
        float radius = (getFurthest().transform.position - player.transform.position).magnitude;
        Vector3 furtherstPointOnRadius = direction.normalized * radius + player.transform.position;
        //Debug.Log("radius " + radius);
        //Debug.Log("furthest point " + furtherstPointOnRadius);

        float distance;
        float smallestDistance = 0f;
        FlockAgent furthest = null;
        foreach(FlockAgent agent in agents)
        {
            if (!furthest)
            {
                furthest = agent;
                smallestDistance = (agent.transform.position - furtherstPointOnRadius).magnitude;
                continue;
            }
            distance = (agent.transform.position - furtherstPointOnRadius).magnitude;
            if(distance < smallestDistance)
            {
                furthest = agent;
                smallestDistance = distance;
            }
        }
        return furthest.gameObject;
    }
}
