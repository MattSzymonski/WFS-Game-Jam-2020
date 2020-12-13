using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    public Player player;

    public float radius = 5.0f;

    private GlobalFlocks globalFlockContainer;
    private HashSet<GameObject> agents = new HashSet<GameObject>();
    private string thisTag;

    // Start is called before the first frame update
    void Start()
    {
        //behavior = ScriptableObject.CreateInstance("AlignmentBehavior") as IBehavior;
        player = gameObject.GetComponent<Player>();
        globalFlockContainer = GameObject.FindObjectOfType<GlobalFlocks>();
        thisTag = "Player1Flock";
        FindFlock();
        globalFlockContainer.AddFlock(thisTag, agents);
        globalFlockContainer.AddPlayerTransform(thisTag.Replace("Flock", ""), player.transform);
    }

    // Update is called once per frame
    void Update()
    {
        HashSet<GameObject> difference = new HashSet<GameObject>();

        // Process player //TODO: with Celeb fix no more in the list, stored explicitly in the global object hashmaps
        agents.UnionWith(GetNearby(player.gameObject).Item2);
        // Process each other member of the flock (agent)
        foreach (GameObject agent in agents)
        {
            HashSet<GameObject> agentsFound = ProcessNearby(agent);
            difference.UnionWith(agentsFound);
        }
        // Finally expand the set with the newly found agents
        agents.UnionWith(difference);
        globalFlockContainer.UpdateFlock(thisTag, agents); // TODO: change if using different tags for player and their flock
        globalFlockContainer.UpdatePlayerTransform(thisTag.Replace("Flock", ""), player.transform);
    }

    HashSet<GameObject> ProcessNearby(GameObject source)
    {
        Tuple<List<Transform>, HashSet<GameObject>> nearbyTransformsAgents = GetNearby(source);

        //Vector3 move = behavior.CalculateMovement(source, nearbyTransformsAgents.Item1, this);

        ////Vector3 move = player.transform.position - source.transform.position;
        ////move = move.normalized * agentSpeed;
        //move *= driveFactor;
        //if (move.sqrMagnitude > agentSpeed*agentSpeed)
        //{
        //    move = move.normalized;
        //    move *= agentSpeed;
        //}
        //source.Move(move);
        return nearbyTransformsAgents.Item2;
    }

    void FindFlock()
    {
        // TODO: add only to this players tag
        GameObject[] flock = GameObject.FindGameObjectsWithTag(thisTag);
        foreach (var o in flock)
            agents.Add(o);
    }

    Tuple<List<Transform>, HashSet<GameObject>> GetNearby(GameObject agent)
    {
        List<Transform> nearbyTransforms = new List<Transform>();
        HashSet<GameObject> nearbyAgents = new HashSet<GameObject>();

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
            var collAgent = c.GetComponentInParent<AlternativeFlock>();
            var creep = c.GetComponentInParent<Creep>();
            if (collAgent)
            {
                nearbyTransforms.Add(c.transform);
                nearbyAgents.Add(collAgent.gameObject);
            }
            //else if (c.GetComponentInParent<Player>())
            //{
            //    nearbyTransforms.Add(c.transform);
            //}
            else if (creep) // add this newly found creep as a flock
            {
                nearbyTransforms.Add(c.transform);
                DestroyImmediate(creep);
                c.tag = thisTag;
                nearbyAgents.Add(c.gameObject.AddComponent<AlternativeFlock>().gameObject);
            }
        }

        return new Tuple<List<Transform>, HashSet<GameObject>>(nearbyTransforms, nearbyAgents);
    }
}
