using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creep : MonoBehaviour
{
    // This is non-flocking creep (neutral behavior)

    public float speed = 2.0f;
    public float radialSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Most of the time move forward
        Vector3 newVector = transform.forward;
        // sometimes turn right or left
        int rand = Random.Range(0, 10);
        if (rand > 8)
        {
            // turn left
            transform.rotation *= Quaternion.Euler(Vector3.up * radialSpeed);
        } else if (rand > 6)
        {
            transform.rotation *= Quaternion.Euler(Vector3.up * radialSpeed);
        }
        transform.position += newVector * Time.deltaTime * speed;
    }
}
