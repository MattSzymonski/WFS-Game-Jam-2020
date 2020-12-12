using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IBehavior : ScriptableObject
{
    public abstract Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller);
}
