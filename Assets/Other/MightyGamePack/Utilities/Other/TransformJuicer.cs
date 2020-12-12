/* 
NAME:
    Transform Juicer

DESCRIPTION:
    Script that animates transform of object using curves
    A substitute of creating animator and animations
    Warning: No two jucers can animate same parameter in the same time, there is no blending.
    Warning: Starting evaluating juicer while currently active evaluation is not finished will lead to hard animation cut, there is no blending
    Warning: AnimatedElement variable should not be changed in playing mode

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
    public enum AnimatedElement
    {
        Position,
        Rotation,
        Scale
    };

    public class TransformJuicer : MonoBehaviour
    {
        public AnimatedElement animatedElement;
        [Tooltip("Curve that describes animated parameter value over time")] public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("Animation curve value will be multiplied by this value (Eg. (0,1,0) will animate only in Y axis")] public Vector3 animationDirectionMultiplier;
        public float evaluationSpeed;
        public bool looping;
        [Tooltip("Jucer will not play if called while already active")] public bool blockIfActive;
        [ShowIf("blockIfActive")] [Tooltip("Each call juicer will update start values of position, rotation and scale")] public bool updateStartValues;

        bool active;
        float evaluateState;
        float curveValue;

        [Header("Collisions")]
        public bool detectCollisions;
        [ShowIf("detectCollisions")] [Tooltip("Object will detect collisions with objects with these tags")] public string[] tagsToCollide;

        Vector3 startPosition;
        Vector3 startRotation;
        Vector3 startScale;

        void Start()
        {
            SetStartValues();
        }

        void Update()
        {
            Evaluate();
        }

        void SetStartValues()
        {
            if (animatedElement == AnimatedElement.Position)
            {
                startPosition = transform.localPosition;
            }

            if (animatedElement == AnimatedElement.Rotation)
            {
                startRotation = transform.localEulerAngles;
            }

            if (animatedElement == AnimatedElement.Scale)
            {
                startScale = transform.localScale;
            }
        }

        void Evaluate()
        {
            if (active)
            {
                if (evaluateState < 1)
                {
                    evaluateState += evaluationSpeed * Time.deltaTime;
                    curveValue = animationCurve.Evaluate(evaluateState);
                }
                else
                {
                    if (looping)
                    {
                        evaluateState = 0;
                    }
                    else
                    {
                        active = false;
                    }
                    curveValue = animationCurve.Evaluate(evaluateState);
                }

                switch (animatedElement)
                {
                    case AnimatedElement.Position:
                        transform.localPosition = startPosition + animationDirectionMultiplier * curveValue;
                        break;
                    case AnimatedElement.Rotation:
                        transform.eulerAngles = startRotation + animationDirectionMultiplier * curveValue;
                        break;
                    case AnimatedElement.Scale:
                        transform.localScale = startScale + animationDirectionMultiplier * curveValue;
                        break;
                }
            }
        }

        public void StartJuicing()
        {
            if (active && blockIfActive)
            {
                return;
            }
            if (blockIfActive && updateStartValues)
            {
                SetStartValues();
            }
            active = true;
            evaluateState = 0;
        }

        public void StopJuicing()
        {
            active = false;
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
    
    GetComponent<FlashJuicer>().StartJuicing(); // Behaviour of this depends on blockIfActive and updateStartValues parameters
    GetComponent<FlashJuicer>().StopJuicing();
    
*/
