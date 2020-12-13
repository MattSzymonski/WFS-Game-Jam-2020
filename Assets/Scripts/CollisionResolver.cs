using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionResolver : MonoBehaviour
{
    public ParticleSystem particles;
    private bool readyToDie = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToDie)
            Die();
    }

    private void OnTriggerEnter(Collider other) // can add more sophisticated collisions?
    {
        string tag = gameObject.tag;
        string otherTag = other.gameObject.tag;
        if (tag != otherTag) // if the tags differ, we have a collision
        {
            //Debug.LogError("Tag: " + tag + " Other: " + otherTag);
            // if they belong to the same tag group -> player/playerFlock ignore
            if (tag.Length > otherTag.Length)
                if (tag.StartsWith(otherTag))
                    return;
            else if (tag.Length < otherTag.Length)
                if (otherTag.StartsWith(tag))
                    return;

            if (readyToDie)
                return;
            // for now don't care about creeps being reduced
            //if (tag == "Creep")
            bool isFlock = tag.Contains("Flock");
            bool isCreep = tag.Contains("Creep");
            if (isFlock || isCreep)
            {
                //Debug.LogError("EXPLOSION " + gameObject.ToString() + " other " + other.gameObject.ToString());
                // spawn particles
                GameObject particles = Instantiate(gameObject, new Vector3(transform.position.x, 0.0f, transform.position.y), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)) as GameObject;
                particles.transform.parent = GameObject.Find("ExplosionParticles").transform;

                readyToDie = true;
                other.GetComponent<CollisionResolver>().readyToDie = true;
            }
        }
    }

    private void Die()
    {
        var flock = GetComponent<FlockAgent>(); // invalidate it
        if (flock)
            flock.isValid = false;
        // remove the object if belongs to the player
        GameObject.Destroy(gameObject, 0.2f);
    }
}
