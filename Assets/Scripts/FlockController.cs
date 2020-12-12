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
        foreach (FlockAgent agent in agents)
        {
            Tuple<List<Transform>, HashSet<FlockAgent>> nearbyTransformsAgents = GetNearby(agent);
            difference.UnionWith(nearbyTransformsAgents.Item2);

           // Vector3 move = behavior.CalculateMovement(agent, nearbyTransformsAgents.Item1, this);

        }
        agents.UnionWith(difference);
        Debug.LogError(agents.Count);
    }

    void FindFlock()
    {
        GameObject[] flock = GameObject.FindGameObjectsWithTag("Flock");
        foreach (var o in flock)
            agents.Add(o.GetComponent<FlockAgent>());
    }

    Tuple<List<Transform>, HashSet<FlockAgent>> GetNearby(FlockAgent agent)
    {
        List<Transform> nearbyTransforms = new List<Transform>();
        HashSet<FlockAgent> nearbyAgents = new HashSet<FlockAgent>();

        Collider[] nearbyColliders = Physics.OverlapSphere(agent.transform.position, radius);
        Debug.LogError(nearbyColliders.Length);

        foreach (var c in nearbyColliders)
        {
            // TODO: potential problem, when del;eting objects from this flock, they will be readded here as it runs on update
            // add tags discarded etc
            // get tags only current player and neutral not discarded
            nearbyAgents.Add(c.GetComponentInParent<FlockAgent>());
            nearbyTransforms.Add(c.transform);
        }

        return new Tuple<List<Transform>, HashSet<FlockAgent>>(nearbyTransforms, nearbyAgents);
    }
}
