using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement_Network : MonoBehaviourPun
{
    public float speed = 10;

    void Start()
    {
        
    }

    public void Update()
    {
        if (base.photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.A))
            {
                transform.position = transform.position + new Vector3(-1, 0, 0) * Time.deltaTime * speed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.position = transform.position + new Vector3(1, 0, 0) * Time.deltaTime * speed;
            }
        }

    }
}
