using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Flock/Behavior/Cohesion")]
public class CohesionBehavior : IBehavior // TODO: add formations
{
    public override Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller)
    {
        // if no neighbours, follow Christ
        if (nearby.Count == 0)
        {
            //return Vector3.zero;
            return controller.player.transform.forward;
        }

        Vector3 cohesionMove = Vector3.zero;
        foreach (Transform neighbor in nearby)
        {
            cohesionMove += (Vector3)neighbor.position;
        }
        cohesionMove /= nearby.Count;

        // create offset from agent pos
        cohesionMove -= (Vector3)agent.transform.position;

        return cohesionMove;
    }
}
