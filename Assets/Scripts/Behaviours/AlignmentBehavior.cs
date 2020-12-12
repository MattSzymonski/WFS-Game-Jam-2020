using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Flock/Behavior/Alignment")]
public class AlignmentBehavior : IBehavior
{
    public override Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller)
    {
        if (nearby.Count == 0)
        {
            Debug.Log("No nearby !");
            // If no neighbours, maintain current direction
            // TODO possible bug, might drive off, maybe align to player
            //return controller.player.transform.forward;
            return agent.transform.forward;
        }
        if (nearby.Contains(controller.player.transform))
        {
            // Possible improvement, instead of returning player vector, add greater weight to it to smooth it out
            return controller.player.transform.forward;
        }
        // TODO add filtering - only adjust to current flock
        Vector3 alignmentMove = Vector3.zero;
        foreach (Transform neighbor in nearby)
        {
            alignmentMove += neighbor.transform.forward;
        }
        alignmentMove /= nearby.Count;

        return alignmentMove;
    }
}
