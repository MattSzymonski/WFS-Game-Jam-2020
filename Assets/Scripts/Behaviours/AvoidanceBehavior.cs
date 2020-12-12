using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidanceBehavior : IBehavior
{
    public override Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller)
    {
        // if no neighbours, follow Christ
        if (nearby.Count == 0)
        {
            return controller.player.transform.forward;
        }

        Vector3 avoidanceMove = Vector3.forward;
    }
}
