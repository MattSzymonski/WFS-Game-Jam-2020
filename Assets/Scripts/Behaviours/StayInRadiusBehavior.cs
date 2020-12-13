using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Flock/Behavior/Stay In Radius")]
public class StayInRadiusBehavior : IBehavior
{
    Vector3 center;
    public float radius = 15f;
    public override Vector3 CalculateMovement(FlockAgent agent, List<Transform> nearby, FlockController controller)
    {
        center = controller.transform.position;
        Vector3 centerOffset = center - agent.transform.position;

        float t = centerOffset.magnitude / radius;

        if (t < 0.9f)
        {
            return Vector3.zero;
        }

        return centerOffset * t * t;
    }

}
