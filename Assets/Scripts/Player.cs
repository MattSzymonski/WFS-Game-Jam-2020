using MightyGamePack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Player : MonoBehaviour
{
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
    public float somethinglol = 1.0f;
    // Start is called before the first frame update
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
        //Rotation(); // this will be used for finding a direction for the CYBERPUNK MIND SWITCH LOL
        Skills();
    }

    private void AdjustSpeed() // TODO: turned off
    {
        currentFlockPenalty = 1.0f;
        //// flock set always contains a player so subtract 1
        //// simple linear penalty to movement for now
        //if (flock.agents.Count != 0)
        //{
        //    currentFlockPenalty -= (flock.agents.Count - 1) * flockPenalty;
        //    currentFlockPenalty = Mathf.Max(currentFlockPenalty, 0.5f);
        //}
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
            lookDirection = new Vector3(Input.GetAxis("Controller" + controllerNumber + " Right Stick Horizontal"), 0, -Input.GetAxis("Controller" + controllerNumber + " Right Stick Vertical")).normalized;
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
    }

    private void Skills()
    {

    }

    private void OnDrawGizmos()
    { 
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
}
