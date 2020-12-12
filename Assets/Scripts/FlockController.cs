using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    public IBehavior behavior;
    public HashSet<FlockAgent> agents = new HashSet<FlockAgent>();

    Player player;

    public float radius = 5.0f;
    public float agentSpeed = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.GetComponent<Player>();
        FindFlock();
    }

    // Update is called once per frame
    void Update()
    {
        HashSet<FlockAgent> difference = new HashSet<FlockAgent>();
        Debug.LogError(agents);

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

        //Vector3 move = behavior.CalculateMovement(source, nearbyTransformsAgents.Item1, this);

        Vector3 move = player.transform.position - source.transform.position;
        move = move.normalized * agentSpeed;
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
        foreach (var x in nearbyColliders) Debug.Log(x.ToString());

        foreach (var c in nearbyColliders)
        {
            // TODO: potential problem, when del;eting objects from this flock, they will be readded here as it runs on update
            // add tags discarded etc
            // get tags only current player and neutral not discarded


            // Only add to list if overlaps with agent
            var collAgent = c.GetComponentInParent<FlockAgent>();
            if (collAgent)
            {
                nearbyTransforms.Add(c.transform);
                nearbyAgents.Add(collAgent);
            }
        }

        return new Tuple<List<Transform>, HashSet<FlockAgent>>(nearbyTransforms, nearbyAgents);
    }
}
