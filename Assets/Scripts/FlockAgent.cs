using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MightyGamePack;


public class FlockAgent : MonoBehaviour
{
    public bool isValid = true;
    [Header ("Slowing down/speeding up")]
    public float distanceFromPlayerSlowDown = 3f;
    [Range(0.0f, 1.0f)]
    public float slowDownRatio =0.3f;

    public float distanceFromPlayerSpeedUp = 8f;
    [Range(0.0f, 1.0f)]
    public float speedUpRatio = 0.4f; 
    private float speedModTime = 0.5f;
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
        if(MainGameManager.mainGameManager.gameState == GameState.Playing)
        {
            if (isSpedUp || isSlowedDown)
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

    }

    public void Move(Vector3 velocity)
    {
        if (!isSlowedDown && !isSpedUp)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (rand > 0.9f)
            {
                isSpedUp = true;
            }
            else if (rand > 0.85f)
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
        float randomRotSpeed = UnityEngine.Random.Range(2f, 5f);
        //Debug.Log(Time.time);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velocity, Vector3.up),  Time.deltaTime*randomRotSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) // can add more sophisticated collisions?
    {
        if (gameObject.tag == "Player1Flock")
        {
            if (other.gameObject.tag == "Player2Flock")
            {
                Camera.main.transform.parent.GetComponent<MightyGamePack.CameraShaker>().ShakeOnce(0.5f, 1f, 1f, 1.25f);
                other.gameObject.GetComponent<FlockAgent>().Die();
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
        // remove the object if belongs to the player
        GameObject.Destroy(gameObject, 0.2f);


    }
}
