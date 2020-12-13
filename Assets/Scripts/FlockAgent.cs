using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockAgent : MonoBehaviour
{
    [Header ("Slowing down/speeding up")]
    public float distanceFromPlayerSlowDown = 3f;
    [Range(0.0f, 1.0f)]
    public float slowDownRatio =0.3f;

    public float distanceFromPlayerSpeedUp = 8f;
    [Range(0.0f, 1.0f)]
    public float speedUpRatio = 0.4f; 
    private float speedModTime = 1f;
    private bool isSpedUp = false;
    private bool isSlowedDown = false;
    private float currTime = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isSpedUp || isSlowedDown)
        {
            currTime += Time.deltaTime;
            if (currTime >= speedModTime)
            {
                currTime = 0f;
                isSpedUp = false;
                isSlowedDown = false;
            }
        }
        
    }

    public void Move(Vector3 velocity)
    {
        if (!isSlowedDown && !isSpedUp)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (rand > 0.8f)
            {
                isSpedUp = true;
            }
            else if (rand > 0.6f)
            {
                isSlowedDown = true;
            }
        }
        if (isSlowedDown)
        {
            float velocityMag = velocity.magnitude;
            velocity = velocity.normalized;
            velocity *= (velocityMag * slowDownRatio);
        }
        else if (isSpedUp)
        {
            float velocityMag = velocity.magnitude;
            velocity = velocity.normalized;
            velocity *= (velocityMag * (1 + speedUpRatio));
        }
        float randomRotSpeed = UnityEngine.Random.Range(2f, 4f);
        Debug.Log(Time.time);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velocity, Vector3.up),  Time.deltaTime*randomRotSpeed);
        transform.position += velocity * Time.deltaTime;
    }

}
