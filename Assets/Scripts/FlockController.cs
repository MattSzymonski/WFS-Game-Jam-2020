using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    // TODO remove placeholder vars
    private float GizmoRadius = 0f;
    private Vector3 GizmoFurthestPoint = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        //behavior = ScriptableObject.CreateInstance("AlignmentBehavior") as IBehavior;
        player = gameObject.GetComponent<Player>();
        FindFlock();
    }

    // Update is called once per frame
    void Update()
    {
        HashSet<FlockAgent> difference = new HashSet<FlockAgent>();

        // Process player
        agents.UnionWith(GetNearby(player.gameObject).Item2);
        // Process each other member of the flock (agent)
        foreach (FlockAgent agent in agents)
        {
            HashSet<FlockAgent> agentsFound = ProcessNearby(agent);
            difference.UnionWith(agentsFound);
        }
        // Finally expand the set with the newly found agents
        agents.UnionWith(difference);
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
                nearbyTransforms.Add(c.transform);
                nearbyAgents.Add(collAgent);
            }
            else if (c.GetComponentInParent<Player>())
            {
                nearbyTransforms.Add(c.transform);
            }
            else if (creep) // add this newly found creep as a flock
            {
                nearbyTransforms.Add(c.transform);
                DestroyImmediate(creep);
                nearbyAgents.Add(c.gameObject.AddComponent<FlockAgent>());
            }
        }

        return new Tuple<List<Transform>, HashSet<FlockAgent>>(nearbyTransforms, nearbyAgents);
    }

    public GameObject getFurthest()
    {
        float distance;
        float greatestDistance = 0;
        FlockAgent furthest = null;
        foreach(FlockAgent agent in agents)
        {
            distance = (agent.transform.position - player.transform.position).magnitude;
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
        // Get a vector line from player in direction, get perpendicular vector from 
        // each one to the line, get the closest (on the side the vector is pointing to)
        float radius = (getFurthest().transform.position - player.transform.position).magnitude;
        Vector3 furtherstPointOnRadius = direction.normalized * radius + player.transform.position;
        //Debug.Log("radius " + radius);
        //Debug.Log("furthest point " + furtherstPointOnRadius);
        GizmoRadius = radius;
        GizmoFurthestPoint = furtherstPointOnRadius;

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
    private void OnDrawGizmos()
    {
        if (player)
        {
            Gizmos.DrawWireSphere(player.transform.position, GizmoRadius);
            Gizmos.DrawLine(player.transform.position, GizmoFurthestPoint);
        }
    }
}
