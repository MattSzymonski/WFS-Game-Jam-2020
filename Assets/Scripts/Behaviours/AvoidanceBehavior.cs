using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Flock/Behavior/Avoidance")]
public class AvoidanceBehavior : IBehavior
{
    public override Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller)
    {
        // if no neighbours, follow Christ
        if (nearby.Count == 0)
        {
            //return controller.player.transform.forward;
            return Vector3.zero;
        }

        Vector3 avoidanceMove = Vector3.forward;

        int numAvoid = 0;
        float SquareAvoidanceRadius = controller.radius * controller.avoidanceRadiusMultiplier * controller.avoidanceRadiusMultiplier;
        foreach (Transform neighbor in nearby)
        {
            if (Vector3.SqrMagnitude(neighbor.position - agent.transform.position) < SquareAvoidanceRadius)
            {
                ++numAvoid;
                avoidanceMove += (Vector3) (agent.transform.position - neighbor.position);
            }
        }

        if (numAvoid > 0)
            avoidanceMove /= numAvoid;
        return avoidanceMove;
    }
}
