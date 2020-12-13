using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockAgent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 velocity)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velocity, Vector3.up),  Time.deltaTime*5f);
        transform.position += velocity * Time.deltaTime;
    }

}
