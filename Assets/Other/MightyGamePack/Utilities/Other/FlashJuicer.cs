/* 
NAME:
    Flash Juicer

DESCRIPTION:
    Warning: Animated parameters should NOT be changed in playing mode by other scripts or components

USAGE:
    Look under the code below 

TODO:
   
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace MightyGamePack
{
    [ExecuteInEditMode]
    public class FlashJuicer : MonoBehaviour
    {
        public Material flashMaterial;
        public float flashDuration;

        Material startMaterial;
        Renderer rend;
        bool active;
        float flashTimer;

        [Header("Collisions")]
        public bool detectCollisions;
        [ShowIf("detectCollisions")] [Tooltip("Object will detect collisions with objects with these tags")] public string[] tagsToCollide;

        void Start()
        {
            rend = GetComponent<Renderer>();
            startMaterial = rend.sharedMaterial;
        }

        void Update()
        {
            Evaluate();
        }

        public void StartJuicing()
        {
            flashTimer = 0;
            if (!active)
            {
                rend.sharedMaterial = flashMaterial;
            }
            active = true;
        }

        void Evaluate()
        {
            if (active)
            {
                if (flashTimer < flashDuration)
                {
                    flashTimer += Time.deltaTime;
                }
                else
                {
                    rend.sharedMaterial = startMaterial;
                    active = false;            
                }
            }
        }

        public void StopJuicing()
        {
            active = false;
            rend.sharedMaterial = startMaterial;
        }


        public void OnCollisionEnter(Collision collision)
        {
            if (detectCollisions)
            {
                for (int i = 0; i < tagsToCollide.Length; i++)
                {
                    if (collision.transform.tag == tagsToCollide[i])
                    {
                        StartJuicing();
                        break;
                    }
                }
            }
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (detectCollisions)
            {
                for (int i = 0; i < tagsToCollide.Length; i++)
                {
                    if (collision.transform.tag == tagsToCollide[i])
                    {
                        StartJuicing();
                        break;
                    }
                }
            }
        }
    }
}

/*
USAGE:

    Add this as component to the object you want to animate
    GetComponent<FlashJuicer>().StartJuicing();
    GetComponent<FlashJuicer>().StopJuicing();
    
*/
