using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Flock/Behavior/Composite")]
public class CompositeBehavior : IBehavior 
{
    public IBehavior[] behaviors;
    public float[] weights; 
    
    public override Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller)
    {
        Vector3 move = Vector3.zero;
        if (behaviors.Length != weights.Length)
        {
            Debug.LogError("Mismatched weight length with behavior number!");
            return Vector3.zero;
        }

        // calculate move for each behavior and assign a weight
        for (int i = 0; i < behaviors.Length; ++i)
        {
            Vector3 componentMove = behaviors[i].CalculateMovement(agent, nearby, controller) * weights[i];

            if (componentMove == Vector3.zero)
                continue;

            //Debug.Log(behaviors[i]);
            //Debug.Log(componentMove.sqrMagnitude);
            if (componentMove.sqrMagnitude > weights[i]*weights[i])
            {
                componentMove = componentMove.normalized * weights[i];
            }
            move += componentMove;
        }
        return move;
    }
}
