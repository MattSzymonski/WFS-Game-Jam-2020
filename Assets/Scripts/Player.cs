using MightyGamePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Player : MonoBehaviour
{

    public MainGameManager mgm;

    Rigidbody rb;
    FlockController flock;
    [Header("Movement")]
    public bool useMouseAndKeyboardInput = false; // TODO: keyboard movement might even be not possible :(?
    public bool useGamePadInput = true;
    [ShowIf("useGamePadInput")] public int controllerNumber = 0;

    [Range(0.0f, 100.0f)]
    public float movementSpeed = 20.0f;

    Vector3 lookDirection;
    Vector3 previousLookDirection;

    [Range(0, .3f)]
    public float movementSmoothing = 0.05f;

    public float flockPenalty = 0.05f; // TODO: apply this penalty to flock rotation as well?
    private float currentFlockPenalty;

    Ray camRay;
    RaycastHit camHit;
    public string mouseTargetLayerName;
    int mouseTargetLayer;
    public bool mouseRayGizmo;

    [Header("Skills")]
    public float singleProjectileSpeed = 7.0f;
    public float somethinglol = 1.0f;
    private int lastTriggerPress = 0; // -1 is Left Trigger, 1 is Right, 0 is None
    // Start is called before the first frame update

    //gizmo stuff
    private Vector3 furthestFlockGizmo;


    public ParticleSystem particles;
    private bool readyToDie = false;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        flock = GetComponent<FlockController>();

    }

    // Update is called once per frame
    void Update()
    {
        AdjustSpeed();
        ChangeDirection();
        Move();
        Rotation(); // this will be used for finding a direction for the CYBERPUNK MIND SWITCH LOL
        Skills();

        if (readyToDie)
            Die();
    }

    private void AdjustSpeed()
    {
        currentFlockPenalty = 1.0f;
        // flock set always contains a player so subtract 1
        // simple linear penalty to movement for now
        if (flock.agents.Count != 0)
        {
            currentFlockPenalty -= (flock.agents.Count - 1) * flockPenalty;
            currentFlockPenalty = Mathf.Max(currentFlockPenalty, 0.5f);
        }
    }

    private void Move()
    {
        DebugExtension.DebugArrow(transform.position, new Vector3(0, 0, 1));
        if (lookDirection == Vector3.zero) // if player does not move, continue with old look direction
        {
            lookDirection = previousLookDirection;
            previousLookDirection = lookDirection;
        }
        else
        {
            previousLookDirection = lookDirection;
        }
        // adjust movement speed based on flock weight??
        float yVel = rb.velocity.y;
        rb.velocity = new Vector3(lookDirection.x * movementSpeed * currentFlockPenalty, yVel, lookDirection.z * movementSpeed * currentFlockPenalty); // will add drag
    }

    private void ChangeDirection()
    {
         if (useMouseAndKeyboardInput)
         {
             camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
             if (Physics.Raycast(camRay, out camHit, 5000, mouseTargetLayer))
             {
                 Vector3 relativeDirection = camHit.point - transform.position;
                 Vector3 moveDirection = MightyUtilites.ClearY(relativeDirection).normalized;

                 if (Vector3.Distance(MightyUtilites.ClearY(transform.position), MightyUtilites.ClearY(camHit.point)) > 0.3f)
                 {
                     Vector3 newRotation = Vector3.RotateTowards(transform.forward, moveDirection, 15.0f * Time.deltaTime, 0.0f).normalized;
                     transform.rotation = Quaternion.LookRotation(newRotation, Vector3.up);
                     lookDirection = newRotation;
                 }
             }
         }
       
        if (useGamePadInput)
        {
            lookDirection = new Vector3(Input.GetAxis("Controller" + (controllerNumber + 1) + " Right Stick Horizontal"), 0, -Input.GetAxis("Controller" + (controllerNumber + 1) + " Right Stick Vertical")).normalized;
            if (lookDirection == Vector3.zero)
            {
                if(previousLookDirection == Vector3.zero) //for fixing Zero roation quat
                {
                    transform.rotation = Quaternion.identity;
                }
                else
                {
                    transform.rotation = Quaternion.LookRotation(previousLookDirection, Vector3.up);
                }
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                previousLookDirection = lookDirection;
            }
   
            DebugExtension.DebugArrow(transform.position, lookDirection * 10, Color.yellow);
        }
    }

    private void Rotation()
    {
        if (useGamePadInput)
        {
            //Debug.Log("Horizontal " + Input.GetAxis("Controller" + controllerNumber + " Left Stick Horizontal"));
            //Debug.Log("Vertical " + Input.GetAxis("Controller" + controllerNumber + " Left Stick Vertical"));
            Vector3 cyberShiftDir = new Vector3(Input.GetAxis("Controller" + (controllerNumber + 1) + " Left Stick Horizontal"), 0, -Input.GetAxis("Controller" + (controllerNumber + 1) + " Left Stick Vertical")).normalized;
            DebugExtension.DebugArrow(transform.position, cyberShiftDir * 10, Color.blue);
            //Debug.Log(cyberShiftDir);
            if(Input.GetAxis("Controller" + (controllerNumber + 1) + " Triggers") == -1) // Left trigger pressed
            {
                // Only go further if Left button isn't held down
                if (lastTriggerPress == -1)
                    return;
                lastTriggerPress = -1;
                if (flock.agents.Count == 0)
                    return;
                GameObject furthestInDir = flock.getFurthestInDirection(cyberShiftDir);
                furthestFlockGizmo = furthestInDir.transform.position;
                // Switcharoo
                Vector3 currentPos = transform.position;
                transform.position = furthestInDir.transform.position;
                furthestInDir.transform.position = currentPos;

                ShootAllCars();
            }
        }
    }

    private void Skills()
    {
        if (useGamePadInput)
        {
            
            // If right trigger, Fire single car
            Debug.Log(Input.GetAxis("Controller" + (controllerNumber+1) + " Triggers"));
            if(Input.GetAxis("Controller" + (controllerNumber + 1) + " Triggers") == 1) // Right trigger pressed
            {
                lastTriggerPress = 1;
                if(!IsInvoking("ShootOneCar"))
                    InvokeRepeating("ShootOneCar", 0.0f, 0.5f);
            }
            if (Input.GetAxis("Controller" + (controllerNumber + 1) + " Triggers") == 0)
            {
                lastTriggerPress = 0;
                if(IsInvoking("ShootOneCar"))
                    CancelInvoke();
            }
        }
    }

    private void ShootAllCars()
    {
        foreach (FlockAgent agent in flock.agents)
        {
            GameObject agentGO = agent.gameObject;
            //furthestFlockGizmo = furthest.transform.position;
            Projectile projectile = agentGO.AddComponent<Projectile>();
            projectile.velocity = agentGO.transform.forward * singleProjectileSpeed;
            DestroyImmediate(agent);
        }
        flock.agents.Clear();
    }
    private void ShootOneCar()
    {
        Vector3 cyberShiftDir = new Vector3(Input.GetAxis("Controller" + (controllerNumber + 1) + " Left Stick Horizontal"), 0, -Input.GetAxis("Controller" + (controllerNumber + 1) + " Left Stick Vertical")).normalized;
        // if no members in flock, skipp
        if (flock.agents.Count == 0)
            return;
        GameObject furthest = flock.getFurthestInDirection(cyberShiftDir);
        FlockAgent furthestFlockAgent = furthest.GetComponent<FlockAgent>();
        //furthestFlockGizmo = furthest.transform.position;
        Projectile projectile = furthest.AddComponent<Projectile>();
        projectile.velocity = furthest.transform.forward * singleProjectileSpeed;
        flock.agents.Remove(furthestFlockAgent);
        DestroyImmediate(furthestFlockAgent);
    }


    private void OnDrawGizmos()
    { 
        Gizmos.DrawSphere(furthestFlockGizmo, 1f);
        if (mouseRayGizmo)
        {
            if(camHit.point != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(Camera.main.transform.localPosition, 0.5f);
                Gizmos.DrawLine(Camera.main.transform.position, camHit.point);
                Gizmos.DrawSphere(camHit.point, 0.5f);
            } 
        }

        //Gizmos.color = Color.green;
        //if (!shoutArea) return;
        //Gizmos.matrix = Matrix4x4.TRS(shoutArea.transform.position, transform.rotation, Vector3.one);
        //Gizmos.DrawWireCube(Vector3.zero, transform.localScale * 8);
    }




    private void OnTriggerEnter(Collider other) // can add more sophisticated collisions?
    {
        if (gameObject.tag == "Player1")
        {
            if (other.gameObject.tag == "Player2")
            {
                mgm.audioManager.PlaySound("PlayerCrash1");
                mgm.GameOver(3);
               
                Debug.Log("Split");


                Camera.main.transform.parent.GetComponent<MightyGamePack.CameraShaker>().ShakeOnce(3.0f, 1f, 1f, 1.25f);
                Die();
                return;
            }

            if (other.gameObject.tag == "Player2Flock")
            {
                mgm.audioManager.PlaySound("PlayerCrash1");
              
                Camera.main.transform.parent.GetComponent<MightyGamePack.CameraShaker>().ShakeOnce(3.0f, 1f, 1f, 1.25f);
                mgm.GameOver(2);
                Die();
                return;
            }
        }

        if (gameObject.tag == "Player2")
        {
            if (other.gameObject.tag == "Player2")
            {
                Camera.main.transform.parent.GetComponent<MightyGamePack.CameraShaker>().ShakeOnce(3.0f, 1f, 1f, 1.25f);

                mgm.audioManager.PlaySound("PlayerCrash1");
               
                mgm.GameOver(3);
                Die();
                Debug.Log("Split");
                return;
            }

            if (other.gameObject.tag == "Player1Flock")
            {
                Camera.main.transform.parent.GetComponent<MightyGamePack.CameraShaker>().ShakeOnce(3.0f, 1f, 1f, 1.25f);
                mgm.audioManager.PlaySound("PlayerCrash1");
               
                mgm.GameOver(1);
                Die();
                return;
            }
        }

     
        //if (gameObject.tag == "Player1Flock")
        //{
        //    if (gameObject.tag == "Player2Flock")
        //    {

        //    }
        //}


        //string tag = gameObject.tag;
        //string otherTag = other.gameObject.tag;
        //if (tag != otherTag) // if the tags differ, we have a collision
        //{
        //    //Debug.LogError("Tag: " + tag + " Other: " + otherTag);
        //    // if they belong to the same tag group -> player/playerFlock ignore
        //    if (tag.Length > otherTag.Length)
        //        if (tag.StartsWith(otherTag))
        //            return;
        //        else if (tag.Length < otherTag.Length)
        //            if (otherTag.StartsWith(tag))
        //                return;

        //    if (readyToDie)
        //        return;
        //    // for now don't care about creeps being reduced
        //    //if (tag == "Creep")
        //    bool isFlock = tag.Contains("Flock");
        //    bool isCreep = tag.Contains("Creep");
        //    if (isFlock || isCreep)
        //    {
        //        //Debug.LogError("EXPLOSION " + gameObject.ToString() + " other " + other.gameObject.ToString());
        //        // spawn particles
        //        GameObject particles = Instantiate(gameObject, new Vector3(transform.position.x, 0.0f, transform.position.y), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f)) as GameObject;
        //        particles.transform.parent = GameObject.Find("ExplosionParticles").transform;

        //        readyToDie = true;

        //        Player p = other.GetComponent<Player>();

        //        if (p != null)
        //        {
        //            p.readyToDie = true;
        //        }  

        //        other.GetComponent<Player>().readyToDie = true;
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
