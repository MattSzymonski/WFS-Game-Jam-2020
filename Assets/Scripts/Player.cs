using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject playerObject;
    public GameObject[] flock;
    // Start is called before the first frame update
    void Start()
    {
        FindFlock();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FindFlock()
    {
        flock = GameObject.FindGameObjectsWithTag("Flock");
    }
}
