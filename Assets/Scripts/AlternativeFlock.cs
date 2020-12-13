using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternativeFlock : MonoBehaviour
{
    public float speed = 15.0f;
    public float rotationSpeed = 15.0f;
    public float neighborDistance = 10.0f;

    Vector3 averageHeading;
    Vector3 averagePosition;

    GlobalFlocks globalFlocks;

    // Start is called before the first frame update
    void Start()
    {
        globalFlocks = GameObject.FindObjectOfType<GlobalFlocks>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0, 5) < 1) // add some randomization
            ApplyFlockRules();

        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    private void ApplyFlockRules()
    {
        HashSet<GameObject> flocks = globalFlocks.GetFlock(gameObject.tag);

        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float gSpeed = 0.1f;

        Vector3 goalPos = globalFlocks.GetPlayerTransform(gameObject.tag.Replace("Flock", "")).position; // tODO: get the same tag as player? Maybe add some regex for distinguishing

        float dist;

        int groupSize = 0;

        //Debug.Log("flock size  " + flocks.Count);
        foreach (var f in flocks)
        {
            if (f == this.gameObject)
                continue;

            dist = Vector3.Distance(f.transform.position, this.transform.position);
            if (dist <= neighborDistance)
            {
                vcentre += f.transform.position;
                ++groupSize;

                if (dist < 1.0f)
                    vavoid += (this.transform.position - f.transform.position);

                AlternativeFlock anotherFlock = f.GetComponent<AlternativeFlock>();
                gSpeed += anotherFlock.speed;
            }
        }

        if (groupSize > 0)
        {
            vcentre = vcentre / groupSize + (goalPos - this.transform.position);
            speed = gSpeed / groupSize;

            Vector3 direction = (vcentre + vavoid) - transform.position;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        }
    }
}
