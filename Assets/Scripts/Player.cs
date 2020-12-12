using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject playerObject;
    public GameObject target;
    [Range(0.0f, 5.0f)]
    public float speed = 2.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = target.transform.position - transform.position;
        transform.forward = direction;
        transform.position += direction.normalized * speed * Time.deltaTime;
    }

}
