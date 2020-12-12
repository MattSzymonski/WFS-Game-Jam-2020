using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeBehaviour : IBehavior 
{
    public IBehavior[] behaviors;
    public override Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller)
    {
        throw new System.NotImplementedException();
    }
}
