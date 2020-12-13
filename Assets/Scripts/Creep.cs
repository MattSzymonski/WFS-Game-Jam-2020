using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creep : MonoBehaviour
{
    // This is non-flocking creep (neutral behavior)
    public MainGameManager mgm;
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



    private void OnTriggerEnter(Collider other) // can add more sophisticated collisions?
    {
        if (gameObject.tag == "Player1Flock")
        {
            if (other.gameObject.tag == "Player2Flock")
            {
                other.gameObject.GetComponent<Creep>().Die();
                this.Die();
            }
        }

        //if (gameObject.tag == "Player2Flock")
        //{
        //    if (other.gameObject.tag == "Player1Flock")
        //    {
        //        other.gameObject.GetComponent<Creep>().Die();
        //        this.Die();
        //    }
        //}
    }

    private void Die()
    {
        var flock = GetComponent<FlockAgent>(); // invalidate it
        if (flock)
            flock.isValid = false;

        mgm.audioManager.PlayRandomSound("Crash1", "Crash2", "Crash3", "Crash4");
        // remove the object if belongs to the player
        GameObject.Destroy(gameObject, 0.2f);


    }


}
